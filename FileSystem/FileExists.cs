using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.FileSystem;

/// <summary>
/// Returns whether a file on the file system exists.
/// </summary>
[Alias("DoesFileExist")]
public class FileExists : CompoundStep<bool>
{
    /// <summary>
    /// The path to the file to check.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("File")]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<bool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var pathResult = await Path.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (pathResult.IsFailure)
            return pathResult.ConvertFailure<bool>();

        var fileSystemResult =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemResult.IsFailure)
            return fileSystemResult.MapError(x => x.WithLocation(this)).ConvertFailure<bool>();

        try
        {
            var r = fileSystemResult.Value.File.Exists(pathResult.Value);
            return r;
        }
        catch (Exception e)
        {
            return new SingleError(new ErrorLocation(this), e, ErrorCode.ExternalProcessError);
        }
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<FileExists, bool>();
}

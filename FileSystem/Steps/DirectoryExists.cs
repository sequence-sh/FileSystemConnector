using Sequence.Core.Internal.Errors;

namespace Sequence.Connectors.FileSystem.Steps;

/// <summary>
/// Returns whether a directory on the file system exists.
/// </summary>
[Alias("DoesDirectoryExist")]
public class DirectoryExists : CompoundStep<SCLBool>
{
    /// <summary>
    /// The path to the folder to check.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("Directory")]
    [Metadata("Path", "Read")]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <inheritdoc />
    protected override async ValueTask<Result<SCLBool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var pathResult = await Path.Run(stateMonad, cancellationToken);

        if (pathResult.IsFailure)
            return pathResult.ConvertFailure<SCLBool>();

        var pathString = await pathResult.Value.GetStringAsync();

        var fileSystemResult =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemResult.IsFailure)
            return fileSystemResult.MapError(x => x.WithLocation(this)).ConvertFailure<SCLBool>();

        try
        {
            var r = fileSystemResult.Value.Directory.Exists(pathString);
            return r.ConvertToSCLObject();
        }
        catch (Exception e)
        {
            return new SingleError(new ErrorLocation(this), e, ErrorCode.ExternalProcessError);
        }
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<DirectoryExists, SCLBool>();
}

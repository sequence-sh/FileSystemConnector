using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.FileSystem;

/// <summary>
/// Creates a new directory in the file system.
/// Will create all directories and subdirectories in the specified path unless they already exist.
/// </summary>
[Alias("mkdir")]
[Alias("CreateDirectory")]
public class DirectoryCreate : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var pathResult = await Path.Run(stateMonad, cancellationToken);

        if (pathResult.IsFailure)
            return pathResult.ConvertFailure<Unit>();

        var pathString = await pathResult.Value.GetStringAsync();

        var fileSystemResult =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemResult.IsFailure)
            return fileSystemResult.MapError(x => x.WithLocation(this)).ConvertFailure<Unit>();

        try
        {
            fileSystemResult.Value.Directory.CreateDirectory(pathString);
        }
        catch (Exception e)
        {
            return new SingleError(new ErrorLocation(this), e, ErrorCode.ExternalProcessError);
        }

        return Unit.Default;
    }

    /// <summary>
    /// The path to the directory to create.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<DirectoryCreate, Unit>();
}

using Reductech.Sequence.Core.Internal.Errors;
using Reductech.Sequence.Core.Internal.Logging;

namespace Reductech.Sequence.Connectors.FileSystem.Steps;

/// <summary>
/// Deletes a file or folder from the file system.
/// </summary>
[Alias("Delete")]
public class DeleteItem : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var pathResult = await Path.Run(stateMonad, cancellationToken);

        if (pathResult.IsFailure)
            return pathResult.ConvertFailure<Unit>();

        var path = await pathResult.Value.GetStringAsync();

        var fileSystemResult =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemResult.IsFailure)
            return fileSystemResult.MapError(x => x.WithLocation(this)).ConvertFailure<Unit>();

        try
        {
            if (fileSystemResult.Value.Directory.Exists(path))
            {
                fileSystemResult.Value.Directory.Delete(path, true);
                LogSituation.DirectoryDeleted.Log(stateMonad, this, path);
            }
            else if (fileSystemResult.Value.File.Exists(path))
            {
                fileSystemResult.Value.File.Delete(path);
                LogSituation.FileDeleted.Log(stateMonad, this, path);
            }
            else
            {
                LogSituation.ItemToDeleteDidNotExist.Log(stateMonad, this, path);
            }

            return Unit.Default;
        }
        catch (Exception e)
        {
            return new SingleError(new ErrorLocation(this), e, ErrorCode.ExternalProcessError);
        }
    }

    /// <summary>
    /// The path to the file or folder to delete.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("File")]
    [Alias("Folder")]
    [Metadata("Path", "Write")]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<DeleteItem, Unit>();
}

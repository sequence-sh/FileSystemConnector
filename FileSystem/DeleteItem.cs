using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Logging;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.FileSystem
{

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

        Result<Unit, IErrorBuilder> result;

        if (stateMonad.ExternalContext.FileSystemHelper.Directory.Exists(path))
        {
            result = stateMonad.ExternalContext.FileSystemHelper.DeleteDirectory(path, true);
            LogSituation.DirectoryDeleted.Log(stateMonad, this, path);
        }
        else if (stateMonad.ExternalContext.FileSystemHelper.File.Exists(path))
        {
            result = stateMonad.ExternalContext.FileSystemHelper.DeleteFile(path);
            LogSituation.FileDeleted.Log(stateMonad, this, path);
        }
        else
        {
            result = Unit.Default;
            LogSituation.ItemToDeleteDidNotExist.Log(stateMonad, this, path);
        }

        return result.MapError(x => x.WithLocation(this));
    }

    /// <summary>
    /// The path to the file or folder to delete.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("File")]
    [Alias("Folder")]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<DeleteItem, Unit>();
}

}

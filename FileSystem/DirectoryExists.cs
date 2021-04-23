using System;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.FileSystem
{

/// <summary>
/// Returns whether a directory on the file system exists.
/// </summary>
[Alias("DoesDirectoryExist")]
public class DirectoryExists : CompoundStep<bool>
{
    /// <summary>
    /// The path to the folder to check.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("Directory")]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<bool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var pathResult = await Path.Run(stateMonad, cancellationToken);

        if (pathResult.IsFailure)
            return pathResult.ConvertFailure<bool>();

        var pathString = await pathResult.Value.GetStringAsync();

        var fileSystemResult =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemResult.IsFailure)
            return fileSystemResult.MapError(x => x.WithLocation(this)).ConvertFailure<bool>();

        try
        {
            var r = fileSystemResult.Value.Directory.Exists(pathString);
            return r;
        }
        catch (Exception e)
        {
            return new SingleError(new ErrorLocation(this), e, ErrorCode.ExternalProcessError);
        }
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<DirectoryExists, bool>();
}

}

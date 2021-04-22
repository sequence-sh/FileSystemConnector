using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.FileSystem
{

/// <summary>
/// Move a directory
/// </summary>
public class DirectoryMove : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var source = await SourceDirectory.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (source.IsFailure)
            return source.ConvertFailure<Unit>();

        var destination = await DestinationDirectory.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (destination.IsFailure)
            return destination.ConvertFailure<Unit>();

        stateMonad.ExternalContext.FileSystemHelper.Directory.Move(source.Value, destination.Value);

        return Unit.Default;
    }

    /// <summary>
    /// The source directory name
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> SourceDirectory { get; set; } = null!;

    /// <summary>
    /// The destination directory name
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<StringStream> DestinationDirectory { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<DirectoryMove, Unit>();
}

}

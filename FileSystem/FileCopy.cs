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
/// Copy a file
/// </summary>
public class FileCopy : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var source = await SourceFile.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (source.IsFailure)
            return source.ConvertFailure<Unit>();

        var destination = await DestinationFile.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (destination.IsFailure)
            return destination.ConvertFailure<Unit>();

        var overwrite = await Overwrite.Run(stateMonad, cancellationToken);

        if (overwrite.IsFailure)
            return overwrite.ConvertFailure<Unit>();

        stateMonad.ExternalContext.FileSystemHelper.File.Copy(
            source.Value,
            destination.Value,
            overwrite.Value
        );

        return Unit.Default;
    }

    /// <summary>
    /// The source file name
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> SourceFile { get; set; } = null!;

    /// <summary>
    /// The destination file name
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<StringStream> DestinationFile { get; set; } = null!;

    /// <summary>
    /// True if the destination file can be overwritten
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("false")]
    public IStep<bool> Overwrite { get; set; } = new BoolConstant(false);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<FileCopy, Unit>();
}

}

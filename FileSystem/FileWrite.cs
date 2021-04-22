using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.FileSystem
{

/// <summary>
/// Writes a file to the local file system.
/// </summary>
[Alias("WriteToFile")]
public sealed class FileWrite : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var path = await Path.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (path.IsFailure)
            return path.ConvertFailure<Unit>();

        var stringStreamResult = await Stream.Run(stateMonad, cancellationToken);

        if (stringStreamResult.IsFailure)
            return stringStreamResult.ConvertFailure<Unit>();

        var compressResult = await Compress.Run(stateMonad, cancellationToken);

        if (compressResult.IsFailure)
            return compressResult.ConvertFailure<Unit>();

        var stream = stringStreamResult.Value.GetStream().stream;

        var r = await stateMonad.ExternalContext.FileSystemHelper
            .WriteFileAsync(
                path.Value,
                stream,
                compressResult.Value,
                cancellationToken
            )
            .MapError(x => x.WithLocation(this));

        stream.Dispose();

        return r;
    }

    /// <summary>
    /// The data to write to file.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("Data")]
    public IStep<StringStream> Stream { get; set; } = null!;

    /// <summary>
    /// The path of the file to write to.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <summary>
    /// Whether to compress the data when writing the file.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("false")]
    public IStep<bool> Compress { get; set; } = new BoolConstant(false);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<FileWrite, Unit>();
}

}

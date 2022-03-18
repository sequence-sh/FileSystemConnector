using Reductech.Sequence.Core.Internal.Errors;

namespace Reductech.Sequence.Connectors.FileSystem.Steps;

/// <summary>
/// Extract a file in the file system.
/// </summary>
[Alias("ExtractFile")]
public class FileExtract : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var archivePathResult = await ArchiveFilePath.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (archivePathResult.IsFailure)
            return archivePathResult.ConvertFailure<Unit>();

        var destinationResult = await Destination.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (destinationResult.IsFailure)
            return destinationResult.ConvertFailure<Unit>();

        var overwriteResult = await Overwrite.Run(stateMonad, cancellationToken);

        if (overwriteResult.IsFailure)
            return overwriteResult.ConvertFailure<Unit>();

        var compressionResult =
            stateMonad.ExternalContext.TryGetContext<ICompression>(
                ConnectorInjection.CompressionKey
            );

        if (compressionResult.IsFailure)
            return compressionResult.MapError(x => x.WithLocation(this)).ConvertFailure<Unit>();

        try
        {
            compressionResult.Value.ExtractToDirectory(
                archivePathResult.Value,
                destinationResult.Value,
                overwriteResult.Value
            );
        }
        catch (Exception e)
        {
            return new SingleError(new ErrorLocation(this), e, ErrorCode.ExternalProcessError);
        }

        return Unit.Default;
    }

    /// <summary>
    /// The path to the archive to extract.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("Container")]
    [Log(LogOutputLevel.Trace)]
    [Metadata("Path", "Read")]
    public IStep<StringStream> ArchiveFilePath { get; set; } = null!;

    /// <summary>
    /// The directory to extract to.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("ToDirectory")]
    [Log(LogOutputLevel.Trace)]
    [Metadata("Path", "Write")]
    public IStep<StringStream> Destination { get; set; } = null!;

    /// <summary>
    /// Whether to overwrite files when extracting.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("false")]
    public IStep<SCLBool> Overwrite { get; set; } = new SCLConstant<SCLBool>(SCLBool.False);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<FileExtract, Unit>();
}

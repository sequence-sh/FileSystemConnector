using Reductech.Sequence.Core.Internal.Errors;

namespace Reductech.Sequence.Connectors.FileSystem.Steps;

/// <summary>
/// Move a file
/// </summary>
public class FileMove : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
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

        var fileSystemResult =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemResult.IsFailure)
            return fileSystemResult.MapError(x => x.WithLocation(this)).ConvertFailure<Unit>();

        try
        {
            fileSystemResult.Value.File.Move(source.Value, destination.Value);
        }
        catch (Exception e)
        {
            return new SingleError(new ErrorLocation(this), e, ErrorCode.ExternalProcessError);
        }

        return Unit.Default;
    }

    /// <summary>
    /// The source file name
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Metadata("Path", "Read")]
    public IStep<StringStream> SourceFile { get; set; } = null!;

    /// <summary>
    /// The destination file name
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Metadata("Path", "Write")]
    public IStep<StringStream> DestinationFile { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<FileMove, Unit>();
}

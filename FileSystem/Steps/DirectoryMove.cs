using Reductech.Sequence.Core.Internal.Errors;

namespace Reductech.Sequence.Connectors.FileSystem.Steps;

/// <summary>
/// Move a directory
/// </summary>
public class DirectoryMove : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
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

        var fileSystemResult =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemResult.IsFailure)
            return fileSystemResult.MapError(x => x.WithLocation(this)).ConvertFailure<Unit>();

        try
        {
            fileSystemResult.Value.Directory.Move(source.Value, destination.Value);
        }
        catch (Exception e)
        {
            return new SingleError(new ErrorLocation(this), e, ErrorCode.ExternalProcessError);
        }

        return Unit.Default;
    }

    /// <summary>
    /// The source directory name
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("Source")]
    [Metadata("Path", "Read")]
    public IStep<StringStream> SourceDirectory { get; set; } = null!;

    /// <summary>
    /// The destination directory name
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("Destination")]
    [Metadata("Path", "Write")]
    public IStep<StringStream> DestinationDirectory { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<DirectoryMove, Unit>();
}

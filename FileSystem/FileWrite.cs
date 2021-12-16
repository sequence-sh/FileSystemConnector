using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.FileSystem;

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

        var fileSystemResult =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemResult.IsFailure)
            return fileSystemResult.MapError(x => x.WithLocation(this))
                .ConvertFailure<Unit>();

        var r = await WriteFileAsync(
                fileSystemResult.Value,
                path.Value,
                stream,
                compressResult.Value,
                cancellationToken
            )
            .MapError(x => x.WithLocation(this));

        await stream.DisposeAsync();

        return r;
    }

    private static async Task<Result<Unit, IErrorBuilder>> WriteFileAsync(
        IFileSystem fileSystem,
        string path,
        Stream stream,
        bool compress,
        CancellationToken cancellationToken)
    {
        Maybe<IErrorBuilder> error;

        try
        {
            var writeStream = fileSystem.File.Create(path);

            if (compress)
            {
                writeStream =
                    new System.IO.Compression.GZipStream(
                        writeStream,
                        System.IO.Compression.CompressionMode.Compress
                    );
            }

            await stream.CopyToAsync(writeStream, cancellationToken);
            writeStream.Close();
            error = Maybe<IErrorBuilder>.None;
        }
        catch (Exception e)
        {
            error = Maybe<IErrorBuilder>.From(
                ErrorCode.ExternalProcessError.ToErrorBuilder(e.Message)
            );
        }

        if (error.HasValue)
            return Result.Failure<Unit, IErrorBuilder>(error.Value);

        return Unit.Default;
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
    public IStep<SCLBool> Compress { get; set; } = new SCLConstant<SCLBool>(SCLBool.False);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<FileWrite, Unit>();
}

using System.Text;
using Reductech.Sequence.Core.Enums;
using Reductech.Sequence.Core.Internal.Errors;

namespace Reductech.Sequence.Connectors.FileSystem.Steps;

/// <summary>
/// Writes a file to the local file system.
/// </summary>
[Alias("WriteToFile")]
public sealed class FileWrite : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var argsResult = await stateMonad.RunStepsAsync(
            Path.WrapStringStream(),
            Stream,
            Compress,
            Encoding,
            cancellationToken
        );

        if (argsResult.IsFailure)
            return argsResult.ConvertFailure<Unit>();

        var (path, stringStream, compress, writeEncoding) = argsResult.Value;

        var (stream, readEncoding) = stringStream.GetStream();

        var fileSystemResult =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemResult.IsFailure)
            return fileSystemResult.MapError(x => x.WithLocation(this))
                .ConvertFailure<Unit>();

        var r = await WriteFileAsync(
                fileSystemResult.Value,
                path,
                stream,
                readEncoding.Convert(),
                writeEncoding.Value.Convert(),
                compress.Value,
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
        Encoding readEncoding,
        Encoding writeEncoding,
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

            var readStream = stream;

            if (!readEncoding.Equals(writeEncoding))
                readStream = System.Text.Encoding.CreateTranscodingStream(
                    readStream,
                    readEncoding,
                    writeEncoding
                );

            await readStream.CopyToAsync(writeStream, cancellationToken);
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
    [Metadata("Path", "Write")]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <summary>
    /// Whether to compress the data when writing the file.
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("false")]
    public IStep<SCLBool> Compress { get; set; } = new SCLConstant<SCLBool>(SCLBool.False);

    /// <summary>
    /// The encoding to use to write the file
    /// </summary>
    [StepProperty(4)]
    [DefaultValueExplanation("UTF8 no BOM")]
    public IStep<SCLEnum<EncodingEnum>> Encoding { get; set; } =
        new SCLConstant<SCLEnum<EncodingEnum>>(new SCLEnum<EncodingEnum>(EncodingEnum.UTF8));

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<FileWrite, Unit>();
}

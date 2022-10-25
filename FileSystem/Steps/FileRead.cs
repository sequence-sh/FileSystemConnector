using Reductech.Sequence.Core.Enums;
using Reductech.Sequence.Core.Internal.Errors;

namespace Reductech.Sequence.Connectors.FileSystem.Steps;

/// <summary>
/// Reads text from a file.
/// </summary>
[Alias("ReadFromFile")]
public sealed class FileRead : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var path = await Path.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (path.IsFailure)
            return path.ConvertFailure<StringStream>();

        var encoding = await Encoding.Run(stateMonad, cancellationToken);

        if (encoding.IsFailure)
            return encoding.ConvertFailure<StringStream>();

        var decompress = await Decompress.Run(stateMonad, cancellationToken);

        if (decompress.IsFailure)
            return decompress.ConvertFailure<StringStream>();

        var fileSystemResult =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemResult.IsFailure)
            return fileSystemResult.MapError(x => x.WithLocation(this))
                .ConvertFailure<StringStream>();

        try
        {
            var fs = fileSystemResult.Value.File.OpenRead(path.Value);

            if (decompress.Value)
            {
                fs = new System.IO.Compression.GZipStream(
                    fs,
                    System.IO.Compression.CompressionMode.Decompress
                );
            }

            var stringStream = new StringStream(fs, encoding.Value);

            return stringStream;
        }
        catch (Exception e)
        {
            return new SingleError(new ErrorLocation(this), e, ErrorCode.ExternalProcessError);
        }
    }

    /// <summary>
    /// The name of the file to read.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Log(LogOutputLevel.Trace)]
    [Metadata("Path", "Read")]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <summary>
    /// How the file is encoded.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("UTF8 no BOM")]
    public IStep<SCLEnum<EncodingEnum>> Encoding { get; set; } =
        new SCLConstant<SCLEnum<EncodingEnum>>(new SCLEnum<EncodingEnum>(EncodingEnum.UTF8));

    /// <summary>
    /// Whether to decompress this string
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("false")]
    public IStep<SCLBool> Decompress { get; set; } = new SCLConstant<SCLBool>(SCLBool.False);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<FileRead, StringStream>();
}

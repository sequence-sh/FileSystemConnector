using Sequence.Connectors.FileSystem.Steps;

namespace Sequence.Connectors.FileSystem.Tests;

public partial class FileExtractTests : StepTestBase<FileExtract, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "FileExtract",
                new FileExtract
                {
                    ArchiveFilePath = Constant("Foo"),
                    Destination     = Constant("Bar"),
                    Overwrite       = Constant(true)
                },
                Unit.Default
            ).WithCompressionMock(x => x.Setup(c => c.ExtractToDirectory("Foo", "Bar", true)));
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "ICompression Error",
                new FileExtract
                {
                    ArchiveFilePath = Constant("Archive"), Destination = Constant("Destination")
                },
                new ErrorBuilder(ErrorCode.MissingContext, "ICompression")
            );

            yield return new ErrorCase(
                "ICompression.ExtractToDirectory Error",
                new FileExtract
                {
                    ArchiveFilePath = Constant("Archive"), Destination = Constant("Destination")
                },
                new ErrorBuilder(
                    new Exception("Ultimate Test Exception"),
                    ErrorCode.ExternalProcessError
                )
            ).WithCompressionMock(
                x => x.Setup(c => c.ExtractToDirectory("Archive", "Destination", false))
                    .Throws(new Exception("Ultimate Test Exception"))
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

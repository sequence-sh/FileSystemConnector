namespace Reductech.Sequence.Connectors.FileSystem.Tests;

public partial class FileCopyTests : StepTestBase<FileCopy, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Copy File",
                    new FileCopy()
                    {
                        SourceFile      = StaticHelpers.Constant("MySource"),
                        DestinationFile = StaticHelpers.Constant("MyDestination"),
                        Overwrite       = StaticHelpers.Constant(true)
                    },
                    Unit.Default
                )
                .WithFileSystem(initialFiles: new[] { ("MySource", "abc") })
                .WithExpectedFileSystem(
                    expectedFinalFiles: new[] { ("/MySource", "abc"), ("/MyDestination", "abc") }
                );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "IFileSystem Error",
                new FileCopy
                {
                    SourceFile = Constant("Source"), DestinationFile = Constant("Destination")
                },
                new ErrorBuilder(ErrorCode.MissingContext, "IFileSystem")
            );

            yield return new ErrorCase(
                "File.Copy Error",
                new FileCopy
                {
                    SourceFile = Constant("Source"), DestinationFile = Constant("Destination")
                },
                new ErrorBuilder(
                    new Exception("Ultimate Test Exception"),
                    ErrorCode.ExternalProcessError
                )
            ).WithFileSystemMock(
                x => x.Setup(fs => fs.File.Copy("Source", "Destination", false))
                    .Throws(new Exception("Ultimate Test Exception"))
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

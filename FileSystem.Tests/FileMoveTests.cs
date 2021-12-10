namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class FileMoveTests : StepTestBase<FileMove, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Move File",
                    new FileMove()
                    {
                        SourceFile      = Constant("MySourceFile.txt"),
                        DestinationFile = Constant("MyDestinationFile.txt")
                    },
                    Unit.Default
                ).WithFileSystem(initialFiles: new[] { ("MySourceFile.txt", "abc") })
                .WithExpectedFileSystem(
                    expectedFinalFiles: new[] { ("/MyDestinationFile.txt", "abc") }
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
                new FileMove
                {
                    SourceFile = Constant("Source"), DestinationFile = Constant("Destination")
                },
                new ErrorBuilder(ErrorCode.MissingContext, "IFileSystem")
            );

            yield return new ErrorCase(
                "File.Move Error",
                new FileMove
                {
                    SourceFile = Constant("Source"), DestinationFile = Constant("Destination")
                },
                new ErrorBuilder(
                    new Exception("Ultimate Test Exception"),
                    ErrorCode.ExternalProcessError
                )
            ).WithFileSystemMock(
                x => x.Setup(fs => fs.File.Move("Source", "Destination"))
                    .Throws(new Exception("Ultimate Test Exception"))
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

}

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class DirectoryMoveTests : StepTestBase<DirectoryMove, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Move Directory",
                    new DirectoryMove
                    {
                        SourceDirectory      = StaticHelpers.Constant("MySource"),
                        DestinationDirectory = StaticHelpers.Constant("MyDestination")
                    },
                    Unit.Default
                ).WithFileSystem(initialDirectories: new[] { "MySource" })
                .WithExpectedFileSystem(expectedFinalDirectories: new[] { "MyDestination" });
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "IFileSystem Error",
                new DirectoryMove
                {
                    SourceDirectory      = Constant("Source"),
                    DestinationDirectory = Constant("Destination")
                },
                new ErrorBuilder(ErrorCode.MissingContext, "IFileSystem")
            );

            yield return new ErrorCase(
                "Directory.Move Error",
                new DirectoryMove
                {
                    SourceDirectory      = Constant("Source"),
                    DestinationDirectory = Constant("Destination")
                },
                new ErrorBuilder(
                    new Exception("Ultimate Test Exception"),
                    ErrorCode.ExternalProcessError
                )
            ).WithFileSystemMock(
                x => x.Setup(fs => fs.Directory.Move("Source", "Destination"))
                    .Throws(new Exception("Ultimate Test Exception"))
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

}

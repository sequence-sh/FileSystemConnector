namespace Reductech.Sequence.Connectors.FileSystem.Tests;

public partial class DirectoryCopyTests : StepTestBase<DirectoryCopy, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Copy Directory",
                    new DirectoryCopy
                    {
                        SourceDirectory      = Constant("/MySource"),
                        DestinationDirectory = Constant("/MyDestination"),
                        Overwrite            = Constant(true),
                        CopySubDirectories   = Constant(true)
                    },
                    Unit.Default
                )
                .WithFileSystem(
                    initialFiles: new[]
                    {
                        ("MySource/f1", "a"), ("MySource/f2", "b"), ("MySource/sub/f3", "c")
                    }
                )
                .WithExpectedFileSystem(
                    new[]
                    {
                        ("/MySource/f1", "a"), ("/MySource/f2", "b"), ("/MySource/sub/f3", "c"),
                        ("/MyDestination/f1", "a"), ("/MyDestination/f2", "b"),
                        ("/MyDestination/sub/f3", "c")
                    },
                    new[] { "MySource", "MyDestination" }
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
                new DirectoryCopy
                {
                    SourceDirectory      = Constant("Source"),
                    DestinationDirectory = Constant("Destination")
                },
                new ErrorBuilder(ErrorCode.MissingContext, "IFileSystem")
            );

            yield return new ErrorCase(
                "Directory does not exist",
                new DirectoryCopy
                {
                    SourceDirectory      = Constant("Source"),
                    DestinationDirectory = Constant("Destination")
                },
                ErrorCode.DirectoryNotFound.ToErrorBuilder("Source")
            ).WithFileSystemMock(x => x.Setup(fs => fs.Directory.Exists("Source")).Returns(false));

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

using Reductech.Sequence.Connectors.FileSystem.Steps;

namespace Reductech.Sequence.Connectors.FileSystem.Tests;

public partial class DirectoryCreateTests : StepTestBase<DirectoryCreate, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Create Directory",
                    new DirectoryCreate { Path = Constant("MyPath") },
                    Unit.Default
                ).WithFileSystem()
                .WithExpectedFileSystem(expectedFinalDirectories: new List<string>() { "MyPath" });
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                    "Create Directory",
                    "CreateDirectory Path: 'MyPath'",
                    Unit.Default
                )
                .WithFileSystem()
                .WithExpectedFileSystem(expectedFinalDirectories: new List<string>() { "MyPath" });
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "IFileSystem Error",
                new DirectoryCreate { Path = Constant("My Path") },
                new ErrorBuilder(ErrorCode.MissingContext, "IFileSystem")
            );

            yield return new ErrorCase(
                "Error returned",
                new DirectoryCreate { Path = Constant("MyPath") },
                new ErrorBuilder(
                    new Exception("Ultimate Test Exception"),
                    ErrorCode.ExternalProcessError
                )
            ).WithFileSystemMock(
                x => x.Setup(fs => fs.Directory.CreateDirectory("MyPath"))
                    .Throws(new Exception("Ultimate Test Exception"))
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

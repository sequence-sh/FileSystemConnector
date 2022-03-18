using Reductech.Sequence.Connectors.FileSystem.Steps;

namespace Reductech.Sequence.Connectors.FileSystem.Tests;

public partial class DirectoryExistsTests : StepTestBase<DirectoryExists, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Directory Exists",
                    new DirectoryExists { Path = Constant("My Path") },
                    true.ConvertToSCLObject()
                ).WithFileSystem(initialDirectories: new[] { "My Path" })
                .WithExpectedFileSystem(expectedFinalDirectories: new[] { "My Path" });

            yield return new StepCase(
                    "Directory Does not exist",
                    new DirectoryExists { Path = Constant("My Path") },
                    false.ConvertToSCLObject()
                ).WithFileSystem()
                .WithExpectedFileSystem();
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "IFileSystem Error",
                new DirectoryExists { Path = Constant("My Path") },
                new ErrorBuilder(ErrorCode.MissingContext, "IFileSystem")
            );

            yield return new ErrorCase(
                "Directory.Exists Error",
                new DirectoryExists { Path = Constant("MyPath") },
                new ErrorBuilder(
                    new Exception("Ultimate Test Exception"),
                    ErrorCode.ExternalProcessError
                )
            ).WithFileSystemMock(
                x => x.Setup(fs => fs.Directory.Exists("MyPath"))
                    .Throws(new Exception("Ultimate Test Exception"))
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

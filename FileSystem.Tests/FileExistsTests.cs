namespace Reductech.Sequence.Connectors.FileSystem.Tests;

public partial class FileExistsTests : StepTestBase<FileExists, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "File exists",
                    new FileExists { Path = Constant("MyFile.txt") },
                    true.ConvertToSCLObject()
                ).WithFileSystem(initialFiles: new[] { ("MyFile.txt", "abc") })
                .WithExpectedFileSystem(expectedFinalFiles: new[] { ("/MyFile.txt", "abc") });

            yield return new StepCase(
                    "File does not exist",
                    new FileExists { Path = Constant("MyFile.txt") },
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
                new FileExists { Path = Constant("My Path") },
                new ErrorBuilder(ErrorCode.MissingContext, "IFileSystem")
            );

            yield return new ErrorCase(
                "File.Exists Error",
                new FileExists { Path = Constant("MyPath") },
                new ErrorBuilder(
                    new Exception("Ultimate Test Exception"),
                    ErrorCode.ExternalProcessError
                )
            ).WithFileSystemMock(
                x => x.Setup(fs => fs.File.Exists("MyPath"))
                    .Throws(new Exception("Ultimate Test Exception"))
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

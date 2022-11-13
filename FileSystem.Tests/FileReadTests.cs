using Sequence.Connectors.FileSystem.Steps;
using Sequence.Core.Steps;

namespace Sequence.Connectors.FileSystem.Tests;

public partial class FileReadTests : StepTestBase<FileRead, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Log file text",
                    new Log { Value = new FileRead { Path = Constant("File.txt"), } },
                    Unit.Default,
                    "Hello World"
                ).WithFileSystem(initialFiles: new[] { ("File.txt", "Hello World") })
                .WithExpectedFileSystem(expectedFinalFiles: new[] { ("/File.txt", "Hello World") });

            //NOTE: Compression is tested in FileWriteTests
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                    "Default",
                    "Log Value: (FileRead Path: 'File.txt')",
                    Unit.Default,
                    "Hello World"
                ).WithFileSystem(initialFiles: new[] { ("File.txt", "Hello World") })
                .WithExpectedFileSystem(expectedFinalFiles: new[] { ("/File.txt", "Hello World") });

            yield return new DeserializeCase(
                    "Ordered Args",
                    "Log (FileRead 'File.txt')",
                    Unit.Default,
                    "Hello World"
                ).WithFileSystem(initialFiles: new[] { ("File.txt", "Hello World") })
                .WithExpectedFileSystem(expectedFinalFiles: new[] { ("/File.txt", "Hello World") });

            yield return new DeserializeCase(
                    "Alias",
                    "Log Value: (ReadFromFile Path: 'File.txt')",
                    Unit.Default,
                    "Hello World"
                ).WithFileSystem(initialFiles: new[] { ("File.txt", "Hello World") })
                .WithExpectedFileSystem(expectedFinalFiles: new[] { ("/File.txt", "Hello World") });
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "IFileSystem Error",
                new FileRead { Path = Constant("File.txt") },
                new ErrorBuilder(ErrorCode.MissingContext, "IFileSystem")
            );

            yield return new ErrorCase(
                "File Read Error",
                new FileRead { Path = Constant("File.txt") },
                new ErrorBuilder(
                    new Exception("Ultimate Test Exception"),
                    ErrorCode.ExternalProcessError
                )
            ).WithFileSystemMock(
                x => x.Setup(fs => fs.File.OpenRead("File.txt"))
                    .Throws(new Exception("Ultimate Test Exception"))
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

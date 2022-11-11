using System.Text;
using Sequence.Connectors.FileSystem.Steps;
using Sequence.Core.Enums;
using Sequence.Core.Steps;

namespace Sequence.Connectors.FileSystem.Tests;

public partial class FileWriteTests : StepTestBase<FileWrite, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Write file",
                    new FileWrite
                    {
                        Path = Constant("Filename.txt"), Stream = Constant("Hello World 😀")
                    },
                    Unit.Default
                ).WithFileSystem()
                .WithExpectedFileSystem(
                    expectedFinalFiles: new[] { ("/Filename.txt", "Hello World 😀") }
                );

            yield return new StepCase(
                    "Write file with encoding",
                    new FileWrite
                    {
                        Path     = Constant("Filename.txt"),
                        Stream   = Constant("Hello World 😀"),
                        Encoding = Constant(EncodingEnum.UTF32)
                    },
                    Unit.Default
                ).WithFileSystem()
                .WithExpectedFileSystem(
                    new[] { ("/Filename.txt", "Hello World 😀", Encoding.UTF32) }!
                );

            yield return new StepCase(
                    "Write file compressed",
                    new Sequence<Unit>()
                    {
                        InitialSteps = new List<IStep<Unit>>()
                        {
                            new FileWrite
                            {
                                Path     = Constant("Filename.txt"),
                                Stream   = Constant("Hello World"),
                                Compress = Constant(true)
                            },
                            new Log()
                            {
                                Value = new FileRead()
                                {
                                    Path       = Constant("Filename.txt"),
                                    Decompress = Constant(true)
                                }
                            },
                            new DeleteItem() //Must delete the file so it's not in the final state
                            {
                                Path = Constant("Filename.txt"),
                            }
                        },
                        FinalStep = new DoNothing()
                    },
                    Unit.Default,
                    "Hello World"
                )
                .WithFileSystem()
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
                new FileWrite { Stream = Constant("Data"), Path = Constant("Destination") },
                new ErrorBuilder(ErrorCode.MissingContext, "IFileSystem")
            );

            yield return new ErrorCase(
                "File.Create Error",
                new FileWrite { Stream = Constant("Data"), Path = Constant("Destination") },
                ErrorCode.ExternalProcessError.ToErrorBuilder("Ultimate Test Exception")
            ).WithFileSystemMock(
                x => x.Setup(fs => fs.File.Create("Destination"))
                    .Throws(new Exception("Ultimate Test Exception"))
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

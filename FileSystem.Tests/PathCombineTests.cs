using System.IO;
using Reductech.Sequence.Core.Steps;

namespace Reductech.Sequence.Connectors.FileSystem.Tests;

public partial class PathCombineTests : StepTestBase<PathCombine, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var currentDirectory = Environment.CurrentDirectory;

            var expected = Path.Combine(currentDirectory, "Hello", "World");

            yield return new StepCase(
                    "Non Relative",
                    new PathCombine
                    {
                        Paths = new ArrayNew<StringStream>
                        {
                            Elements = new List<IStep<StringStream>>()
                            {
                                Constant(currentDirectory),
                                Constant("Hello"),
                                Constant("World")
                            }
                        }
                    },
                    expected
                ).WithFileSystem()
                .WithExpectedFileSystem();

            yield return new StepCase(
                    "Relative",
                    new PathCombine
                    {
                        Paths = new ArrayNew<StringStream>
                        {
                            Elements = new List<IStep<StringStream>>()
                            {
                                Constant("Hello"), Constant("World")
                            }
                        }
                    },
                    expected
                )
                .WithFileSystem(currentDirectory: currentDirectory)
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
                new PathCombine { Paths = Array("Directory", "File.txt") },
                new ErrorBuilder(ErrorCode.MissingContext, "IFileSystem")
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

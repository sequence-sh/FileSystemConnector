﻿using Sequence.Connectors.FileSystem.Steps;

namespace Sequence.Connectors.FileSystem.Tests;

public partial class DeleteItemTests : StepTestBase<DeleteItem, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            //Note log messages are ignored as they are debug messages
            yield return new StepCase(
                        "Delete Directory",
                        new DeleteItem { Path = Constant("My Path") },
                        Unit.Default
                        //, "Directory 'My Path' Deleted."
                    ).WithFileSystem(initialDirectories: new List<string> { "My Path" })
                    .WithExpectedFileSystem()
                ;

            yield return new StepCase(
                    "Delete File",
                    new DeleteItem { Path = Constant("My Path") },
                    Unit.Default
                    // , "File 'My Path' Deleted."
                ).WithFileSystem(new[] { ("My Path", "abcd") })
                .WithExpectedFileSystem();

            yield return new StepCase(
                    "Item does not exist",
                    new DeleteItem { Path = Constant("My Path") },
                    Unit.Default
                    //, "Item 'My Path' did not exist."
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
                new DeleteItem { Path = Constant("My Path") },
                new ErrorBuilder(ErrorCode.MissingContext, "IFileSystem")
            );

            yield return new ErrorCase(
                "Could not delete file",
                new DeleteItem { Path = Constant("My Path") },
                new ErrorBuilder(
                    new Exception("Ultimate Test Exception"),
                    ErrorCode.ExternalProcessError
                )
            ).WithFileSystemMock(
                x =>
                {
                    x.Setup(fs => fs.Directory.Exists("My Path")).Returns(true);

                    x.Setup(fs => fs.Directory.Delete("My Path", true))
                        .Throws(new Exception("Ultimate Test Exception"));
                }
            );

            foreach (var ec in base.ErrorCases)
                yield return ec;
        }
    }
}

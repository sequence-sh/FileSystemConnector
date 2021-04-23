using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

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
                        "Could not delete file",
                        new DeleteItem { Path = Constant("My Path") },
                        new ErrorBuilder(
                            new Exception("Ultimate Test Exception"),
                            ErrorCode.ExternalProcessError
                        )
                    )
                    .WithFileSystemMock(
                        x =>
                        {
                            x.Setup(fs => fs.Directory.Exists("My Path")).Returns(true);

                            x.Setup(fs => fs.Directory.Delete("My Path", true))
                                .Throws(new Exception("Ultimate Test Exception"));
                        }
                    )
                ;
        }
    }
}

}

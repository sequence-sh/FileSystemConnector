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
            yield return new StepCase(
                        "Delete Directory",
                        new DeleteItem { Path = Constant("My Path") },
                        Unit.Default
                        //, "Directory 'My Path' Deleted."
                    )
                    .WithDirectoryAction(x => x.Setup(a => a.Exists("My Path")).Returns(true))
                    .WithDirectoryAction(x => x.Setup(a => a.Delete("My Path", true)))
                ;

            yield return new StepCase(
                    "Delete File",
                    new DeleteItem { Path = Constant("My Path") },
                    Unit.Default
                    // , "File 'My Path' Deleted."
                )
                .WithDirectoryAction(x => x.Setup(a => a.Exists("My Path")).Returns(false))
                .WithFileAction(x => x.Setup(a => a.Exists("My Path")).Returns(true))
                .WithFileAction(x => x.Setup(a => a.Delete("My Path")));

            yield return new StepCase(
                    "Item does not exist",
                    new DeleteItem { Path = Constant("My Path") },
                    Unit.Default
                    //, "Item 'My Path' did not exist."
                ).WithDirectoryAction(x => x.Setup(a => a.Exists("My Path")).Returns(false))
                .WithFileAction(x => x.Setup(a => a.Exists("My Path")).Returns(false));
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
                        ErrorCode.ExternalProcessError,
                        "Exception of type 'System.Exception' was thrown."
                    )
                )
                .WithDirectoryAction(x => x.Setup(a => a.Exists("My Path")).Returns(true))
                .WithDirectoryAction(
                    x => x.Setup(a => a.Delete("My Path", true)).Throws<Exception>()
                );
        }
    }
}

}

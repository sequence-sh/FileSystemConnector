using System.Collections.Generic;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class FileMoveTests : StepTestBase<FileMove, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Move File",
                    new FileMove()
                    {
                        SourceFile      = Constant("MySourceFile.txt"),
                        DestinationFile = Constant("MyDestinationFile.txt")
                    },
                    Unit.Default
                ).WithFileSystem(initialFiles: new[] { ("MySourceFile.txt", "abc") })
                .WithExpectedFileSystem(
                    expectedFinalFiles: new[] { ("C:\\MyDestinationFile.txt", "abc") }
                );
        }
    }
}

}

using System.Collections.Generic;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class DirectoryMoveTests : StepTestBase<DirectoryMove, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Move Directory",
                new DirectoryMove()
                {
                    SourceDirectory      = StaticHelpers.Constant("MySource"),
                    DestinationDirectory = StaticHelpers.Constant("MyDestination")
                },
                Unit.Default
            ).WithDirectoryAction(x => x.Setup(f => f.Move("MySource", "MyDestination")));
        }
    }
}

}

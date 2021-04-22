using System.Collections.Generic;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class FileCopyTests : StepTestBase<FileCopy, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Copy File",
                    new FileCopy()
                    {
                        SourceFile      = StaticHelpers.Constant("MySource"),
                        DestinationFile = StaticHelpers.Constant("MyDestination"),
                        Overwrite       = StaticHelpers.Constant(true)
                    },
                    Unit.Default
                )
                .WithFileAction(x => x.Setup(f => f.Copy("MySource", "MyDestination", true)));
        }
    }
}

}

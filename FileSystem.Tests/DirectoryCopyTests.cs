using System.Collections.Generic;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class DirectoryCopyTests : StepTestBase<DirectoryCopy, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Copy Directory",
                    new DirectoryCopy
                    {
                        SourceDirectory      = StaticHelpers.Constant("/MySource"),
                        DestinationDirectory = StaticHelpers.Constant("/MyDestination"),
                        Overwrite            = StaticHelpers.Constant(true),
                        CopySubDirectories   = StaticHelpers.Constant(true)
                    },
                    Unit.Default
                )
                .WithFileSystem(
                    initialFiles: new[]
                    {
                        ("MySource/f1", "a"), ("MySource/f2", "b"), ("MySource/sub/f3", "c")
                    }
                )
                .WithExpectedFileSystem(
                    new[]
                    {
                        ("/MySource/f1", "a"), ("/MySource/f2", "b"), ("/MySource/sub/f3", "c"),
                        ("/MyDestination/f1", "a"), ("/MyDestination/f2", "b"),
                        ("/MyDestination/sub/f3", "c")
                    },
                    new[] { "MySource", "MyDestination" }
                );
        }
    }
}

}

using System.Collections.Generic;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class DirectoryExistsTests : StepTestBase<DirectoryExists, bool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Directory Exists",
                    new DirectoryExists { Path = Constant("My Path") },
                    true
                )
                .WithDirectoryAction(x => x.Setup(a => a.Exists("My Path")).Returns(true));
        }
    }
}

}

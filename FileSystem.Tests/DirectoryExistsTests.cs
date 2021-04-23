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
                ).WithFileSystem(initialDirectories: new[] { "My Path" })
                .WithExpectedFileSystem(expectedFinalDirectories: new[] { "My Path" });

            yield return new StepCase(
                    "Directory Does not exist",
                    new DirectoryExists { Path = Constant("My Path") },
                    false
                ).WithFileSystem()
                .WithExpectedFileSystem();
        }
    }
}

}

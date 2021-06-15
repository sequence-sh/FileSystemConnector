using System.Collections.Generic;
using Reductech.EDR.Core;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class DirectoryGetItemsTests : StepTestBase<DirectoryGetItems, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Basic Case",
                new DirectoryGetItems() { Directory = StaticHelpers.Constant("") },
                new[]
                {
                    Entity.Create(("Name", "Alpha.txt"), ("Type", "File")),
                    Entity.Create(("Name", "temp"),      ("Type", "Directory"))
                }.ToSCLArray()
            ).WithFileSystem(initialFiles: new[] { ("Alpha.txt", "a") });
        }
    }
}

}

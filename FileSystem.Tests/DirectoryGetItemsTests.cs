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
            ).WithFileSystem(new[] { ("Alpha.txt", "a") });

            yield return new StepCase(
                "Nested one level",
                new DirectoryGetItems() { Directory = StaticHelpers.Constant("myDir") },
                new[] { Entity.Create(("Name", "Alpha.txt"), ("Type", "File")) }.ToSCLArray()
            ).WithFileSystem(new[] { ("myDir/Alpha.txt", "a") });

            yield return new StepCase(
                "Simple pattern",
                new DirectoryGetItems()
                {
                    Directory = StaticHelpers.Constant("myDir"),
                    Pattern   = StaticHelpers.Constant("*.txt")
                },
                new[] { Entity.Create(("Name", "Alpha.txt"), ("Type", "File")) }.ToSCLArray()
            ).WithFileSystem(new[] { ("myDir/Alpha.txt", "a"), ("myDir/Alpha.jpg", "a"), });

            yield return new StepCase(
                "Recursive Case",
                new DirectoryGetItems() { Directory = StaticHelpers.Constant("dira") },
                new[]
                {
                    Entity.Create(("Name", "Alpha.txt"), ("Type", "File")),
                    Entity.Create(
                        ("Name", "dirb"),
                        ("Type", "Directory"),
                        ("Children",
                         new List<Entity>()
                         {
                             Entity.Create(("Name", "Beta.txt"), ("Type", "File")),
                         })
                    ),
                    Entity.Create(
                        ("Name", "dirc"),
                        ("Type", "Directory"),
                        ("Children",
                         new List<Entity>()
                         {
                             Entity.Create(("Name", "Gamma.txt"), ("Type", "File")),
                         })
                    ),
                }.ToSCLArray()
            ).WithFileSystem(
                new[]
                {
                    ("dira/Alpha.txt", "a"), ("dira/dirb/Beta.txt", "a"),
                    ("dira/dirc/Gamma.txt", "a"),
                }
            );
        }
    }
}

}

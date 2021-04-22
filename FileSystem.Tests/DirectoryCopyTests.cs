using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Thinktecture.IO;

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
                        new DirectoryCopy()
                        {
                            SourceDirectory      = StaticHelpers.Constant("MySource"),
                            DestinationDirectory = StaticHelpers.Constant("MyDestination"),
                            Overwrite            = StaticHelpers.Constant(true),
                            CopySubDirectories   = StaticHelpers.Constant(true)
                        },
                        Unit.Default
                    )
                    .WithDirectoryAction(x => x.Setup(d => d.Exists("MySource")).Returns(true))
                    .WithDirectoryAction(
                        x => x.Setup(d => d.GetDirectories("MySource"))
                            .Returns(new[] { Path.Combine("MySource", "Sub") })
                    )
                    .WithDirectoryAction(
                        x => x.Setup(d => d.CreateDirectory("MyDestination"))
                            .Returns((null as IDirectoryInfo)!)
                    )
                    .WithDirectoryAction(
                        x => x.Setup(d => d.GetFiles("MySource"))
                            .Returns(
                                new[]
                                {
                                    Path.Combine("MySource", "f1"),
                                    Path.Combine("MySource", "f2")
                                }
                            )
                    )
                    .WithFileAction(
                        x => x.Setup(
                            f => f.Copy(
                                Path.Combine("MySource",      "f1"),
                                Path.Combine("MyDestination", "f1"),
                                true
                            )
                        )
                    )
                    .WithFileAction(
                        x => x.Setup(
                            f => f.Copy(
                                Path.Combine("MySource",      "f2"),
                                Path.Combine("MyDestination", "f2"),
                                true
                            )
                        )
                    )
                    .WithDirectoryAction(
                        x => x.Setup(d => d.Exists(Path.Combine("MySource", "Sub"))).Returns(true)
                    )
                    .WithDirectoryAction(
                        x => x.Setup(d => d.GetDirectories(Path.Combine("MySource", "Sub")))
                            .Returns(new string[] { })
                    )
                    .WithDirectoryAction(
                        x => x.Setup(d => d.CreateDirectory(Path.Combine("MyDestination", "Sub")))
                            .Returns((null as IDirectoryInfo)!)
                    )
                    .WithDirectoryAction(
                        x => x.Setup(d => d.GetFiles(Path.Combine("MySource", "Sub")))
                            .Returns(new[] { Path.Combine("MySource", "Sub", "f3") })
                    )
                    .WithFileAction(
                        x => x.Setup(
                            f => f.Copy(
                                Path.Combine("MySource",      "Sub", "f3"),
                                Path.Combine("MyDestination", "Sub", "f3"),
                                true
                            )
                        )
                    )
                ;
        }
    }
}

}

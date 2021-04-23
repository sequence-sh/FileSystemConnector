using System.Collections.Generic;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class FileExtractTests : StepTestBase<FileExtract, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                        "FileExtract",
                        new FileExtract
                        {
                            ArchiveFilePath = Constant("Foo"),
                            Destination     = Constant("Bar"),
                            Overwrite       = Constant(true)
                        },
                        Unit.Default
                    )
                    .WithCompressionMock(
                        x => x.Setup(c => c.ExtractToDirectory("Foo", "Bar", true))
                    )
                ;
        }
    }
}

}

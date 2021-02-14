using System.Collections.Generic;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.Templates.EDRConnector.Tests
{

public partial class CheckFileExistsTests : StepTestBase<CheckFileExists, bool>
{
    private const string FilePath = "path/to/file";

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "File exists",
                    new CheckFileExists { Path = Constant(FilePath) },
                    true
                )
                .WithFileSystemAction(x => x.Setup(a => a.DoesFileExist(FilePath)).Returns(true));

            yield return new StepCase(
                    "File does not exist",
                    new CheckFileExists { Path = Constant(FilePath) },
                    false
                )
                .WithFileSystemAction(x => x.Setup(a => a.DoesFileExist(FilePath)).Returns(false));
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Check if a file exists (file exists)",
                $"Print (CheckFileExists '{FilePath}')",
                Unit.Default,
                "True"
            ).WithFileSystemAction(x => x.Setup(a => a.DoesFileExist(FilePath)).Returns(true));
        }
    }
}

}

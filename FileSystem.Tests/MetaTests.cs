using System.Reflection;
using Sequence.Connectors.FileSystem.Steps;

namespace Sequence.Connectors.FileSystem.Tests;

public class MetaTests : Core.TestHarness.MetaTestsBase
{
    /// <inheritdoc />
    public override Assembly StepAssembly { get; } = Assembly.GetAssembly(typeof(FileRead))!;

    /// <inheritdoc />
    public override Assembly TestAssembly { get; } = Assembly.GetAssembly(typeof(MetaTests))!;
}

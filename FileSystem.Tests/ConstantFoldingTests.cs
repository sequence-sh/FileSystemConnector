﻿using System.Reflection;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Sequence.Connectors.FileSystem.Steps;
using Sequence.Core.Abstractions;
using Sequence.Core.Internal.Parser;
using Sequence.Core.Internal.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace Sequence.Connectors.FileSystem.Tests;

public class ConstantFoldingTests
{
    public ConstantFoldingTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    private ITestOutputHelper TestOutputHelper { get; }

    [Theory]
    [InlineData("fileread 'abc'",                 "abc")]
    [InlineData("fileread (path: 'abc')['path']", "abc")]
    public async Task TestGettingPathParameters(string scl, object expectedValue)
    {
        var sfs = StepFactoryStore.TryCreateFromAssemblies(
                NullExternalContext.Instance,
                Assembly.GetAssembly(typeof(FileRead))
            )
            .Value;

        var variables = new Dictionary<VariableName, InjectedVariable>();

        var parseResult = SCLParsing
            .TryParseStep(scl)
            .Bind(
                x => x.TryFreeze(
                    SCLRunner.RootCallerMetadata,
                    sfs,
                    new OptimizationSettings(false, false, variables)
                )
            );

        parseResult.ShouldBeSuccessful();

        var expectedSclObject = ISCLObject.CreateFromCSharpObject(expectedValue)
            .Serialize(SerializeOptions.Primitive);

        var pathValue =
            parseResult.Value.GetParameterValues()
                .Single(x => x.Parameter.Metadata.ContainsKey("Path"))
                .Value;

        var pvv = await pathValue.TryGetConstantValueAsync(
            variables.ToDictionary(x => x.Key, x => x.Value.SCLObject),
            sfs
        );

        pvv.ShouldHaveValue();

        pvv.Value.Serialize(SerializeOptions.Primitive).Should().Be(expectedSclObject);
    }
}

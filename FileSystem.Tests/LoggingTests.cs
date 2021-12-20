using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoTheory;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.Sequence.Core.Internal.Serialization;
using Xunit.Abstractions;

namespace Reductech.Sequence.Connectors.FileSystem.Tests
{

public partial class LoggingTests
{
    [GenerateAsyncTheory("CheckLogging")]
    #pragma warning disable CA1822 // Mark members as static
    public IEnumerable<LoggingTestCase> TestCases
        #pragma warning restore CA1822 // Mark members as static
    {
        get
        {
            yield return new
                LoggingTestCase(
                    "No Path to combine",
                    "Log (PathCombine [])",
                    CheckMessageAndScope(LogLevel.Debug, "Sequence Started", null),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        new Regex("ConnectorSettings"),
                        null
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "Log Started with Parameters: [Value, (PathCombine Paths: [])]",
                        new[] { "Log" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "PathCombine Started with Parameters: [Paths, []]",
                        new[] { "Log", "PathCombine" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "ArrayNew Started with Parameters: [Elements, []]",
                        new[] { "Log", "PathCombine", "ArrayNew" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "ArrayNew Completed Successfully with Result: []",
                        new[] { "Log", "PathCombine", "ArrayNew" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Warning,
                        "No path was provided. Returning the Current Directory: /MyDir",
                        new[] { "Log", "PathCombine" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "PathCombine Completed Successfully with Result: string Length: 6",
                        new[] { "Log", "PathCombine" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Information,
                        "/MyDir",
                        new[] { "Log" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "Log Completed Successfully with Result: Unit",
                        new[] { "Log" }
                    ),
                    CheckMessageAndScope(LogLevel.Debug, "Sequence Completed", null)
                ).WithFileSystem(currentDirectory: "/MyDir");

            yield return new LoggingTestCase(
                "Unqualified Path to combine",
                "Log (PathCombine ['File'])",
                CheckMessageAndScope(LogLevel.Debug, "Sequence Started", null),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    new Regex("ConnectorSettings"),
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Log Started with Parameters: [Value, (PathCombine Paths: [string Length: 4])]",
                    new[] { "Log" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "PathCombine Started with Parameters: [Paths, [string Length: 4]]",
                    new[] { "Log", "PathCombine" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Started with Parameters: [Elements, [string Length: 4]]",
                    new[] { "Log", "PathCombine", "ArrayNew" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Completed Successfully with Result: [string Length: 4]",
                    new[] { "Log", "PathCombine", "ArrayNew" }
                ),
                CheckMessageAndScope(
                    LogLevel.Debug,
                    "Path /MyDir was not fully qualified. Prepending the Current Directory: /MyDir",
                    new[] { "Log", "PathCombine" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "PathCombine Completed Successfully with Result: string Length: 11",
                    new[] { "Log", "PathCombine", }
                ),
                x =>
                {
                    x.LogLevel.Should().Be(LogLevel.Information);
                    x.Message.Should().BeOneOf("/MyDir/File", "/MyDir\\File");
                },
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Log Completed Successfully with Result: Unit",
                    new[] { "Log" }
                ),
                CheckMessageAndScope(LogLevel.Debug, "Sequence Completed", null)
            ).WithFileSystem(currentDirectory: "/MyDir");

            yield return new LoggingTestCase(
                "File Read",
                "FileRead 'MyFile' | Log",
                CheckMessageAndScope(LogLevel.Debug, "Sequence Started", null),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    new Regex("ConnectorSettings"),
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Log Started with Parameters: [Value, (FileRead Path: string Length: 6 Encoding: EncodingEnum.UTF8 Decompress: False)]",
                    new[] { "Log" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "FileRead Started with Parameters: [Path, string Length: 6], [Encoding, EncodingEnum.UTF8], [Decompress, False]",
                    new[] { "Log", "FileRead" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "FileRead Completed Successfully with Result: UTF8-Stream",
                    new[] { "Log", "FileRead" }
                ),
                CheckMessageAndScope(LogLevel.Information, "MyData", new[] { "Log" }),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Log Completed Successfully with Result: Unit",
                    new[] { "Log" }
                ),
                CheckMessageAndScope(LogLevel.Debug, "Sequence Completed", null)
            ).WithFileSystem(initialFiles: new[] { ("MyFile", "MyData") });
        }
    }

    private static Action<LogEntry> CheckMessageAndScope(
        LogLevel logLevel,
        Regex messageRegex,
        IReadOnlyList<string>? expectedScopes)
    {
        return entry =>
        {
            entry.LogLevel.Should().Be(logLevel);
            entry.Message.Should().MatchRegex(messageRegex);

            var trueExpectedScopes =
                expectedScopes is null
                    ? new List<string>() { "Sequence" }
                    : expectedScopes.Prepend("Sequence").ToList();

            entry.Scopes.Select(x => x.Message)
                .Should()
                .BeEquivalentTo(trueExpectedScopes);
        };
    }

    private static Action<LogEntry> CheckMessageAndScope(
        LogLevel logLevel,
        string expectedMessage,
        IReadOnlyList<string>? expectedScopes)
    {
        return entry =>
        {
            entry.LogLevel.Should().Be(logLevel);
            entry.Message.Should().Match(expectedMessage);

            var trueExpectedScopes =
                expectedScopes is null
                    ? new List<string>() { "Sequence" }
                    : expectedScopes.Prepend("Sequence").ToList();

            entry.Scopes.Select(x => x.Message)
                .Should()
                .BeEquivalentTo(trueExpectedScopes);
        };
    }

    public record LoggingTestCase : IAsyncTestInstance, ICaseWithSetup
    {
        public LoggingTestCase(string name, string scl, params Action<LogEntry>[] expectedLogs)
        {
            Name         = name;
            SCL          = scl;
            ExpectedLogs = expectedLogs;
        }

        public string Name { get; set; }

        public string SCL { get; set; }
        public IReadOnlyList<Action<LogEntry>> ExpectedLogs { get; set; }

        /// <inheritdoc />
        public async Task RunAsync(ITestOutputHelper testOutputHelper)
        {
            var repo = new MockRepository(MockBehavior.Strict);

            var assembly = typeof(PathCombine).Assembly;

            var externalContext = ExternalContextSetupHelper.GetExternalContext(
                repo,
                repo.OneOf<IRestClientFactory>()
            );

            var sfsResult = StepFactoryStore.TryCreateFromAssemblies(externalContext, assembly);

            sfsResult.ShouldBeSuccessful();

            var loggerFactory = TestLoggerFactory.Create();
            loggerFactory.AddXunit(testOutputHelper);

            var logger = loggerFactory.CreateLogger("Test");

            var sclRunner = new SCLRunner(
                logger,
                sfsResult.Value,
                externalContext
            );

            var r = await sclRunner.RunSequenceFromTextAsync(
                SCL,
                new Dictionary<string, object>(),
                CancellationToken.None
            );

            r.ShouldBeSuccessful();

            loggerFactory.Sink.LogEntries.Should().SatisfyRespectively(ExpectedLogs);
        }

        /// <inheritdoc />
        public ExternalContextSetupHelper ExternalContextSetupHelper { get; } = new();

        public RESTClientSetupHelper RESTClientSetupHelper { get; } = new();

        /// <inheritdoc />
        public List<Action> FinalChecks { get; } = new();
    }
}

}

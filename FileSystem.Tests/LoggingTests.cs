using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoTheory;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.FileSystem.Tests
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
                    CheckMessageAndScope(LogLevel.Debug, "EDR Sequence Started", null),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "Log Started with Parameters: [Value, PathCombine]",
                        new[] { "Log" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "PathCombine Started with Parameters: [Paths, ArrayNew]",
                        new[] { "Log", "PathCombine" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "ArrayNew Started with Parameters: [Elements, 0 Elements]",
                        new[] { "Log", "PathCombine", "ArrayNew" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "ArrayNew Completed Successfully with Result: 0 Elements",
                        new[] { "Log", "PathCombine", "ArrayNew" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Warning,
                        "No path was provided. Returning the Current Directory: MyDir",
                        new[] { "Log", "PathCombine" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "PathCombine Completed Successfully with Result: string Length: 5",
                        new[] { "Log", "PathCombine" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Information,
                        @"MyDir",
                        new[] { "Log" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "Log Completed Successfully with Result: Unit",
                        new[] { "Log" }
                    ),
                    CheckMessageAndScope(LogLevel.Debug, "EDR Sequence Completed", null)
                ).WithDirectoryAction(
                x =>
                    x.Setup(f => f.GetCurrentDirectory()).Returns("MyDir")
            );

            yield return new LoggingTestCase(
                    "Unqualified Path to combine",
                    "Log (PathCombine ['File'])",
                    CheckMessageAndScope(LogLevel.Debug, "EDR Sequence Started", null),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "Log Started with Parameters: [Value, PathCombine]",
                        new[] { "Log" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "PathCombine Started with Parameters: [Paths, ArrayNew]",
                        new[] { "Log", "PathCombine" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "ArrayNew Started with Parameters: [Elements, 1 Elements]",
                        new[] { "Log", "PathCombine", "ArrayNew" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "ArrayNew Completed Successfully with Result: 1 Elements",
                        new[] { "Log", "PathCombine", "ArrayNew" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Debug,
                        "Path MyDir was not fully qualified. Prepending the Current Directory: MyDir",
                        new[] { "Log", "PathCombine" }
                    ),
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "PathCombine Completed Successfully with Result: string Length: 10",
                        new[] { "Log", "PathCombine", }
                    ),
                    x =>
                    {
                        x.LogLevel.Should().Be(LogLevel.Information);
                        x.Message.Should().BeOneOf(@"MyDir\File", @"MyDir/File");
                    },
                    CheckMessageAndScope(
                        LogLevel.Trace,
                        "Log Completed Successfully with Result: Unit",
                        new[] { "Log" }
                    ),
                    CheckMessageAndScope(LogLevel.Debug, "EDR Sequence Completed", null)
                )
                .WithDirectoryAction(
                    x =>
                        x.Setup(f => f.GetCurrentDirectory()).Returns("MyDir")
                );

            yield return new LoggingTestCase(
                "File Read",
                "FileRead 'MyFile' | Log",
                CheckMessageAndScope(LogLevel.Debug, "EDR Sequence Started", null),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Log Started with Parameters: [Value, FileRead]",
                    new[] { "Log" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "FileRead Started with Parameters: [Path, \"MyFile\"], [Encoding, UTF8], [Decompress, False]",
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
                CheckMessageAndScope(LogLevel.Debug, "EDR Sequence Completed", null)
            ).WithFileAction(
                x => x.Setup(a => a.OpenRead("MyFile"))
                    .Returns(
                        () =>
                        {
                            var s  = new MemoryStream();
                            var sw = new StreamWriter(s);
                            sw.Write("MyData");
                            s.Seek(0, SeekOrigin.Begin);
                            sw.Flush();

                            return new FakeFileStreamAdapter(s);
                        }
                    )
            );
        }
    }

    private static Action<LogEntry> CheckMessageAndScope(
        LogLevel logLevel,
        string expectedMessage,
        IReadOnlyList<string>? expectedScopes)
    {
        return entry =>
        {
            entry.LogLevel.Should().Be(logLevel);
            entry.Message.Should().Be(expectedMessage);

            var trueExpectedScopes =
                expectedScopes is null
                    ? new List<string>() { "EDR" }
                    : expectedScopes.Prepend("EDR").ToList();

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
            var spf = StepFactoryStore.CreateFromAssemblies(typeof(PathCombine).Assembly);

            var loggerFactory = TestLoggerFactory.Create();
            loggerFactory.AddXunit(testOutputHelper);

            var logger = loggerFactory.CreateLogger("Test");
            var repo   = new MockRepository(MockBehavior.Strict);

            var context = ExternalContextSetupHelper.GetExternalContext(repo);

            var sclRunner = new SCLRunner(
                SCLSettings.EmptySettings,
                logger,
                spf,
                context
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
    }
}

}

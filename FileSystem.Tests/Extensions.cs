using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using Moq;
using Reductech.EDR.Core.Abstractions;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public static class Extensions
{
    public static T WithCompressionMock<T>(this T cws, Action<Mock<ICompression>> action)
        where T : ICaseWithSetup
    {
        cws.ExternalContextSetupHelper.AddContextMock(
            ConnectorInjection.CompressionKey,
            mr =>
            {
                var mock = mr.Create<ICompression>();
                action(mock);
                return mock;
            }
        );

        return cws;
    }

    public static T WithFileSystemMock<T>(this T cws, Action<Mock<IFileSystem>> action)
        where T : ICaseWithSetup
    {
        cws.ExternalContextSetupHelper.AddContextMock(
            ConnectorInjection.FileSystemKey,
            mr =>
            {
                var mock = mr.Create<IFileSystem>();
                action(mock);
                return mock;
            }
        );

        return cws;
    }

    public static T WithFileSystem<T>(
        this T cws,
        IReadOnlyCollection<(string fileName, string fileText)>? initialFiles = null,
        IReadOnlyCollection<string>? initialDirectories = null,
        string? currentDirectory = null)
        where T : ICaseWithSetup
    {
        var filesDict =
            (initialFiles ?? new List<(string fileName, string fileText)>()).ToDictionary(
                x => x.fileName,
                x => new MockFileData(x.fileText)
            );

        var fs = new MockFileSystem(filesDict, currentDirectory);

        foreach (var initialDirectory in initialDirectories ?? new List<string>())
        {
            fs.AddDirectory(initialDirectory);
        }

        cws = cws.WithContext(ConnectorInjection.FileSystemKey, fs);

        return cws;
    }

    public static T WithExpectedFileSystem<T>(
        this T cws,
        IReadOnlyCollection<(string filename, string filetext)>? expectedFinalFiles = null,
        IReadOnlyCollection<string>? expectedFinalDirectories = null)
        where T : ICaseThatExecutes
    {
        cws = cws.WithFinalContextCheck(
            ec => CheckContext(
                ec,
                expectedFinalFiles ?? new List<(string filename, string filetext)>(),
                expectedFinalDirectories ?? new List<string>()
            )
        );

        return cws;

        static void CheckContext(
            IExternalContext externalContext,
            IReadOnlyCollection<(string filename, string filetext)> expectedFinalFiles,
            IReadOnlyCollection<string> expectedFinalDirectories)
        {
            var fs = externalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

            fs.ShouldBeSuccessful();

            var allActualFiles = fs.Value.Directory
                .EnumerateFiles(
                    fs.Value.Directory.GetCurrentDirectory(),
                    "*",
                    SearchOption.AllDirectories
                )
                .Select(NormalizePath)
                .ToList();

            allActualFiles.Should()
                .BeEquivalentTo(expectedFinalFiles.Select(x => x.filename).Select(NormalizePath));

            foreach (var (expectedFilename, expectedFileText) in expectedFinalFiles)
            {
                var actualText = fs.Value.File.ReadAllText(expectedFilename);

                actualText.Should()
                    .Be(
                        expectedFileText,
                        $"File '{expectedFilename}' should have expected text"
                    );
            }

            var actualDirectories =
                    fs.Value.Directory
                        .EnumerateDirectories(fs.Value.Directory.GetCurrentDirectory())
                        .Select(
                            x => Path.GetRelativePath(fs.Value.Directory.GetCurrentDirectory(), x)
                        )
                        .Select(NormalizePath)
                        .Except(new[] { "temp" }) //ignore the temp directory
                ;

            actualDirectories.Should()
                .BeEquivalentTo(expectedFinalDirectories.Select(NormalizePath));
        }

        static string NormalizePath(string s)
        {
            if (s.StartsWith("C:", StringComparison.OrdinalIgnoreCase))
                s = s[2..];

            s = s.Replace('/', '\\');

            return s;
        }
    }
}

}

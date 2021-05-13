using System.Collections.Generic;
using System.IO.Abstractions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Connectors;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.FileSystem
{

public sealed class ConnectorInjection : IConnectorInjection
{
    public const string FileSystemKey = "FileSystem.FileSystem";
    public const string CompressionKey = "FileSystem.Compression";

    /// <inheritdoc />
    public Result<IReadOnlyCollection<(string Name, object Context)>, IErrorBuilder>
        TryGetInjectedContexts(SCLSettings settings)
    {
        IFileSystem  fileSystem  = new System.IO.Abstractions.FileSystem();
        ICompression compression = new CompressionAdapter();

        IReadOnlyCollection<(string Name, object Context)> list =
            new List<(string Name, object Context)>
            {
                (FileSystemKey, fileSystem), (CompressionKey, compression)
            };

        return Result.Success<IReadOnlyCollection<(string Name, object Context)>, IErrorBuilder>(
            list
        );
    }
}

}

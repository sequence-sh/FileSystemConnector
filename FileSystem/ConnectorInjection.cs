﻿using Sequence.Core.Connectors;
using Sequence.Core.Internal.Errors;

namespace Sequence.Connectors.FileSystem;

/// <summary>
/// For injecting the connector context
/// </summary>
public sealed class ConnectorInjection : IConnectorInjection
{
    /// <summary>
    /// The key for FileSystem injection
    /// </summary>
    public const string FileSystemKey = "FileSystem.FileSystem";

    /// <summary>
    /// The key for Compression injection
    /// </summary>
    public const string CompressionKey = "FileSystem.Compression";

    /// <inheritdoc />
    public Result<IReadOnlyCollection<(string Name, object Context)>, IErrorBuilder>
        TryGetInjectedContexts()
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

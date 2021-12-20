namespace Reductech.Sequence.Connectors.FileSystem;

/// <summary>
/// Default implementation of ICompression
/// </summary>
public class CompressionAdapter : ICompression
{
    /// <inheritdoc />
    public void ExtractToDirectory(
        string sourceArchiveFileName,
        string destinationDirectoryName,
        bool overwrite)
    {
        System.IO.Compression.ZipFile.ExtractToDirectory(
            sourceArchiveFileName,
            destinationDirectoryName,
            overwrite
        );
    }
}

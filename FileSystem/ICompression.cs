namespace Sequence.Connectors.FileSystem;

/// <summary>
/// Abstraction layer for System.IO.Compression
/// </summary>
public interface ICompression
{
    /// <summary>
    /// Extract a file to a directory
    /// </summary>
    public void ExtractToDirectory(
        string sourceArchiveFileName,
        string destinationDirectoryName,
        bool overwrite);
}

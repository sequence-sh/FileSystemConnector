using System.Linq;
using Reductech.Sequence.Core.Internal.Errors;
using Entity = Reductech.Sequence.Core.Entity;

namespace Reductech.Sequence.Connectors.FileSystem;

/// <summary>
/// Gets all items in a directory
/// </summary>
[Alias("ls")]
[Alias("dir")]
public class DirectoryGetItems : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var directory =
            await Directory.Run(stateMonad, cancellationToken).Map(x => x.GetStringAsync());

        if (directory.IsFailure)
            return directory.ConvertFailure<Array<Entity>>();

        var pattern = await Pattern.Run(stateMonad, cancellationToken).Map(x => x.GetStringAsync());

        if (pattern.IsFailure)
            return pattern.ConvertFailure<Array<Entity>>();

        var includeFiles = await IncludeFiles.Run(stateMonad, cancellationToken);

        if (includeFiles.IsFailure)
            return includeFiles.ConvertFailure<Array<Entity>>();

        var includeDirectories = await IncludeDirectories.Run(stateMonad, cancellationToken);

        if (includeDirectories.IsFailure)
            return includeDirectories.ConvertFailure<Array<Entity>>();

        var recursive = await Recursive.Run(stateMonad, cancellationToken);

        if (recursive.IsFailure)
            return recursive.ConvertFailure<Array<Entity>>();

        var fileSystem =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystem.IsFailure)
            return fileSystem.MapError(x => x.WithLocation(this))
                .ConvertFailure<Array<Entity>>();

        var items = new List<Item>();

        var realPath = string.IsNullOrWhiteSpace(directory.Value)
            ? fileSystem.Value.Directory.GetCurrentDirectory()
            : directory.Value;

        if (includeFiles.Value)
        {
            var files = EnumerateFiles(fileSystem.Value, realPath, pattern.Value);
            items.AddRange(files);
        }

        if (includeDirectories.Value)
        {
            var directories = EnumerateDirectories(
                fileSystem.Value,
                realPath,
                pattern.Value,
                recursive.Value,
                includeFiles.Value
            );

            items.AddRange(directories);
        }

        var entities = items.Select(x => x.ConvertToEntity()).ToSCLArray();

        return entities;
    }

    [Serializable]
    private class Item : IEntityConvertible
    {
        [System.Text.Json.Serialization.JsonPropertyName("Name")]
        public string Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("FullPath")]
        public string FullPath { get; set; }

        /// <summary>
        /// True for files, false for directories
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("IsFile")]
        public bool IsFile { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("Directory")]
        public string Directory { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("Children")]
        public IReadOnlyList<Item>? Children { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("BaseName")]
        public string BaseName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("Extension")]
        public string? Extension { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("CreationTime")]
        public DateTime CreationTime { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("LastWriteTime")]
        public DateTime LastWriteTime { get; set; }
    }

    private static Item CreateFile(IFileSystem fileSystem, string fullPath)
    {
        var fileInfo = fileSystem.FileInfo.FromFileName(fullPath);
        var baseName = fileSystem.Path.GetFileNameWithoutExtension(fileInfo.Name);

        var item = new Item()
        {
            Name          = fileInfo.Name,
            IsFile        = true,
            CreationTime  = fileInfo.CreationTime,
            LastWriteTime = fileInfo.LastWriteTime,
            Children      = null,
            Directory     = fileInfo.Directory?.Name ?? "",
            BaseName      = baseName,
            Extension     = fileInfo.Extension,
            FullPath      = fullPath
        };

        return item;
    }

    private static Item CreateDirectory(
        IFileSystem fileSystem,
        string fullPath,
        IReadOnlyList<Item> children)
    {
        var dirInfo  = fileSystem.DirectoryInfo.FromDirectoryName(fullPath);
        var baseName = fileSystem.Path.GetFileNameWithoutExtension(dirInfo.Name);

        var item = new Item()
        {
            Name          = dirInfo.Name,
            IsFile        = false,
            CreationTime  = dirInfo.CreationTime,
            LastWriteTime = dirInfo.LastWriteTime,
            Children      = children.Any() ? children : null,
            Directory     = dirInfo.Parent?.Name ?? "",
            BaseName      = baseName,
            Extension     = null,
            FullPath      = fullPath
        };

        return item;
    }

    private static IEnumerable<Item> EnumerateFiles(
        IFileSystem fileSystem,
        string path,
        string pattern)
    {
        List<string> names;

        if (string.IsNullOrWhiteSpace(pattern))
            names = fileSystem.Directory.EnumerateFiles(path).ToList();
        else
            names = fileSystem.Directory.EnumerateFiles(path, pattern).ToList();

        foreach (var name in names)
        {
            yield return CreateFile(fileSystem, name);
        }
    }

    private static IEnumerable<Item> EnumerateDirectories(
        IFileSystem fileSystem,
        string path,
        string pattern,
        bool recursive,
        bool addChildFiles)
    {
        List<string> names;

        if (string.IsNullOrWhiteSpace(pattern))
            names = fileSystem.Directory.EnumerateDirectories(path).ToList();
        else
            names = fileSystem.Directory.EnumerateDirectories(path, pattern).ToList();

        foreach (var name in names)
        {
            var children = new List<Item>();

            if (addChildFiles || recursive)
            {
                var combinedPath = fileSystem.Path.Combine(path, name);

                if (addChildFiles)
                {
                    var files = EnumerateFiles(fileSystem, combinedPath, pattern);
                    children.AddRange(files);
                }

                if (recursive)
                {
                    var dirs = EnumerateDirectories(
                        fileSystem,
                        combinedPath,
                        pattern,
                        recursive,
                        addChildFiles
                    );

                    children.AddRange(dirs);
                }

                var directory = CreateDirectory(fileSystem, combinedPath, children);
                yield return directory;
            }
        }
    }

    /// <summary>
    /// The path to the directory to enumerate
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Metadata("Path", "Read")]
    public IStep<StringStream> Directory { get; set; } = null!;

    /// <summary>
    /// The pattern to search by
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("No Pattern")]
    [Example("*.jpg")]
    public IStep<StringStream> Pattern { get; set; } = new SCLConstant<StringStream>("");

    /// <summary>
    /// Whether to include files in the results
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("true")]
    public IStep<SCLBool> IncludeFiles { get; set; } = new SCLConstant<SCLBool>(SCLBool.True);

    /// <summary>
    /// Whether to include directories in the results
    /// </summary>
    [StepProperty(4)]
    [DefaultValueExplanation("true")]
    public IStep<SCLBool> IncludeDirectories { get; set; } = new SCLConstant<SCLBool>(SCLBool.True);

    /// <summary>
    /// Whether to search recursively
    /// </summary>
    [StepProperty(5)]
    [DefaultValueExplanation("true")]
    public IStep<SCLBool> Recursive { get; set; } = new SCLConstant<SCLBool>(SCLBool.True);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<DirectoryGetItems, Array<Entity>>();
}

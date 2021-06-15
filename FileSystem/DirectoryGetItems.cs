using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.FileSystem
{

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

        var entities = new List<Entity>();

        var realPath = string.IsNullOrWhiteSpace(directory.Value)
            ? fileSystem.Value.Directory.GetCurrentDirectory()
            : directory.Value;

        if (includeFiles.Value)
        {
            var files = EnumerateFiles(fileSystem.Value, realPath, pattern.Value);
            entities.AddRange(files);
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

            entities.AddRange(directories);
        }

        return entities.ToSCLArray();
    }

    private const string NameKey = "Name";
    private const string TypeKey = "Type";
    private const string ChildrenKey = "Children";

    private static Entity CreateFile(string name)
    {
        return Entity.Create((NameKey, name), (TypeKey, "File"));
    }

    private static Entity CreateDirectory(string name, IReadOnlyCollection<Entity> children)
    {
        if (children.Any())
        {
            return Entity.Create((NameKey, name), (TypeKey, "Directory"), (ChildrenKey, children));
        }
        else
        {
            return Entity.Create((NameKey, name), (TypeKey, "Directory"));
        }
    }

    private static IEnumerable<Entity> EnumerateFiles(
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
            var fileName = fileSystem.Path.GetFileName(name);
            yield return CreateFile(fileName);
        }
    }

    private static IEnumerable<Entity> EnumerateDirectories(
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
            var children = new List<Entity>();

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

                var dirName = fileSystem.Path.GetFileName(name);

                var directory = CreateDirectory(dirName, children);
                yield return directory;
            }
        }
    }

    /// <summary>
    /// The path to the directory to enumerate
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> Directory { get; set; } = null!;

    /// <summary>
    /// The pattern to search by
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("No Pattern")]
    [Example("*.jpg")]
    public IStep<StringStream> Pattern { get; set; } = new StringConstant("");

    /// <summary>
    /// Whether to include files in the results
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("true")]
    public IStep<bool> IncludeFiles { get; set; } = new BoolConstant(true);

    /// <summary>
    /// Whether to include directories in the results
    /// </summary>
    [StepProperty(4)]
    [DefaultValueExplanation("true")]
    public IStep<bool> IncludeDirectories { get; set; } = new BoolConstant(true);

    /// <summary>
    /// Whether to search recursively
    /// </summary>
    [StepProperty(5)]
    [DefaultValueExplanation("true")]
    public IStep<bool> Recursive { get; set; } = new BoolConstant(true);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<DirectoryGetItems, Array<Entity>>();
}

}

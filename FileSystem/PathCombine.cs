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
using Reductech.EDR.Core.Internal.Logging;

namespace Reductech.EDR.Connectors.FileSystem
{

/// <summary>
/// Combine Paths.
/// If the resulting path is not fully qualified it will be prefixed with the current working directory.
/// </summary>
[Alias("JoinPath")]
[Alias("ResolvePath")]
public sealed class PathCombine : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var pathsResult = await Paths.Run(stateMonad, cancellationToken)
            .Bind(x => x.GetElementsAsync(cancellationToken));

        if (pathsResult.IsFailure)
            return pathsResult.ConvertFailure<StringStream>();

        var paths = new List<string>();

        foreach (var stringStream in pathsResult.Value)
            paths.Add(await stringStream.GetStringAsync());

        var fileSystemResult =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemResult.IsFailure)
            return fileSystemResult.MapError(x => x.WithLocation(this))
                .ConvertFailure<StringStream>();

        if (!paths.Any())
        {
            var currentDirectory = fileSystemResult.Value.Directory.GetCurrentDirectory();

            LogSituation.NoPathProvided.Log(stateMonad, this, currentDirectory);

            return new StringStream(currentDirectory);
        }

        if (!fileSystemResult.Value.Path.IsPathFullyQualified(paths[0]))
        {
            var currentDirectory =
                fileSystemResult.Value.Directory.GetCurrentDirectory();

            paths = paths.Prepend(currentDirectory).ToList();

            LogSituation.QualifyingPath.Log(stateMonad, this, paths[0], currentDirectory);
        }

        StringStream result = fileSystemResult.Value.Path.Combine(paths.ToArray());

        return result;
    }

    /// <summary>
    /// The paths to combine.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<StringStream>> Paths { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<PathCombine, StringStream>();
}

}

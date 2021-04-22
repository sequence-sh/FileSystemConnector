using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.FileSystem
{

/// <summary>
/// Returns true if the file in the specified path exists, false otherwise
/// </summary>
public class CheckFileExists : CompoundStep<bool>
{
    /// <summary>
    /// The path to the file to check.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("File")]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<bool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var pathResult = await Path.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (pathResult.IsFailure)
            return pathResult.ConvertFailure<bool>();

        var r = stateMonad.FileSystemHelper.DoesFileExist(pathResult.Value);
        return r;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => CheckFileExistsStepFactory.Instance;
}

/// <summary>
/// Returns true if the file in the specified path exists, false otherwise
/// </summary>
public class CheckFileExistsStepFactory : SimpleStepFactory<CheckFileExists, bool>
{
    private CheckFileExistsStepFactory() { }

    /// <summary>
    /// An instance of DoesFileExistStepFactory
    /// </summary>
    public static SimpleStepFactory<CheckFileExists, bool> Instance { get; } =
        new CheckFileExistsStepFactory();
}

}

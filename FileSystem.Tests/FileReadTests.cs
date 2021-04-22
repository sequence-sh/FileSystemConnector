using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Moq;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Thinktecture.IO;
using Thinktecture.IO.Adapters;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class FileReadTests : StepTestBase<FileRead, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Log file text",
                new Log<StringStream> { Value = new FileRead { Path = Constant("File.txt"), } },
                Unit.Default,
                "Hello World"
            ).WithFileAction(
                x => x.Setup(f => f.OpenRead("File.txt"))
                    .Returns(new FakeFileStreamAdapter("Hello World"))
            );

            yield return new StepCase(
                    "Log file text compressed",
                    new Log<StringStream>
                    {
                        Value = new FileRead
                        {
                            Path = Constant("File.txt"), Decompress = Constant(true)
                        }
                    },
                    Unit.Default,
                    "Hello World"
                ).WithFileAction(
                    x => x.Setup(f => f.OpenRead("File.txt"))
                        .Returns(new FakeFileStreamAdapter("Hello World"))
                )
                .WithCompressionAction(
                    x => x.Setup(c => c.Decompress(It.IsAny<IStream>()))
                        .Returns(
                            new StreamAdapter(
                                new MemoryStream(Encoding.ASCII.GetBytes("Hello World"))
                            )
                        )
                );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Default",
                "Log Value: (FileRead Path: 'File.txt')",
                Unit.Default,
                "Hello World"
            ).WithFileAction(
                x => x.Setup(f => f.OpenRead("File.txt"))
                    .Returns(new FakeFileStreamAdapter("Hello World"))
            );

            yield return new DeserializeCase(
                "Ordered Args",
                "Log (FileRead 'File.txt')",
                Unit.Default,
                "Hello World"
            ).WithFileAction(
                x => x.Setup(f => f.OpenRead("File.txt"))
                    .Returns(new FakeFileStreamAdapter("Hello World"))
            );

            yield return new DeserializeCase(
                "Alias",
                "Log Value: (ReadFromFile Path: 'File.txt')",
                Unit.Default,
                "Hello World"
            ).WithFileAction(
                x => x.Setup(f => f.OpenRead("File.txt"))
                    .Returns(new FakeFileStreamAdapter("Hello World"))
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "ValueIf Error",
                new FileRead { Path = Constant("File.txt"), },
                new ErrorBuilder(
                    ErrorCode.ExternalProcessError,
                    "Exception of type 'System.Exception' was thrown."
                )
            ).WithFileAction(
                x => x.Setup(f => f.OpenRead("File.txt"))
                    .Throws<Exception>()
            );
        }
    }
}

}

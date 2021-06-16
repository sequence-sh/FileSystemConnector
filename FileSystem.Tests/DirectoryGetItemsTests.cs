using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Newtonsoft.Json;
using Reductech.EDR.Core;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Connectors.FileSystem.Tests
{

public partial class DirectoryGetItemsTests : StepTestBase<DirectoryGetItems, Array<Entity>>
{
    [Serializable]
    private class Item : IEntityConvertible
    {
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string FullPath { get; set; }

        /// <summary>
        /// True for files, false for directories
        /// </summary>
        [JsonProperty]
        public bool IsFile { get; set; }

        [JsonProperty] public string Directory { get; set; }

        [JsonProperty] public IReadOnlyList<Item>? Children { get; set; }

        [JsonProperty] public string BaseName { get; set; }
        [JsonProperty] public string? Extension { get; set; }

        [JsonProperty] public DateTime CreationTime { get; set; }
        [JsonProperty] public DateTime LastWriteTime { get; set; }
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var mfd = new MockFileData("");

            var root      = new MockFileSystem().Directory.GetCurrentDirectory();
            var separator = new MockFileSystem().Path.DirectorySeparatorChar;

            yield return new StepCase(
                "Basic Case",
                new DirectoryGetItems() { Directory = StaticHelpers.Constant("") },
                new[]
                {
                    new Item()
                    {
                        BaseName      = "Alpha",
                        Children      = null,
                        CreationTime  = mfd.CreationTime.DateTime,
                        Directory     = root,
                        Extension     = ".txt",
                        FullPath      = $"{root}Alpha.txt",
                        IsFile        = true,
                        LastWriteTime = mfd.LastWriteTime.DateTime,
                        Name          = "Alpha.txt"
                    }.ConvertToEntity(),
                    new Item()
                    {
                        BaseName      = "temp",
                        Children      = null,
                        CreationTime  = mfd.CreationTime.DateTime,
                        Directory     = root,
                        Extension     = null,
                        FullPath      = $"{root}temp",
                        IsFile        = false,
                        LastWriteTime = mfd.LastWriteTime.DateTime,
                        Name          = "temp"
                    }.ConvertToEntity()
                }.ToSCLArray()
            ).WithFileSystem(new[] { ("Alpha.txt", "a") });

            yield return new StepCase(
                "Nested one level",
                new DirectoryGetItems() { Directory = StaticHelpers.Constant("myDir") },
                new[]
                {
                    new Item()
                    {
                        BaseName      = "Alpha",
                        Children      = null,
                        CreationTime  = mfd.CreationTime.DateTime,
                        Directory     = "myDir",
                        Extension     = ".txt",
                        FullPath      = $"{root}myDir{separator}Alpha.txt",
                        IsFile        = true,
                        LastWriteTime = mfd.LastWriteTime.DateTime,
                        Name          = "Alpha.txt"
                    }.ConvertToEntity(),
                }.ToSCLArray()
            ).WithFileSystem(new[] { ("myDir/Alpha.txt", "a") });

            yield return new StepCase(
                "Simple pattern",
                new DirectoryGetItems()
                {
                    Directory = StaticHelpers.Constant("myDir"),
                    Pattern   = StaticHelpers.Constant("*.txt")
                },
                new[]
                {
                    new Item()
                    {
                        BaseName      = "Alpha",
                        Children      = null,
                        CreationTime  = mfd.CreationTime.DateTime,
                        Directory     = "myDir",
                        Extension     = ".txt",
                        FullPath      = $"{root}myDir{separator}Alpha.txt",
                        IsFile        = true,
                        LastWriteTime = mfd.LastWriteTime.DateTime,
                        Name          = "Alpha.txt"
                    }.ConvertToEntity(),
                }.ToSCLArray()
            ).WithFileSystem(new[] { ("myDir/Alpha.txt", "a"), ("myDir/Alpha.jpg", "a"), });

            yield return new StepCase(
                "Recursive Case",
                new DirectoryGetItems() { Directory = StaticHelpers.Constant("dira") },
                new[]
                {
                    new Item()
                    {
                        BaseName      = "Alpha",
                        Children      = null,
                        CreationTime  = mfd.CreationTime.DateTime,
                        Directory     = "dira",
                        Extension     = ".txt",
                        FullPath      = $"{root}dira{separator}Alpha.txt",
                        IsFile        = true,
                        LastWriteTime = mfd.LastWriteTime.DateTime,
                        Name          = "Alpha.txt"
                    }.ConvertToEntity(),
                    new Item()
                    {
                        BaseName      = "dirb",
                        CreationTime  = mfd.CreationTime.DateTime,
                        Directory     = "dira",
                        Extension     = null,
                        FullPath      = $"{root}dira{separator}dirb",
                        IsFile        = false,
                        LastWriteTime = mfd.LastWriteTime.DateTime,
                        Name          = "dirb",
                        Children = new List<Item>()
                        {
                            new()
                            {
                                BaseName      = "dirc",
                                CreationTime  = mfd.CreationTime.DateTime,
                                Directory     = "dirb",
                                Extension     = null,
                                FullPath      = $"{root}dira{separator}dirb{separator}dirc",
                                IsFile        = false,
                                LastWriteTime = mfd.LastWriteTime.DateTime,
                                Name          = "dirc",
                                Children = new List<Item>()
                                {
                                    new()
                                    {
                                        BaseName = "Beta",
                                        Children = null,
                                        CreationTime =
                                            mfd.CreationTime.DateTime,
                                        Directory = "dirc",
                                        Extension = ".txt",
                                        FullPath =
                                            $"{root}dira{separator}dirb{separator}dirc{separator}Beta.txt",
                                        IsFile = true,
                                        LastWriteTime =
                                            mfd.LastWriteTime.DateTime,
                                        Name = "Beta.txt"
                                    }
                                }
                            }
                        }
                    }.ConvertToEntity(),
                }.ToSCLArray()
            ).WithFileSystem(new[] { ("dira/Alpha.txt", "a"), ("dira/dirb/dirc/Beta.txt", "a") });
        }
    }
}

}

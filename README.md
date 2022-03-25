# Sequence® FileSystem Connector

[Reductech Sequence®](https://gitlab.com/reductech/sequence) is a collection of
libraries that automates cross-application e-discovery and forensic workflows.

This connector contains Steps to interact with the file system of the host computer.

You can:

    - Read and write files
    - Create, move, copy, and delete files
    - Combine paths
    - Extract compressed files

## Examples

Read the contents of a file

```scala
- <text> = FileRead Path: "MyFile.txt"
- print <text>
```

Write to a file

```scala
- FileWrite Stream: "Hello World" Path: "Filename.txt"
```

# Documentation

Documentation available at https://sequence.sh

# Releases

Can be downloaded from the [Releases page](https://gitlab.com/reductech/sequence/connectors/filesystem/-/releases).

# NuGet Packages

Are available in the [Reductech Nuget feed](https://gitlab.com/reductech/nuget/-/packages).

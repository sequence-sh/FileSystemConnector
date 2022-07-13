# Sequence® FileSystem Connector

[Sequence®](https://sequence.sh) is a collection of libraries for
automation of cross-application e-discovery and forensic workflows.

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

https://sequence.sh

# Download

https://sequence.sh/download

# Try SCL and Core

https://sequence.sh/playground

# Package Releases

Can be downloaded from the [Releases page](https://gitlab.com/reductech/sequence/connectors/filesystem/-/releases).

# NuGet Packages

Release nuget packages are available from [nuget.org](https://www.nuget.org/profiles/Sequence).

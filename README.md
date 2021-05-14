# EDR FileSystem Connector

[Reductech EDR](https://gitlab.com/reductech/edr) is a collection of
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

### [Try FileSystem Connector](https://gitlab.com/reductech/edr/edr/-/releases)

Using [EDR](https://gitlab.com/reductech/edr/edr),
the command line tool for running Sequences.

## Documentation

Documentation is available here: https://docs.reductech.io

## E-discovery Reduct

The PowerShell Connector is part of a group of projects called
[E-discovery Reduct](https://gitlab.com/reductech/edr)
which consists of a collection of [Connectors](https://gitlab.com/reductech/edr/connectors)
and a command-line application for running Sequences, called
[EDR](https://gitlab.com/reductech/edr/edr/-/releases).

# Releases

Can be downloaded from the [Releases page](https://gitlab.com/reductech/edr/connectors/filesystem/-/releases).

# NuGet Packages

Are available in the [Reductech Nuget feed](https://gitlab.com/reductech/nuget/-/packages).

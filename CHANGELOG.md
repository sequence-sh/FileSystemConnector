# v0.15.0 (2022-05-27)

Maintenance release - dependency updates only.

# v0.14.0 (2022-03-25)

Encoding can now be specified when using `FileWrite`.

## Issues Closed in this Release

### New Features

- Add Encoding parameter for FileWrite #44
- Add Security Attributes to Path parameters of Steps #57

# v0.13.0 (2022-01-16)

EDR is now Sequence. The following has changed:

- The GitLab group has moved to https://gitlab.com/reductech/sequence
- The root namespace is now `Reductech.Sequence`
- The documentation site has moved to https://sequence.sh

Everything else is still the same - automation, simplified.

The project has now been updated to use .NET 6.

## Issues Closed in this Release

### Maintenance

- Rename EDR to Sequence #33
- Update Core to support SCLObject types #30
- Upgrade to use .net 6 #29

# v0.12.0 (2021-11-26)

Maintenance release - dependency updates only.

# v0.11.0 (2021-09-03)

Maintenace release - dependency updates and improved testing.

## Issues Closed in this Release

### Maintenance

- Add unit tests to improve coverage #13

# v0.10.0 (2021-07-02)

## Issues Closed in this Release

### New Features

- Add DirectoryGetItems Step #4

### Maintenance

- Update Core to latest and removing SCLSettings #5

# v0.9.0 (2021-05-14)

First release. Versions numbers are aligned with Core.

## Summary of Changes

### Steps

- Moved the following Steps from Core:
  - `DirectoryCopy`
  - `DirectoryExists`
  - `DirectoryMove`
  - `CreateDirectory`
  - `FileCopy`
  - `FileExists`
  - `FileExtract`
  - `FileMove`
  - `FileRead`
  - `FileWrite`
  - `DeleteItem`
  - `PathCombine`

## Issues Closed in this Release

### Maintenance

- Enable publish to connector registry #2
- Move in steps and tests from Core #1





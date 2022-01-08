# FoxProDatabaseExtractor
Tool written in C# with .NET 6.0 that reads a FoxPro 9 database file and dump the contents of all the user tables into individual csv files.

# Requirements
- .NET 6.0 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/6.0
- Visual FoxPro 9.0 - Note that Microsoft [does not offer the installer anymore](https://docs.microsoft.com/en-us/previous-versions/visualstudio/foxpro/mt490121(v=msdn.10)).

# Build

Since Visual FoxPro 9.0 was built for x86 only, this project needs to be built for that platform as well.

- If building from Visual Studio directly, the only platform specified in the solution is x86, so you can build directly into that platform without any manual changes.
- If building using the `dotnet` command directly from the `src` folder, you need to pass the `--arch x86` arguments to force building in x86.

# Usage

```
FoxProDatabaseExtractor.exe <vfpDatabaseFile> <targetCsvFolder>
```

Example:
```
FoxProDatabaseExtractor.exe path/to/database.dbc path/to/directory/to/dump/csv/files/
```

**Note**: Only works on Windows because Visual FoxPro is a Windows-only database system.

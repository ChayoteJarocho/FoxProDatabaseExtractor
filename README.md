# FoxProDatabaseExtractor
Tool written in C# with .NET 6.0 that reads a FoxPro 9 database file and dump the contents of all the user tables into individual csv files.

# Requirements
- .NET 6.0 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/6.0
- Visual FoxPro 9.0 - Note that Microsoft [does not offer the installer anymore](https://docs.microsoft.com/en-us/previous-versions/visualstudio/foxpro/mt490121(v=msdn.10)).

# Usage

```
FoxProDatabaseExtractor.exe <vfpDatabaseFile> <targetCsvFolder>
```

Example:
```
FoxProDatabaseExtractor.exe path/to/database.dbc path/to/directory/to/dump/csv/files/
```

**Note**: Only works on Windows because Visual FoxPro is a Windows-only database system.

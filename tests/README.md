
Use the file `data1.dbc` for testing the console app.

It contains a table named `table1`, which contains one column for each supported FoxPro data type.

| Column name | Data type |
|--|--|
| blob | Blob |
| character | Character |
| characterbin | Character (binary) |
| currency | Currency |
| date | Date |
| datetime | DateTime |
| double | Double |
| float | Float |
| general | General |
| integer | Integer |
| integerautoinc | Integer (AutoInc) |
| logical | Logical |
| memo | Memo |
| memobin | Memo (binary) |
| numeric | Numeric |
| varbin | Varbinary |
| varchar | Varchar |
| varcharbin | Varchar (binary) |

Note - Some of the data types are duplicates / reinterpreted internally as another data type. See `DataType.cs` for more information.
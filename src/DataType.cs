namespace FoxProDatabaseExtractor
{
    /// <summary>
    /// Represents a FoxPro column data type.
    /// The official documentation shows a total of 11 data types, but does not show the actual underlying integer values of each data type:
    /// https://docs.microsoft.com/en-us/sql/odbc/microsoft/visual-foxpro-field-data-types?view=sql-server-ver15
    /// The integer data types were manually inspected and extracted from Microsoft Visual Fox Pro 9.
    /// Also, aaccording to Microsoft Visual FoxPro 9, there are a total of 19 possible fields.
    /// Those additional fields have their own unique name, but their underlying integer value is a duplicate of another existing one.
    /// </summary>
    public enum DataType
    {
        Integer = 3,
        Double = 5,
        Currency = 6,
        Logical = 11,
        General = 128,
        Character = 129,
        Numeric = 131,
        Date = 133,
        DateTime = 135,
        // Duplicate values:
        //   IntegerAutoInc = 3
        //   Blob = 128
        //   MemoBin = 128
        //   VarBinary = 128
        //   CharacterBinary = 129
        //   Memo = 129
        //   VarChar = 129
        //   VarCharBin = 129
        //   Float = 131
    }
}

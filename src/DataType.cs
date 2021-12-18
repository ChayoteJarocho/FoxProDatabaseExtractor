namespace FoxProDatabaseExtractor
{
    /// <summary>
    /// Represents a FoxPro column data type.
    /// https://docs.microsoft.com/en-us/sql/odbc/microsoft/visual-foxpro-field-data-types?view=sql-server-ver15
    /// </summary>
    public enum DataType
    {
        Double    = 'B',
        Character = 'C',
        Date      = 'D',
        Float     = 'F',
        General   = 'G',
        Integer   = 'I',
        Logical   = 'L',
        Memo      = 'M',
        Numeric   = 'N',
        DateTime  = 'T',
        Currency  = 'Y',
    }
}

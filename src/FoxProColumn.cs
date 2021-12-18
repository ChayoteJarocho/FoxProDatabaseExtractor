namespace FoxProDatabaseExtractor
{
    /// <summary>
    /// Represents a column from a FoxPro table.
    /// </summary>
    public class FoxProColumn
    {
        /// <summary>
        /// The column name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// The column data type.
        /// </summary>
        public DataType DataType
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new <see cref="FoxProColumn"/> instance.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <param name="dataType">The data type of the column.</param>
        public FoxProColumn(string name, DataType dataType)
        {
            this.Name = name;
            this.DataType = dataType;
        }
    }
}

namespace FoxProDatabaseExtractor
{
    /// <summary>
    /// Represents a column from a FoxPro table.
    /// </summary>
    public class FoxProColumn
    {
        /// <summary>
        /// Initializes a new <see cref="FoxProColumn"/> instance.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <param name="dataType">The data type of the column.</param>
        public FoxProColumn(string name, DataType dataType)
        {
            Name = name;
            DataType = dataType;
        }

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
        /// Returns the column name.
        /// </summary>
        /// <returns>A string representing the column name.</returns>
        public override string ToString() => Name;
    }
}

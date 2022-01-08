using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace FoxProDatabaseExtractor
{
    /// <summary>
    /// Represents a table from a FoxPro database.
    /// </summary>
    public class FoxProTable
    {
        private const char CsvSeparator = '|';

        // A connection to the FoxPro datasource.
        private readonly OleDbConnection _foxProConnection;

        // A database command object to extract and fill DataSets.
        private readonly OleDbDataAdapter _foxProAdapter;

        // The FoxPro object with the columns schema.
        // Each row in this object describes an actual user column.
        private readonly DataTable _columnsTable;

        // Contains all the user columns in this table.
        private readonly List<FoxProColumn> _foxProColumns;

        private readonly string _selectCommand;

        /// <summary>
        /// Represents the table name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new <see cref="FoxProTable"/> instance.
        /// </summary>
        /// <param name="connection">A valid FoxPro connection. Must have been already opened by the <see cref="FoxProDatabaseConnector"/>.</param>
        /// <param name="row">The data row containing the current table's information.</param>
        public FoxProTable(OleDbConnection connection, DataRow row)
        {
            _foxProConnection = connection;

            Name = row["TABLE_NAME"]?.ToString() ?? throw new NullReferenceException("TABLE_NAME was not found for the current row.");

            Console.WriteLine($"  Extracting table '{Name}'...");
            _columnsTable = _foxProConnection.GetSchema(
                OleDbMetaDataCollectionNames.Columns,
                new string[]
                {
                    string.Empty, // Catalog
                    string.Empty, // Owner
                    Name,         // Table
                    string.Empty  // Column
                }
            );

            _foxProColumns = new List<FoxProColumn>();
            ExtractColumns();

            // SelectCommand depends on generating the columns first.
            _selectCommand = GetSelectCommand();
            _foxProAdapter = new OleDbDataAdapter(_selectCommand, _foxProConnection);
        }

        /// <summary>
        /// Gets the list of column names of this FoxPro table in CSV style: in one line, with a special character as separator.
        /// </summary>
        public string GetJoinedColumnNames()
        {
            List<string> columnNames = new();

            foreach (FoxProColumn column in _foxProColumns)
            {
                columnNames.Add(column.Name);
            }

            return string.Join(CsvSeparator, columnNames);
        }

        /// <summary>
        /// Gets the list of column types of this FoxPro table in CSV style: in one line, with a special character as separator.
        /// </summary>
        public string GetJoinedColumnTypes()
        {
            List<string> columnTypes = new();

            foreach (FoxProColumn column in _foxProColumns)
            {
                columnTypes.Add(column.DataType.ToString());
            }

            return string.Join(CsvSeparator, columnTypes);
        }

        /// <summary>
        /// Yields all the data rows of this FoxPro table in CSV style: in one line, with a special character as separator.
        /// </summary>
        public IEnumerable<string> GetJoinedValuesRows()
        {
            DataTable valuesTable = GetValuesTable();
            List<string> rowValues = new();
            foreach (DataRow row in valuesTable.Rows)
            {
                rowValues.Clear();
                foreach (object? rawValue in row.ItemArray)
                {
                    string safeValue = GetSafeValue(rawValue);
                    rowValues.Add(safeValue);
                }

                string joinedValues = string.Join(CsvSeparator, rowValues.ToArray());
                yield return joinedValues;
            }
        }

        /// <summary>
        /// Generates the SELECT command to extract all the values from this table.
        /// </summary>
        private string GetSelectCommand()
        {
            string columnNames = GetSelectColumnNames();
            return string.Format(FoxProDatabaseConnector.SelectCommandFormatString, columnNames, Name);
        }

        /// <summary>
        /// The values of this FoxPro table.
        /// </summary>
        private DataTable GetValuesTable()
        {
            Console.WriteLine(_selectCommand);

            DataTable table = new();
            try
            {
                _foxProAdapter.Fill(table);
            }
            catch
            {
                _foxProConnection.Close();
                _foxProConnection.Dispose();
                throw;
            }

            return table;
        }

        /// <summary>
        /// Returns the columns of this FoxPro table as a comma-separated string, to be used in a SELECT command.
        /// </summary>
        private string GetSelectColumnNames()
        {
            List<string> columnNames = new();

            foreach (FoxProColumn column in _foxProColumns)
            {
                // If float values are not collected as string, then 'OleDbDataAdapter.Fill(DataTable)' throws InvalidOperationException with the following message:
                // "The provider could not determine the Decimal value. For example, the row was just created, the default for the Decimal column was not available, and the consumer had not yet set a new Decimal value."
                string fixedName = column.DataType switch
                {
                    DataType.Integer or DataType.Logical or DataType.General or DataType.Character or DataType.Date or DataType.DateTime => column.Name,
                    DataType.Double or DataType.Currency or DataType.Numeric => $" VAL(STR({column.Name})) ",
                    _ => throw new NotSupportedException($"Column type not supported: {column.Name}"),
                };
                ;

                columnNames.Add(fixedName);
            }

            return string.Join(',', columnNames);
        }

        /// <summary>
        /// Gets all the columns from this FoxPro table and saves them in the column list.
        /// </summary>
        private void ExtractColumns()
        {
            Console.WriteLine("    Extracting columns...");
            foreach (DataRow row in _columnsTable.Rows)
            {
                string name = row["COLUMN_NAME"]?.ToString() ?? throw new NullReferenceException("COLUMN_NAME was null for current row.");

                Console.WriteLine($"      Extracting column '{name}'...");

                string dataTypeName = row["DATA_TYPE"]?.ToString() ?? throw new NullReferenceException($"DATA_TYPE was null for current row with column name '{name}'.");

                DataType dataType = (DataType)Enum.Parse(typeof(DataType), dataTypeName);
                
                FoxProColumn column = new(name, dataType);

                _foxProColumns.Add(column);
            }
        }
        
        /// <summary>
        /// Evaluates the value of a cell in the row and simplifies some data:
        /// - The string gets trimmed.
        /// - '\\' gets converted to '\\\\'.
        /// - 'True' gets converted to '1'.
        /// - 'False' gets converted to '0'.
        /// </summary>
        /// <param name="rawValue">The original value of the row cell.</param>
        /// <returns>The string with any necessary modifications.</returns>
        private static string GetSafeValue(object? rawValue)
        {
            if (rawValue == null)
            {
                throw new NullReferenceException("Cannot retrieve safe value for a null raw value.");
            }
            string safeValue = (rawValue.ToString() ?? string.Empty).Trim().Replace("\\", "\\\\");

            if (safeValue == "True")
            {
                safeValue = "1";
            }
            else if(safeValue == "False")
            {
                safeValue = "0";
            }

            return safeValue;
        }

        /// <summary>
        /// Returns the table name.
        /// </summary>
        /// <returns>A string representing the table name.</returns>
        public override string ToString() => Name;
    }
}

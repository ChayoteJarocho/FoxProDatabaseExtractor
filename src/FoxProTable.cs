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

        /// <summary>
        /// Initializes a new <see cref="FoxProTable"/> instance.
        /// </summary>
        /// <param name="connection">A valid FoxPro connection. Must have been already opened by the <see cref="FoxProDatabaseConnector"/>.</param>
        /// <param name="row">The data row containing the current table's information.</param>
        public FoxProTable(OleDbConnection connection, DataRow row)
        {
            _foxProConnection = connection;

            Name = row["TABLE_NAME"].ToString();

            Console.WriteLine($"  Extracting table '{Name}'...");
            _columnsTable = _foxProConnection.GetSchema(
                OleDbMetaDataCollectionNames.Columns,
                new string[]
                {
                    null, // Catalog
                    null, // Owner
                    Name, // Table
                    null  // Column
                }
            );

            _foxProColumns = new List<FoxProColumn>();
            ExtractColumns();

            // SelectCommand depends on generating the columns first.
            _foxProAdapter = new OleDbDataAdapter(SelectCommand, _foxProConnection);
        }

        /// <summary>
        /// Represents the table name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list of column names of this FoxPro table in CSV style: in one line, with a special character as separator.
        /// </summary>
        public string JoinedColumnNames
        {
            get
            {
                List<string> columnNames = new();

                foreach (FoxProColumn column in _foxProColumns)
                {
                    columnNames.Add(column.Name);
                }

                return string.Join(CsvSeparator, columnNames);
            }
        }

        /// <summary>
        /// Gets the list of column types of this FoxPro table in CSV style: in one line, with a special character as separator.
        /// </summary>
        public string JoinedColumnTypes
        {
            get
            {
                List<string> columnTypes = new();

                foreach (FoxProColumn column in _foxProColumns)
                {
                    columnTypes.Add(column.DataType.ToString());
                }

                return string.Join(CsvSeparator, columnTypes);
            }
        }

        /// <summary>
        /// Yields all the data rows of this FoxPro table in CSV style: in one line, with a special character as separator.
        /// </summary>
        public IEnumerable<string> JoinedValuesRows
        {
            get
            {
                foreach (DataRow row in ValuesTable.Rows)
                {
                    List<string> rowValues = new();

                    foreach (object rawValue in row.ItemArray)
                    {
                        string safeValue = GetSafeValue(rawValue);
                        rowValues.Add(safeValue);
                    }

                    string joinedValues = string.Join(CsvSeparator, rowValues.ToArray());

                    yield return joinedValues;
                }
            }
        }

        /// <summary>
        /// Generates the SELECT command to extract all the values from this table.
        /// </summary>
        private string SelectCommand =>  string.Format(FoxProDatabaseConnector.SelectCommandFormatString, SelectColumnNames, Name);

        /// <summary>
        /// The values of this FoxPro table.
        /// </summary>
        private DataTable ValuesTable
        {
            get
            {
                Console.WriteLine(SelectCommand);

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
        }

        /// <summary>
        /// Returns the columns of this FoxPro table as a comma-separated string, to be used in a SELECT command.
        /// </summary>
        private string SelectColumnNames
        {
            get
            {
                List<string> columnNames = new();

                foreach (FoxProColumn column in _foxProColumns)
                {
                    string fixedName = column.DataType switch
                    {
                        DataType.Numeric => $" VAL(STR({column.Name})) ",
                        _ => column.Name,
                    };
                    columnNames.Add(fixedName);
                }

                return string.Join(',', columnNames);
            }
        }

        /// <summary>
        /// Gets all the columns from this FoxPro table and saves them in the column list.
        /// </summary>
        private void ExtractColumns()
        {
            Console.WriteLine("    Extracting columns...");
            foreach (DataRow row in _columnsTable.Rows)
            {
                string name = row["COLUMN_NAME"].ToString();

                Console.WriteLine($"      Extracting column '{name}'...");

                DataType dataType = (DataType)Enum.Parse(typeof(DataType), row["DATA_TYPE"].ToString());
                
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
        private static string GetSafeValue(object rawValue)
        {
            string safeValue = rawValue.ToString().Trim().Replace("\\", "\\\\");

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
    }
}

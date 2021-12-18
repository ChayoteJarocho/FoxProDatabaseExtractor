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
        /// <summary>
        /// Generates the SELECT command to extract all the values from this table.
        /// </summary>
        private string SelectCommand
        {
            get
            {
                return string.Format(
                  FoxProDatabaseConnector.SelectCommandFormatString,
                  SelectColumnNames,
                  Name);
            }
        }

        /// <summary>
        /// A connection to the FoxPro datasource.
        /// </summary>
        private OleDbConnection FoxProConnection
        {
            get;
            set;
        }
        
        /// <summary>
        /// A database command object to extract and fill DataSets.
        /// </summary>
        private OleDbDataAdapter FoxProAdapter
        {
            get;
            set;
        }

        /// <summary>
        /// The columns of this FoxPro table.
        /// </summary>
        private DataTable ColumnsTable
        {
            get;
            set;
        }

        /// <summary>
        /// The values of this FoxPro table.
        /// </summary>
        private DataTable ValuesTable
        {
            get
            {
                Console.WriteLine(SelectCommand);

                FoxProAdapter = new OleDbDataAdapter(SelectCommand, FoxProConnection);
                DataTable table = new DataTable();
                try
                {
                    FoxProAdapter.Fill(table);
                }
                catch(Exception e)
                {
                    FoxProConnection.Close();
                    FoxProConnection.Dispose();
                    throw new Exception("ValuesTable exception: ", e);
                }

                return table;
            }
        }

        /// <summary>
        /// Contains all the columns from this FoxPro table.
        /// </summary>
        public List<FoxProColumn> FoxProColumns;

        /// <summary>
        /// Represents the table name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the columns of this FoxPro table as a comma-separated string, to be used in a SELECT command.
        /// </summary>
        public string SelectColumnNames
        {
            get
            {
                List<string> columnNames = new List<string>();

                foreach (FoxProColumn column in FoxProColumns)
                {
                    string fixedName;
                    switch (column.DataType)
                    {
                        case DataType.Numeric:
                            fixedName = String.Format(" VAL(STR({0})) ", column.Name);
                            break;
                        default:
                            fixedName = column.Name;
                            break;
                    }
                    columnNames.Add(fixedName);
                }

                return String.Join(", ", columnNames);
            }
        }

        /// <summary>
        /// Gets the list of column names of this FoxPro table in CSV style: in one line, with a special character as separator.
        /// </summary>
        public string JoinedColumnNames
        {
            get
            {
                List<string> columnNames = new List<string>();

                foreach (FoxProColumn column in FoxProColumns)
                {
                    columnNames.Add(column.Name);
                }

                return String.Join(FoxProDatabaseConnector.Separator, columnNames);
            }
        }

        /// <summary>
        /// Gets the list of column types of this FoxPro table in CSV style: in one line, with a special character as separator.
        /// </summary>
        public string JoinedColumnTypes
        {
            get
            {
                List<string> columnTypes = new List<string>();

                foreach (FoxProColumn column in FoxProColumns)
                {
                    columnTypes.Add(column.DataType.ToString());
                }

                return String.Join(FoxProDatabaseConnector.Separator, columnTypes);
            }
        }

        /// <summary>
        /// Yields all the data rows of this FoxPro table in CSV style: in one line, with a special character as separator.
        /// </summary>
        public IEnumerable<string> JoinedValuesRows
        {
            get
            {
                DataTable dataTable = ValuesTable;

                foreach (DataRow row in dataTable.Rows)
                {
                    List<string> rowValues = new List<string>();

                    foreach (object rawValue in row.ItemArray)
                    {
                        string safeValue = GetSafeValue(rawValue);
                        rowValues.Add(safeValue);
                    }

                    string joinedValues = String.Join(FoxProDatabaseConnector.Separator, rowValues.ToArray());

                    yield return joinedValues;
                }
            }
        }

        /// <summary>
        /// Initializes a new <see cref="FoxProTable"/> instance.
        /// </summary>
        /// <param name="connection">A valid FoxPro connection. Must have been already opened by the <see cref="FoxProDatabaseConnector"/>.</param>
        /// <param name="row">The data row containing the current table's information.</param>
        public FoxProTable(OleDbConnection connection, DataRow row)
        {
            FoxProConnection = connection;
            
            Name = row["TABLE_NAME"].ToString();

            Console.WriteLine("  Extracting table '{0}'...", Name);

            ColumnsTable = FoxProConnection.GetSchema(
                OleDbMetaDataCollectionNames.Columns,
                new string[]
                {
                    null, // Catalog
                    null, // Owner
                    Name, // Table
                    null  // Column
                }
            );
            
            ExtractColumns();
        }

        /// <summary>
        /// Gets all the columns from this FoxPro table and saves them in the column list.
        /// </summary>
        private void ExtractColumns()
        {
            Console.WriteLine("    Extracting columns...");

            FoxProColumns = new List<FoxProColumn>();

            foreach (DataRow row in ColumnsTable.Rows)
            {
                string name = row["COLUMN_NAME"].ToString();

                Console.WriteLine("      Extracting column '{0}'...", name);

                DataType dataType = (DataType)Enum.Parse(typeof(DataType), row["DATA_TYPE"].ToString());
                
                if(Name == "cheques")
                {
                    Console.WriteLine("[{0}|{1}]", name, dataType);
                }

                FoxProColumn column = new FoxProColumn(name, dataType);

                FoxProColumns.Add(column);
            }
        }
        
        /// <summary>
        /// Evaluates the value of a cell in the row and simplifies some data.
        /// </summary>
        /// <param name="rawValue">The original value of the row cell.</param>
        /// <returns></returns>
        private string GetSafeValue(object rawValue)
        {
            string safeValue = rawValue.ToString().Trim();

            safeValue.Replace("\\", "\\\\");

            switch (safeValue)
            {
                case "True":
                    safeValue = "1";
                    break;
                case "False":
                    safeValue = "0";
                    break;
                default:
                    break;
            }

            return safeValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace FoxProDatabaseExtractor
{
    /// <summary>
    /// Controls the connection to the FoxPro database and data extraction.
    /// </summary>
    public class FoxProDatabaseConnector
    {
        public const string Separator = "|";
        public const string SelectCommandFormatString = @"SELECT {0} FROM {1}";

        private const string FoxProConnectionFormatString = @"Provider=VFPOLEDB.1;Data Source={0};";

        public List<FoxProTable> FoxProTables;

        /// <summary>
        /// The connection to the FoxPro data source.
        /// </summary>
        private OleDbConnection FoxProConnection
        {
            get;
            set;
        }

        /// <summary>
        /// The absolute path to the FoxPro database file.
        /// </summary>
        private string DatabasePath
        {
            get;
            set;
        }

        /// <summary>
        /// The FoxPro schema table objects.
        /// Each row in this object describes an actual user table.
        /// </summary>
        private DataTable FoxProDataTables
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new <see cref="FoxProDatabaseConnector"/> instance.
        /// </summary>
        /// <param name="databasePath">The absolute path to the FoxPro database file.</param>
        public FoxProDatabaseConnector(string databasePath)
        {
            DatabasePath = databasePath;

            StartConnection();

            try
            {
                ExtractTables();
            }
            catch(Exception e)
            {
                EndConnection();
                throw new Exception("Unexpected error: ", e);
            }
        }

        /// <summary>
        /// Closes and disposes the FoxPro database connection.
        /// </summary>
        public void EndConnection()
        {
            if (FoxProConnection.State == ConnectionState.Open)
            {
                Console.WriteLine("Closing the database connection...");

                FoxProConnection.Close();
                FoxProConnection.Dispose();
            }
        }

        /// <summary>
        /// Opens a connection to the FoxPro database.
        /// </summary>
        private void StartConnection()
        {
            Console.WriteLine("Starting connection to database...");

            string foxProConnectionString = string.Format(FoxProConnectionFormatString, DatabasePath);

            FoxProConnection = new OleDbConnection(foxProConnectionString);
            FoxProConnection.Open();

            FoxProDataTables = FoxProConnection.GetSchema(OleDbMetaDataCollectionNames.Tables);
        }

        /// <summary>
        /// Iterates through the database tables and saves them to a list of FoxProTable objects.
        /// </summary>
        private void ExtractTables()
        {
            Console.WriteLine("Extracting tables...");

            FoxProTables = new List<FoxProTable>();

            foreach (DataRow row in FoxProDataTables.Rows)
            {
                FoxProTable foxProTable = new FoxProTable(FoxProConnection, row);
                FoxProTables.Add(foxProTable);
            }
        }
    }
}

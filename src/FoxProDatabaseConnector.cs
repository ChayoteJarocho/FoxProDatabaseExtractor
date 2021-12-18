using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace FoxProDatabaseExtractor
{
    /// <summary>
    /// Controls the connection to the FoxPro database and data extraction.
    /// </summary>
    public class FoxProDatabaseConnector : IDisposable
    {
        public const string SelectCommandFormatString = @"SELECT {0} FROM {1}";
        private const string FoxProConnectionFormatString = @"Provider=VFPOLEDB.1;Data Source={0};";

        // The connection to the FoxPro data source.
        private readonly OleDbConnection _foxProConnection;

        // The absolute path to the FoxPro database file.
        private readonly string _databasePath;

        // The FoxPro object with the tables schema.
        // Each row in this object describes an actual user table.
        private readonly DataTable _foxProDataTables;

        // Contains all the user tables in this database.
        private readonly List<FoxProTable> _foxProTables;

        /// <summary>
        /// Initializes a new <see cref="FoxProDatabaseConnector"/> instance and starts a connection to the database.
        /// </summary>
        /// <param name="databasePath">The absolute path to the FoxPro database file.</param>
        public FoxProDatabaseConnector(string databasePath)
        {
            _databasePath = databasePath;

            string foxProConnectionString = string.Format(FoxProConnectionFormatString, _databasePath);
            _foxProConnection = new OleDbConnection(foxProConnectionString);

            Console.WriteLine("Starting connection to database...");
            _foxProConnection.Open();

            _foxProDataTables = _foxProConnection.GetSchema(OleDbMetaDataCollectionNames.Tables);

            _foxProTables = new List<FoxProTable>();

            try
            {
                ExtractTables();
            }
            catch
            {
                EndConnection();
                throw;
            }
        }

        /// <summary>
        /// The list of user tables in the FoxPro database.
        /// </summary>
        public List<FoxProTable> FoxProTables => _foxProTables;

        /// <summary>
        /// Ends the connection to the database.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                EndConnection();
            }
        }

        /// <summary>
        /// Ends the connection to the database.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Closes and disposes the FoxPro database connection.
        /// </summary>
        private void EndConnection()
        {
            if (_foxProConnection.State == ConnectionState.Open)
            {
                Console.WriteLine("Closing the database connection...");

                _foxProConnection.Close();
                _foxProConnection.Dispose();
            }
        }

        /// <summary>
        /// Iterates through the database tables and saves them to a list of FoxProTable objects.
        /// </summary>
        private void ExtractTables()
        {
            Console.WriteLine("Extracting tables...");

            foreach (DataRow row in _foxProDataTables.Rows)
            {
                FoxProTable foxProTable = new(_foxProConnection, row);
                _foxProTables.Add(foxProTable);
            }
        }
    }
}

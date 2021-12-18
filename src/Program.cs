using System;
using System.IO;

namespace FoxProDatabaseExtractor
{
    public class Program
    {
        private const string ExtensionFoxProDB = ".dbc";

        private static FoxProDatabaseConnector connector = null;

        /// <summary>
        /// Writes a line of text in the file.
        /// </summary>
        /// <param name="writer">The file stream open for writing.</param>
        /// <param name="text">The line of text to write.</param>
        private static void PrintLineToCSVFile(StreamWriter writer, string text)
        {
            writer.WriteLine(text);
        }

        /// <summary>
        /// Iterates through all the FoxPro tables and saves each one of them in a CSV file.
        /// </summary>
        private static void SaveFoxProTablesToCSVFiles(string targetCsvDirFullPath)
        {
            Console.WriteLine("Creating target directory...");
            Directory.CreateDirectory(targetCsvDirFullPath);
            Console.WriteLine("Target directory successfully created.");

            foreach (FoxProTable table in connector.FoxProTables)
            {
                string fileName = String.Format("{0}.csv", table.Name);
                string filePath = Path.Combine(targetCsvDirFullPath, fileName);

                if(File.Exists(filePath))
                {
                    Console.WriteLine("File '{0}' exists. Deleting...", fileName);
                    File.Delete(filePath);
                }

                Console.WriteLine("  Writing to file '{0}'...", fileName);

                StreamWriter writer = File.CreateText(filePath);

                PrintLineToCSVFile(writer, table.JoinedColumnTypes);
                PrintLineToCSVFile(writer, table.JoinedColumnNames);
                
                foreach (string joinedValues in table.JoinedValuesRows)
                {
                    PrintLineToCSVFile(writer, joinedValues);
                }
                
                writer.Close();

                Console.WriteLine("Finished writing file {0}.", filePath);
            }
        }

        /// <summary>
        /// Prints the usage example.
        /// </summary>
        private static void PrintHelpAndExit()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("{0} database{1} TargetCsvDir/", AppDomain.CurrentDomain.FriendlyName, ExtensionFoxProDB);
            Environment.Exit(-1);
        }

        public static void Main(string[] args)
        {
            // Verify the command was executed with one argument
            if (args == null || args.Length != 2 || string.IsNullOrWhiteSpace(args[0]) || string.IsNullOrWhiteSpace(args[1]))
            {
                Console.WriteLine("Incorrect command usage.");
                PrintHelpAndExit();
            }

            string databaseFullPath = Path.GetFullPath(args[0]);
            string extension = Path.GetExtension(databaseFullPath);
            string targetCsvDirFullPath = Path.GetFullPath(args[1]);

            if (extension != ExtensionFoxProDB)
            {
                Console.WriteLine("Unexpected file extension '{0}'. It should be '{1}'", extension, ExtensionFoxProDB);
                PrintHelpAndExit();
            }
            else if (!File.Exists(databaseFullPath))
            {
                Console.WriteLine("Database path does not exist: {0}", databaseFullPath);
                PrintHelpAndExit();
            }
            else if (Directory.Exists(targetCsvDirFullPath))
            {
                Console.WriteLine("Target directory already exists: {0}", targetCsvDirFullPath);
                PrintHelpAndExit();
            }

            connector = new FoxProDatabaseConnector(databaseFullPath);

            SaveFoxProTablesToCSVFiles(targetCsvDirFullPath);
            
            connector.EndConnection();

            Console.WriteLine("Finished! Press enter to exit.");
            Console.ReadLine();
        }
    }
}

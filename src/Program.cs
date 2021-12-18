using System;
using System.IO;

namespace FoxProDatabaseExtractor
{
    public class Program
    {
        private const string ExtensionFoxProDB = ".dbc";

        /// <summary>
        /// Iterates through all the FoxPro tables and saves each one of them in a CSV file.
        /// </summary>
        private static void SaveFoxProTablesToCSVFiles(FoxProDatabaseConnector connector, string targetCsvDirFullPath)
        {
            Console.WriteLine("Creating target directory...");
            Directory.CreateDirectory(targetCsvDirFullPath);
            Console.WriteLine("Target directory successfully created.");

            foreach (FoxProTable table in connector.FoxProTables)
            {
                string fileName = $"{table.Name}.csv";
                string filePath = Path.Combine(targetCsvDirFullPath, fileName);

                if(File.Exists(filePath))
                {
                    Console.WriteLine($"File '{fileName}' exists. Deleting...");
                    File.Delete(filePath);
                }

                Console.WriteLine($"  Writing to file '{fileName}'...");

                StreamWriter writer = File.CreateText(filePath);

                writer.WriteLine(table.JoinedColumnTypes);
                writer.WriteLine(table.JoinedColumnNames);
                
                foreach (string joinedValues in table.JoinedValuesRows)
                {
                    writer.WriteLine(joinedValues);
                }
                
                writer.Close();

                Console.WriteLine($"Finished writing file {filePath}.");
            }
        }

        /// <summary>
        /// Prints the usage example.
        /// </summary>
        private static void PrintHelpAndExit()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"{AppDomain.CurrentDomain.FriendlyName} database{ExtensionFoxProDB} TargetCsvDir/");
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
                Console.WriteLine($"Unexpected file extension '{extension}'. It should be '{ExtensionFoxProDB}'");
                PrintHelpAndExit();
            }
            else if (!File.Exists(databaseFullPath))
            {
                Console.WriteLine($"Database path does not exist: {databaseFullPath}");
                PrintHelpAndExit();
            }
            else if (Directory.Exists(targetCsvDirFullPath))
            {
                Console.WriteLine($"Target directory already exists: {targetCsvDirFullPath}");
                PrintHelpAndExit();
            }

            using FoxProDatabaseConnector connector = new(databaseFullPath);

            SaveFoxProTablesToCSVFiles(connector, targetCsvDirFullPath);
            
            Console.WriteLine("Finished! Press enter to exit.");
            Console.ReadLine();
        }
    }
}

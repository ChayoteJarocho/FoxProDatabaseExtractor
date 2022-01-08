using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FoxProDatabaseExtractor
{
    public class Program
    {
        private const string ExtensionFoxProDB = ".dbc";

        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
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
                Console.Write("Would you like to delete the target directory and recreate it? [Y|N]: ");
                string answer = Console.ReadLine() ?? "N";
                if (answer.ToUpperInvariant() == "Y")
                {
                    Console.WriteLine($"Deleting target directory: {targetCsvDirFullPath}");
                    Directory.Delete(targetCsvDirFullPath, recursive: true);
                }
                else
                {
                    PrintHelpAndExit();
                }
            }

            using FoxProDatabaseConnector connector = new(databaseFullPath);

            SaveFoxProTablesToCSVFiles(connector, targetCsvDirFullPath);

            Console.WriteLine("Finished! Press enter to exit.");
            Console.ReadLine();
        }

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

                string joinedColumnTypes = table.GetJoinedColumnTypes();
                writer.WriteLine(joinedColumnTypes);
                string joinedColumnNames = table.GetJoinedColumnNames();
                writer.WriteLine(joinedColumnNames);

                IEnumerable<string> joinedValuesRows = table.GetJoinedValuesRows();
                foreach (string joinedValues in joinedValuesRows)
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
        [DoesNotReturn]
        private static void PrintHelpAndExit()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"{AppDomain.CurrentDomain.FriendlyName} database{ExtensionFoxProDB} TargetCsvDir/");
            Environment.Exit(-1);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cerberus.Logic;
using CommandLine;
using CommandLine.Text;
using BigEndianBinaryReader;

namespace Cerberus.CLI
{
    class Program
    {
        /// <summary>
        /// File Extensions we accept
        /// </summary>
        static readonly string[] AcceptedExtensions =
        {
            ".ff",
            ".gsc",
            ".csc",
            ".gscc",
            ".cscc",
        };

        static bool Disassemble = false;

        /// <summary>
        /// Directory of the processed scripts
        /// </summary>
        static readonly string ProcessDirectory = "ProcessedScripts";

        /// <summary>
        /// Supported Hash Tables
        /// </summary>
        static readonly Dictionary<string, Dictionary<uint, string>> HashTables = new Dictionary<string, Dictionary<uint, string>>()
        {
            { "BlackOps3", new Dictionary<uint, string>() },
        };

        /// <summary>
        /// Loads in required hash tables
        /// </summary>
        static void LoadHashTables()
        {
            foreach (var hashTable in HashTables)
            {
                if (!File.Exists(hashTable.Key + ".txt"))
                    continue;

                string[] lines = File.ReadAllLines(hashTable.Key + ".txt");
                foreach (string line in lines)
                {
                    string lineTrim = line.Trim();
                
                    // Ignore comment lines
                    if (!lineTrim.StartsWith("#"))
                    {
                        string[] lineSplit = lineTrim.Split(',');
                
                        if (lineSplit.Length > 1)
                        {
                            // Parse as hex, without 0x
                            if (uint.TryParse(lineSplit[0].TrimStart('0', 'x'), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var hash))
                            {
                                hashTable.Value[hash] = lineSplit[1];
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Prints help output
        /// </summary>
        static void PrintHelp()
        {
            Console.WriteLine(": Example: Cerberus.CLI [options] <files (.gsc|.csc|.gscc|.cscc|.ff)>");
            Console.WriteLine(": Options: ");

            Console.WriteLine(":\t-d, --disassemble    Disassembles the script/s.");
        }

        /// <summary>
        /// Extract script files from fastfile
        /// </summary>
        /// <param name="filePath"></param>
        static void ProcessFastFile(string filePath)
        {
            var files = FastFile.Decompress(filePath, filePath + ".output");

            foreach (var file in files)
            {
                Console.WriteLine(": Found {0}", file);
            }

            File.Delete(filePath + ".output");
        }

        /// <summary>
        /// Processes a script file
        /// </summary>
        /// <param name="filePath"></param>
        static void ProcessScript(string filePath)
        {
            var reader = new Reader(filePath);

            using (var script = ScriptBase.LoadScript(reader, HashTables))
            {
                var outputPath = Path.Combine(ProcessDirectory, script.Game, script.FilePath);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                if (Disassemble)
                {
                    File.WriteAllText(outputPath + ".script_asm" + Path.GetExtension(outputPath), script.Disassemble());
                }

                File.WriteAllText(outputPath + ".decompiled" + Path.GetExtension(outputPath), script.Decompile());
            }

            reader.Close();
        }

        static void ParseFiles(string file)
        {
            try
            {
                if (AcceptedExtensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    Console.WriteLine(": Processing {0}...", Path.GetFileName(file));

                    switch (Path.GetExtension(file).ToLower())
                    {
                        case ".gsc":
                        case ".csc":
                        case ".gscc":
                        case ".cscc":
                            {
                                ProcessScript(file);
                                break;
                            }
                        case ".ff":
                            {
                                ProcessFastFile(file);
                                break;
                            }
                    }

                    Console.WriteLine(": Processed {0} successfully.", Path.GetFileName(file));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(": An error has occured while processing {0}: {1}", Path.GetFileName(file), e.Message);
                Console.WriteLine(e);
            }
}

        /// <summary>
        /// Main Entry Point
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine(": ----------------------------------------------------------");
            Console.WriteLine(": Cerberus Command Line - Black Ops II/III Last Gen Script Decompiler / Extractor");
            Console.WriteLine(": Developed by Scobalula, last gen port by CraftyCritter");
            Console.WriteLine(": Version: {0}", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine(": ----------------------------------------------------------");
            var ArgOffset = 0;

            // Force working directory back to exe
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            Console.WriteLine(": Exporting to: {0}", Directory.GetCurrentDirectory());

            LoadHashTables();

            if(args.Length == 0)
            {
                PrintHelp();
                Console.WriteLine(": Execution completed successfully, press Enter to exit.");
                Console.ReadLine();
                return;
            }

            if (args[0].Equals("--disassemble"))
            {
                Disassemble = true;
                ArgOffset = 1;
            }

            if((File.GetAttributes(args[ArgOffset]) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                foreach(string file in Directory.GetFiles(args[ArgOffset], "*.*", SearchOption.AllDirectories))
                {
                    ParseFiles(file);
                }
            }
            else
            {
                ParseFiles(args[ArgOffset]);
            }

            GC.Collect();

            Console.WriteLine(": Execution completed successfully, press Enter to exit.");
            Console.ReadLine();
        }
    }
}
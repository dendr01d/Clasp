using Clasp;
using Clasp.ExtensionMethods;

namespace Standalone
{
    internal class Program
    {
        private const string EXE_NAME = "clasp";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string sourceDir = string.Empty;
            string moduleName = string.Empty;

            string inputPath = string.Empty;
            StreamWriter outputStream = new StreamWriter(Console.OpenStandardOutput(), System.Text.Encoding.Default)
            {
                AutoFlush = true
            };
            bool consoleOutput = true;


            if (args.Length < 2 || args.Length > 3)
            {
                PrintHelp(outputStream);
                PauseConsole(outputStream, consoleOutput);
                return;
            }

            if (args.Length >= 2)
            {
                sourceDir = args[0];

                string fileName = Path.HasExtension(args[1])
                    ? args[1]
                    : string.Format("{0}.clsp", args[1]);

                moduleName = Path.GetFileNameWithoutExtension(args[1]);

                inputPath = Path.Combine(sourceDir, fileName);

                if (!File.Exists(inputPath))
                {
                    PrintHelp(outputStream);
                    PauseConsole(outputStream, consoleOutput);
                    return;
                }
            }

            if (args.Length >= 3)
            {
                string destFile = Path.GetFullPath(args[1]);
                if (!Path.EndsInDirectorySeparator(destFile)
                    && Path.GetDirectoryName(destFile) is string destDir)
                {
                    Directory.CreateDirectory(destDir);
                    File.Create(destFile);
                    outputStream = new StreamWriter(File.OpenWrite(destFile), System.Text.Encoding.Default);
                    consoleOutput = false;
                }
                else
                {
                    PrintHelp(outputStream);
                    PauseConsole(outputStream, consoleOutput);
                    return;
                }
            }

            try
            {
                string output = RunProcessor(sourceDir, outputStream);
                if (consoleOutput)
                {
                    Console.WriteLine(output);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, outputStream);
            }

            if (!consoleOutput)
            {
                outputStream.Flush();
                outputStream.Close();
            }
            else
            {
                PauseConsole(outputStream, consoleOutput);
            }
        }

        private static void PrintHelp(StreamWriter outputStream)
        {
            outputStream.WriteLine("{0} Usage:", EXE_NAME);
            outputStream.WriteLine("   - {0} <source dir> <module path>", EXE_NAME);
            outputStream.WriteLine("   - {0} <source dir> <module path> <destination>", EXE_NAME);
            outputStream.WriteLine();
            outputStream.WriteLine(" <source dir>: The root directory of the CLASP code repository.");
            outputStream.WriteLine("<module name>: The name of the CLASP module to be executed.");
            outputStream.WriteLine("<destination>: The path to an output file, if provided.");
        }

        private static void PauseConsole(StreamWriter outputStream, bool consoleOutput)
        {
            if (consoleOutput)
            {
                outputStream.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
            }
        }

        private static string RunProcessor(string inputFilePath, StreamWriter oStream)
        {
            Processor pross = new Processor(oStream);
            return pross.Process(inputFilePath);
        }

        private static void HandleException(Exception ex, StreamWriter oStream)
        {
            ex.PrintExceptionInfo(oStream);
        }
    }
}

using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.PortableExecutable;

namespace Clasp
{
    public static class Terminal
    {
        public static void RunConsole()
        {
            Stream input = Console.OpenStandardInput();
            Stream output = Console.OpenStandardOutput();
            Run(input, output, output);
        }

        private const string _SHOW_MACHINE_CMD = "show";
        private const string _PAUSE_MACHINE_CMD = "pause";
        private const string _QUIT_CMD = "quit";

        private static bool _showingSteps = false;
        private static bool _pausing = false;

        private static void Run(Stream inputStream, Stream outputStream, Stream errorStream)
        {
            StreamReader reader = new(inputStream);
            StreamWriter writer = new(outputStream);
            StreamWriter errors = new(errorStream);

            writer.AutoFlush = true;
            errors.AutoFlush = true;

            writer.WriteLine("CLASP Terminal");
            writer.WriteLine($"* \"{_SHOW_MACHINE_CMD}\" to toggle machine printing");
            writer.WriteLine($"* \"{_PAUSE_MACHINE_CMD}\" to toggle step-by-step pausing");
            writer.WriteLine($"* \"{_QUIT_CMD}\" to exit");

            string? input = string.Empty;
            string output = string.Empty;

            Environment scope = GlobalEnvironment.Standard();

            while (input != _QUIT_CMD)
            {
                writer.WriteLine();
                writer.Write("> ");
                input = reader.ReadLine();
                output = string.Empty;

                switch (input)
                {
                    case _SHOW_MACHINE_CMD:
                        _showingSteps = !_showingSteps;
                        output = "ok";
                        break;

                    case _PAUSE_MACHINE_CMD:
                        _pausing = !_pausing;
                        output = "ok";
                        break;

                    default:
                        if (!string.IsNullOrWhiteSpace(input))
                        {
                            try
                            {
                                Expression result = Evaluator.Evaluate(Parser.Parse(input), scope);
                                output = result.ToPrinted();
                            }
                            catch (Exception ex)
                            {
                                errors.WriteLine("ERROR: " + ex.Message);
                                errors.WriteLine(ex.StackTrace);
                            }
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(output))
                {
                    writer.WriteLine(output);
                }
            }

            return;
        }
    }
}

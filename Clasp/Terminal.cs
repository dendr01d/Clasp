using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.PortableExecutable;

namespace Clasp
{
    public static class Terminal
    {
        public static void RunConsole()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Stream input = Console.OpenStandardInput();
            Stream output = Console.OpenStandardOutput();
            Run(input, output, output);
        }

        private static readonly string _header = File.ReadAllText(@".\Header.txt");

        private static readonly System.Text.Encoding CharFormat = System.Text.Encoding.Default;

        private const string _SHOW_INPUT_STEPS_CMD = "mirror";
        private const string _SHOW_MACHINE_CMD = "show";
        private const string _PAUSE_MACHINE_CMD = "pause";
        private const string _CLEAR_SCREEN_CMD = "clear";
        private const string _RELOAD_ENV_CMD = "reload";
        private const string _TIMER_CMD = "timer";
        private const string _QUIT_CMD = "quit";

        private static bool _showingInput = false;
        private static bool _showingSteps = false;
        private static bool _pausing = false;

        private static void Run(Stream inputStream, Stream outputStream, Stream errorStream)
        {
            StreamReader reader = new(inputStream, CharFormat);
            StreamWriter writer = new(outputStream, CharFormat);
            StreamWriter errors = new(errorStream, CharFormat);

            writer.AutoFlush = true;
            errors.AutoFlush = true;

            bool showIntro = true;
            bool showTimer = false;

            string? input = string.Empty;
            string output = string.Empty;

            Environment scope = GlobalEnvironment.LoadStandard();

            while (input != _QUIT_CMD)
            {
                if (showIntro)
                {
                    writer.WriteLine(_header);
                    //writer.WriteLine($"(Enc {CharFormat.EncodingName})");
                    writer.WriteLine($"∙ \"{_SHOW_INPUT_STEPS_CMD}\" to toggle input-mirroring");
                    writer.WriteLine($"∙ \"{_SHOW_MACHINE_CMD}\" to toggle machine printing");
                    writer.WriteLine($"∙ \"{_PAUSE_MACHINE_CMD}\" to toggle step-by-step pausing");
                    writer.WriteLine($"∙ \"{_CLEAR_SCREEN_CMD}\" to clear the screen");
                    writer.WriteLine($"∙ \"{_RELOAD_ENV_CMD}\" to reload the std env from file");
                    writer.WriteLine($"∙ \"{_TIMER_CMD}\" to display the computation timer");
                    writer.WriteLine($"∙ \"{_QUIT_CMD}\" to exit");
                    showIntro = false;
                }

                writer.WriteLine();
                writer.Write("> ");
                input = reader.ReadLine();
                output = string.Empty;

                switch (input)
                {

                    case _SHOW_INPUT_STEPS_CMD:
                        _showingInput = !_showingInput;
                        output = ToggledMsg(_showingInput);
                        break;

                    case _SHOW_MACHINE_CMD:
                        _showingSteps = !_showingSteps;
                        output = ToggledMsg(_showingSteps);
                        break;

                    case _PAUSE_MACHINE_CMD:
                        _pausing = !_pausing;
                        output = ToggledMsg(_pausing);
                        break;

                    case _TIMER_CMD:
                        showTimer = !showTimer;
                        output = ToggledMsg(showTimer);
                        break;

                    case _CLEAR_SCREEN_CMD:
                        Console.Clear();
                        showIntro = true;
                        output = string.Empty;
                        break;

                    case _RELOAD_ENV_CMD:
                        scope = GlobalEnvironment.LoadStandard();
                        output = Symbol.Ok.Print();
                        break;

                    default:
                        if (!string.IsNullOrWhiteSpace(input))
                        {
                            int i = 1;

                            IEnumerable<Token> tokens = Lexer.Lex(input);
                            IEnumerable<Expression> exprs = Parser.ParseText(input);

                            if (_showingInput)
                            {
                                writer.WriteLine(" INPUT: " + input);
                                writer.WriteLine("TOKENS: " + string.Join(", ", tokens));
                                writer.WriteLine(" SPLIT: ");
                                foreach(Expression expr in exprs)
                                {
                                    writer.WriteLine($"{i,6}: " + expr.ToString());
                                }
                                writer.WriteLine("----------");
                            }

                            System.Diagnostics.Stopwatch? timer = showTimer
                                ? System.Diagnostics.Stopwatch.StartNew()
                                : null;

                            foreach (Expression expr in exprs)
                            {
                                try
                                {
                                    Expression result = Evaluator.Evaluate(expr, scope, _showingSteps ? writer : null, _pausing);
                                    output = result.Serialize();
                                }
                                catch (Exception ex)
                                {
                                    errors.WriteLine("ERROR: " + ex.Message);
                                    errors.WriteLine($"\tin expression {i}: " + expr.Serialize());
                                    errors.WriteLine(ex.StackTrace);
                                }
                            }

                            timer?.Stop();

                            if (timer is not null)
                            {
                                Console.WriteLine("(In {0:N3} seconds)", timer.Elapsed.TotalSeconds);
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

        private static string ToggledMsg(bool state) => "Toggled " + (state ? "ON" : "OFF");
    }
}

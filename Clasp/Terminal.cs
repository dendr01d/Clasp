using Clasp.ExtensionMethods;
using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Process;
using Clasp.Data.Terms;
using Clasp.Data.Text;
using System.IO;
using System;
using System.Collections.Generic;

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

        private const string _SHOW_HELP_CMD = "\\help";
        private const string _SHOW_INPUT_STEPS_CMD = "\\mirror";
        private const string _SHOW_MACHINE_CMD = "\\show";
        private const string _PAUSE_MACHINE_CMD = "\\pause";
        private const string _CLEAR_SCREEN_CMD = "\\clear";
        private const string _RELOAD_ENV_CMD = "\\reload";
        private const string _TIMER_CMD = "\\timer";
        private const string _QUIT_CMD = "\\quit";

        private static bool _showingInput = true;
        private static bool _showingSteps = false;
        private static bool _pausing = false;

        private static void Run(Stream inputStream, Stream outputStream, Stream errorStream)
        {
            StreamReader reader = new(inputStream, CharFormat);
            StreamWriter writer = new(outputStream, CharFormat);
            StreamWriter errors = new(errorStream, CharFormat);

            writer.AutoFlush = true;
            errors.AutoFlush = true;

            bool showHeader = true;
            bool showHelp = true;
            bool reloadEnv = false;

            bool showTimer = false;

            string input = string.Empty;
            string output = string.Empty;

            Binding.Environment env = StandardEnv.CreateNew();
            Binding.BindingStore store = new BindingStore();

            while (input != _QUIT_CMD)
            {
                try
                {
                    if (showHeader)
                    {
                        writer.WriteLine(_header);
                        writer.WriteLine();
                        showHeader = false;
                    }

                    if (showHelp)
                    {
                        writer.WriteLine($" ∙ \"{_SHOW_INPUT_STEPS_CMD}\" to toggle input-mirroring");
                        writer.WriteLine($" ∙ \"{_SHOW_MACHINE_CMD}\" to toggle machine printing");
                        writer.WriteLine($" ∙ \"{_PAUSE_MACHINE_CMD}\" to toggle step-by-step pausing");
                        writer.WriteLine($" ∙ \"{_CLEAR_SCREEN_CMD}\" to clear the screen");
                        writer.WriteLine($" ∙ \"{_RELOAD_ENV_CMD}\" to reload the std env from file");
                        writer.WriteLine($" ∙ \"{_TIMER_CMD}\" to display the computation timer");
                        writer.WriteLine($" ∙ \"{_QUIT_CMD}\" to exit");
                        showHelp = false;
                    }

                    if (reloadEnv)
                    {
                        env = StandardEnv.CreateNew();
                        store = new BindingStore();
                        reloadEnv = false;
                    }

                    writer.WriteLine();
                    writer.Write("> ");
                    input = reader.ReadLine() ?? string.Empty;
                    output = string.Empty;

                    switch (input.ToLower())
                    {
                        case _SHOW_HELP_CMD:
                            showHelp = true;
                            break;

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
                            showHeader = true;
                            break;

                        case _RELOAD_ENV_CMD:
                            reloadEnv = true;
                            break;

                        default:
                            if (!string.IsNullOrWhiteSpace(input))
                            {
                                if (_showingInput) writer.WriteLine(" INPUT: {0}", input);

                                IEnumerable<Token> tokens = Lexer.Lex("REPL", input);
                                if (_showingInput) writer.WriteLine("TOKENS: {0}", Printer.PrintRawTokens(tokens));

                                Syntax readSyntax = Reader.Read(tokens);
                                if (_showingInput) writer.WriteLine("  READ: {0}", readSyntax.ToString());

                                Syntax expandedSyntax = Expander.Expand(readSyntax, env, store);
                                if (_showingInput) writer.WriteLine("EXPAND: {0}", expandedSyntax.ToString());

                                AstNode parsedInput = Parser.ParseAST(expandedSyntax, store, 0);
                                if (_showingInput) writer.WriteLine(" PARSE: {0}", parsedInput.ToString());

                                if (_showingInput) writer.WriteLine("-------");

                                Term result = Machine.Interpret(parsedInput, env);
                                output = result.ToString();

                                //System.Diagnostics.Stopwatch? timer = showTimer
                                //    ? System.Diagnostics.Stopwatch.StartNew()
                                //    : null;

                                //foreach (Expression expr in exprs)
                                //{
                                //    try
                                //    {
                                //        Expression result = Evaluator.Evaluate(expr, scope, _showingSteps, _pausing);
                                //        output = result.Write();
                                //    }
                                //    catch (Exception ex)
                                //    {
                                //        errors.WriteLine("ERROR: " + ex.Message);
                                //        errors.WriteLine($"\tin expression {i}: " + expr.Write());
                                //        errors.WriteLine(ex.StackTrace);
                                //    }
                                //}

                                //timer?.Stop();

                                //if (timer is not null)
                                //{
                                //    Console.WriteLine("(In {0:N3} seconds)", timer.Elapsed.TotalSeconds);
                                //}
                            }
                            break;
                    }

                    if (!string.IsNullOrEmpty(output))
                    {
                        writer.WriteLine(output);
                    }
                }
                catch (AggregateException aggEx)
                {
                    foreach (Exception ex in aggEx.InnerExceptions)
                    {
                        PrintExceptionInfo(ex);
                    }
                    //ExceptionContinue();
                }
                catch (Exception ex)
                {
                    PrintExceptionInfo(ex);
                    //ExceptionContinue();
                }
            }

            return;
        }

        private static string ToggledMsg(bool state) => "Toggled " + (state ? "ON" : "OFF");

        private static void PrintExceptionInfo(Exception ex)
        {
            Console.Write(ex switch
            {
                LexerException => "Lexing error: ",
                ReaderException => "Reading error: ",
                ExpanderException => "Expansion error: ",
                ParserException => "Parsing error: ",
                _ => "Unknown error: "
            });

            if (ex is ISourceTraceable ist)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(Printer.PrintLineErrorHelper(ist));
            }
            else
            {
                Console.WriteLine("{0}{1}{2}", ex.Message, System.Environment.NewLine, ex.GetSimpleStackTrace());

                if (ex.InnerException is not null)
                {
                    Console.WriteLine("└─>");
                    PrintExceptionInfo(ex.InnerException);
                }
            }
        }

        private static void ExceptionContinue()
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}

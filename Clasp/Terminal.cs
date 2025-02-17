using Clasp.ExtensionMethods;
using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Process;
using Clasp.Data.Terms;
using Clasp.Data.Text;
using System.IO;
using System;
using System.Collections.Generic;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.VirtualMachine;
using Clasp.Interfaces;
using Clasp.Exceptions;

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

            void PrintStep(int i, MachineState machine)
            {
                writer.WriteLine("------------------------------------------------------------");
                writer.WriteLine("Step {0}:", i);
                writer.WriteLine();
                Interpreter.PrintMachineState(machine, writer);
                writer.WriteLine();
            }

            void PrintStepAndPause(int i, MachineState machine)
            {
                PrintStep(i, machine);
                Console.ReadKey(true);
            }

            bool showHeader = true;
            bool showHelp = true;
            //bool reloadEnv = false;

            bool showTimer = false;

            string input = string.Empty;
            string output = string.Empty;

            // -----

            Processor clasp = new Processor(writer);
            Blob session = new Blob("REPL", []);

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

                    //if (reloadEnv)
                    //{
                    //    clasp.ReloadEnv();
                    //    reloadEnv = false;
                    //}

                    writer.WriteLine();
                    PrintSeparator();
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

                        //case _RELOAD_ENV_CMD:
                        //    reloadEnv = true;
                        //    break;

                        default:
                            if (!string.IsNullOrWhiteSpace(input))
                            {
                                if (_showingInput) writer.WriteLine(" INPUT: {0}", input);

                                IEnumerable<Token> tokens = clasp.LexLine(input, session);
                                if (_showingInput) writer.WriteLine("TOKENS: {0}", Printer.PrintRawTokens(tokens));

                                Syntax readSyntax = clasp.Read(tokens);
                                if (_showingInput) writer.WriteLine("  READ: {0}", readSyntax.ToString());
                                if (_showingInput) writer.WriteLine("    └─> {0}", readSyntax.ToDatum());

                                Syntax expandedSyntax = clasp.Expand(readSyntax);
                                if (_showingInput) writer.WriteLine("EXPAND: {0}", expandedSyntax.ToString());
                                if (_showingInput) writer.WriteLine("    └─> {0}", expandedSyntax.ToDatum());

                                CoreForm parsedInput = clasp.Parse(expandedSyntax);
                                if (_showingInput) writer.WriteLine(" PARSE: {0}", parsedInput.ToString());

                                if (_showingInput) writer.WriteLine("-------");

                                System.Diagnostics.Stopwatch? timer = showTimer
                                    ? System.Diagnostics.Stopwatch.StartNew()
                                    : null;

                                Term result;

                                if (_showingSteps)
                                {
                                    if (_pausing)
                                    {
                                        result = clasp.Interpret(parsedInput, PrintStepAndPause);
                                    }
                                    else
                                    {
                                        result = clasp.Interpret(parsedInput, PrintStep);
                                    }
                                }
                                else
                                {
                                    result = clasp.Interpret(parsedInput);
                                }

                                timer?.Stop();
                                output = result.ToString();

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
                catch (AggregateException aggEx)
                {
                    foreach (Exception ex in aggEx.InnerExceptions)
                    {
                        writer.WriteLine();
                        PrintExceptionInfo(writer, ex);
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteLine();
                    PrintExceptionInfo(writer, ex);
                }
            }

            return;
        }

        private static string ToggledMsg(bool state) => "Toggled " + (state ? "ON" : "OFF");

        private static void PrintExceptionInfo(StreamWriter sw, Exception ex)
        {

        }

        private static void PrintSeparator()
        {
            Console.WriteLine(new string('-', Console.WindowWidth - 1));
        }
    }
}

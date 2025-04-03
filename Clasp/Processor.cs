using System;
using System.Collections.Generic;

using Clasp.AbstractMachine;
using Clasp.Binding.Modules;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Process;

namespace Clasp
{
    public static class Processor
    {
        internal static IEnumerable<string> Pipe(string source) => Piper.PipeInFileContents(source);
        internal static IEnumerable<Token> Lex(string source, string input) => Lexer.LexText(source, input);

        internal static Syntax Read(IEnumerable<Token> tokens) => Reader.ReadTokenSyntax(tokens);

        internal static Syntax ExpandTopLevel(Syntax rawSyntax) => Expander.Expand(rawSyntax, CompilationContext.ForTopLevel());

        internal static CoreForm Parse(Syntax expandedSyntax) => Parser.ParseSyntax(expandedSyntax, 1);

        internal static Term Interpret(CoreForm prog) => Interpreter.InterpretInVacuum(prog);

        // ------

        //internal static CoreForm ProcessModularProgram(string inputFilePath)
        //{

        //    IEnumerable<string> texts = Pipe(inputFilePath);
        //    IEnumerable<Token> tokens = Lex(inputFilePath, string.Join(Environment.NewLine, texts));
        //    Syntax readSyntax = Reader.ReadModuleForm(tokens);
        //    Syntax expandedSyntax = Expander.Expand(readSyntax, CompilationContext.ForTopLevel());
        //    CoreForm parsedInput = Parser.ParseSyntax(expandedSyntax, 1);

        //    MachineState mx = new MachineState(parsedInput, new RootEnv());
        //    Term result = Interpreter.InterpretInVacuum(parsedInput);

        //    return parsedInput;
        //}

        /// <summary>
        /// Read and interpret a CLASP program from the contents of <paramref name="inputFilePath"/>.
        /// </summary>
        /// <param name="inputFilePath">The path to a file containing the source code of a CLASP program.</param>
        /// <param name="outputStream">Circumstantial program output will be directed here.</param>
        /// <returns>The result of the CLASP program, coerced to a string.</returns>
        public static string ProcessProgram(string inputFilePath)
        {
            string moduleName = Module.NameFromPath(inputFilePath);

            if (Module.Visit(moduleName) is VisitedModule vMdl)
            {
                Console.WriteLine(Printer.FormPrinter.Print(vMdl.ParsedForm.ToTerm(), false));
                Console.WriteLine();
            }

            return Module.Instantiate(moduleName).ToPrintedString();
        }



        private static void PrintStepAndPause(int i, MachineState machine)
        {
            PrintStep(i, machine);
            Console.ReadKey(true);
        }

        private static void PrintStep(int i, MachineState machine)
        {
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Step {0}:", i);
            Console.WriteLine();
            Console.WriteLine(Interpreter.PrintMachineState(machine));
            Console.WriteLine();
        }
    }
}

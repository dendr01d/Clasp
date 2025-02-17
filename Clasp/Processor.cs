using System.Collections.Generic;
using System.IO;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Data.VirtualMachine;
using Clasp.Process;

namespace Clasp
{
    public class Processor
    {
        internal StreamWriter OutputStream { get; private set; }
        internal SuperEnvironment RuntimeEnv { get; private set; }
        internal SuperEnvironment CompileTimeEnv { get; private set; }

        public Processor(StreamWriter outputStream)
        {
            OutputStream = outputStream;
            RuntimeEnv = StandardEnv.CreateNew(this);
            CompileTimeEnv = StandardEnv.CreateNew(this);
        }

        public Processor CreateSubProcess() => new Processor(OutputStream);

        /// <summary>
        /// Read the contents of the file at the given path line-by-line.
        /// </summary>
        internal IEnumerable<string> Pipe(string source) => Piper.PipeInFileContents(source);

        /// <summary>
        /// Lex <paramref name="input"/> into a stream of tokens, while earmarking it as coming from <paramref name="source"/>.
        /// </summary>
        internal IEnumerable<Token> Lex(string source, string input) => Lexer.LexText(source, input);

        internal IEnumerable<Token> LexLine(string input, Blob session)
        {
            int currentCharCount = session.CharacterCount;
            int currentLineCount = session.Lines.Length + 1;
            session.AddLine(input);

            return Lexer.LexSingleLine(session, input, currentLineCount, currentCharCount);
        }

        /// <summary>
        /// Read a series of tokens into structured <see cref="Term"/> values, preserving lexical qualities
        /// by wrapping them into <see cref="Syntax"/> objects.
        /// </summary>
        internal Syntax Read(IEnumerable<Token> tokens) => Reader.ReadTokens(tokens);

        /// <summary>
        /// Hygienically expand macros in <paramref name="rawSyntax"/>.
        /// </summary>
        internal Syntax Expand(Syntax rawSyntax, int phase = 1)
            => Expander.ExpandSyntax(rawSyntax, new ExpansionContext(CompileTimeEnv, phase));

        /// <summary>
        /// Parse <paramref name="expandedSyntax"/> into a structured <see cref="CoreForm"/> object using the lexical
        /// information saved in <paramref name="context"/>.
        /// </summary>
        internal CoreForm Parse(Syntax expandedSyntax, int phase = 1)
            => Parser.ParseSyntax(expandedSyntax, new ParseContext(CompileTimeEnv, phase));

        /// <summary>
        /// Invoke a virtual machine to execute the program encoded by <paramref name="prog"/> and return the result.
        /// </summary>
        internal Term Interpret(CoreForm prog) => Interpreter.InterpretProgram(prog, RuntimeEnv);
        /// <summary>
        /// <inheritdoc cref="Interpret(CoreForm)"/>
        /// <paramref name="postStepHook"/> will be called following every machine step.
        /// </summary>
        internal Term Interpret(CoreForm prog, System.Action<int, MachineState> postStepHook)
            => Interpreter.Interpret(prog, RuntimeEnv, postStepHook);

        // ------

        internal CoreForm ProcessProgram(string inputFilePath)
        {
            Syntax inputSyntax = Reader.ReadTokens(Lexer.LexLines(inputFilePath, Piper.PipeInFileContents(inputFilePath)));

            ExpansionContext context = new ExpansionContext(CompileTimeEnv, 1);
            Syntax expandedSyntax = Expander.ExpandSyntax(inputSyntax, context);
            CoreForm program = Parser.ParseSyntax(expandedSyntax, context);

            return program;
        }

        /// <summary>
        /// Read and interpret a CLASP program from the contents of <paramref name="inputFilePath"/>.
        /// </summary>
        /// <param name="inputFilePath">The path to a file containing the source code of a CLASP program.</param>
        /// <param name="outputStream">Circumstantial program output will be directed here.</param>
        /// <returns>The result of the CLASP program, coerced to a string.</returns>
        public string Process(string inputFilePath)
        {
            CoreForm program = ProcessProgram(inputFilePath);
            Term output = Interpreter.InterpretProgram(program, RuntimeEnv);
            return output.ToString();
        }
    }
}

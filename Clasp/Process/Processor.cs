using System.Collections.Generic;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Data.VirtualMachine;

namespace Clasp.Process
{
    internal class Processor
    {
        public SuperEnvironment RuntimeEnv { get; private set; }
        public SuperEnvironment CompileTimeEnv { get; private set; }

        public Processor()
        {
            RuntimeEnv = StandardEnv.CreateNew(this);
            CompileTimeEnv = StandardEnv.CreateNew(this);
        }

        /// <summary>
        /// Lex <paramref name="input"/> into a stream of tokens, while earmarking it as coming from <paramref name="source"/>.
        /// </summary>
        public IEnumerable<Token> Lex(string source, string input) => Lexer.LexText(source, input);

        public IEnumerable<Token> LexLine(string input, Blob session)
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
        public Syntax Read(IEnumerable<Token> tokens) => Reader.ReadTokens(tokens);

        /// <summary>
        /// Hygienically expand macros in <paramref name="rawSyntax"/>.
        /// </summary>
        public Syntax Expand(Syntax rawSyntax, int phase = 1)
            => Expander.ExpandSyntax(rawSyntax, new ExpansionContext(CompileTimeEnv, phase));

        /// <summary>
        /// Parse <paramref name="expandedSyntax"/> into a structured <see cref="CoreForm"/> object using the lexical
        /// information saved in <paramref name="context"/>.
        /// </summary>
        public CoreForm Parse(Syntax expandedSyntax, int phase = 1)
            => Parser.ParseSyntax(expandedSyntax, new ParseContext(CompileTimeEnv, phase));

        /// <summary>
        /// Invoke a virtual machine to execute the program encoded by <paramref name="prog"/> and return the result.
        /// </summary>
        public Term Interpret(CoreForm prog) => Interpreter.InterpretProgram(prog, RuntimeEnv);
        /// <summary>
        /// <inheritdoc cref="Interpret(CoreForm)"/>
        /// <paramref name="postStepHook"/> will be called following every machine step.
        /// </summary>
        public Term Interpret(CoreForm prog, System.Action<int, MachineState> postStepHook)
            => Interpreter.Interpret(prog, RuntimeEnv, postStepHook);


    }
}

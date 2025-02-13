using System.Collections.Generic;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Syntax;
using Clasp.Data.Text;

namespace Clasp.Process
{
    internal class Processor
    {
        public SuperEnvironment TopLevelEnv { get; private set; }

        public Processor()
        {
            TopLevelEnv = null!;
            ReloadEnv();
        }

        public void ReloadEnv()
        {
            TopLevelEnv = StandardEnv.CreateNew(this);
        }

        /// <summary>
        /// Lex <paramref name="input"/> into a stream of tokens, while earmarking it as coming from <paramref name="source"/>.
        /// </summary>
        public IEnumerable<Token> Lex(string source, string input) => Lexer.LexText(source, input);

        /// <summary>
        /// Read a series of tokens into structured <see cref="Term"/> values, preserving lexical qualities
        /// by wrapping them into <see cref="Syntax"/> objects.
        /// </summary>
        public Syntax Read(IEnumerable<Token> tokens) => Reader.ReadTokens(tokens);

        /// <summary>
        /// Hygienically expand macros in <paramref name="rawSyntax"/>, recording variable renamings in <paramref name="context"/>.
        /// </summary>
        /// <remarks>
        /// Parsing the resulting expanded syntax will require <paramref name="context"/>.
        /// </remarks>
        public Syntax Expand(Syntax rawSyntax, ExpansionContext context) => Expander.ExpandSyntax(rawSyntax, context);
        /// <summary>
        /// Expand macros in <paramref name="rawSyntax"/>.
        /// </summary>
        public Syntax Expand(Syntax rawSyntax) => Expander.ExpandSyntax(rawSyntax, ExpansionContext.FreshExpansion(TopLevelEnv));

        /// <summary>
        /// Parse <paramref name="expandedSyntax"/> into a structured <see cref="CoreForm"/> object using the lexical
        /// information saved in <paramref name="context"/>.
        /// </summary>
        public CoreForm Parse(Syntax expandedSyntax, ExpansionContext context) => Parser.ParseSyntax(expandedSyntax, context);
        /// <summary>
        /// First <see cref="Expand(Syntax, ExpansionContext)"/> the provided <paramref name="rawSyntax"/>, then
        /// <see cref="Parse(Syntax, ExpansionContext)"/> it into a <see cref="CoreForm"/>.
        /// </summary>
        public CoreForm Parse(Syntax rawSyntax)
        {
            ExpansionContext context = ExpansionContext.FreshExpansion(TopLevelEnv);
            Syntax expandedStx = Expand(rawSyntax, context);
            return Parse(expandedStx, context);
        }

        /// <summary>
        /// Invoke a virtual machine to execute the program encoded by <paramref name="prog"/> and return the result.
        /// </summary>
        public Term Interpret(CoreForm prog) => Interpreter.InterpretProgram(prog, TopLevelEnv);
        /// <summary>
        /// <inheritdoc cref="Interpret(CoreForm)"/>
        /// <paramref name="postStepHook"/> will be called following every machine step.
        /// </summary>
        public Term Interpret(CoreForm prog, System.Action<int, MachineState> postStepHook)
            => Interpreter.Interpret(prog, TopLevelEnv, postStepHook);


        #region Statics

        public static CoreForm ParseText(string source, string input, Environment env)
        {
            Syntax rawStx = Reader.ReadTokens(Lexer.LexText(source, input));

            ExpansionContext context = ExpansionContext.FreshExpansion(env);
            Syntax expandedStx = Expander.ExpandSyntax(rawStx, context);
            return Parser.ParseSyntax(expandedStx, context);
        }

        #endregion


    }
}

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public BindingStore LexicalBindings { get; private set; }

        private readonly ScopeTokenGenerator _tokenGen;

        public Processor()
        {
            TopLevelEnv = null!;
            ReloadEnv();

            LexicalBindings = new BindingStore(TopLevelEnv);

            _tokenGen = new ScopeTokenGenerator();
        }

        public Processor(SuperEnvironment env, BindingStore bs)
        {
            TopLevelEnv = env;
            LexicalBindings = bs;
        }

        public void ReloadEnv()
        {
            TopLevelEnv = StandardEnv.CreateNew();
        }

        public IEnumerable<Token> Lex(string source, string input) => Lexer.LexText(source, input);
        public Syntax Read(IEnumerable<Token> tokens) => Reader.ReadTokens(tokens);
        public Syntax Expand(Syntax stx) => Expander.ExpandSyntax(stx, ExpansionContext.FreshExpansion(TopLevelEnv, LexicalBindings, _tokenGen));
        public CoreForm Parse(Syntax stx) => Parser.ParseSyntax(stx, ExpansionContext.FreshExpansion(TopLevelEnv, LexicalBindings, _tokenGen));

        public Term Interpret(CoreForm prog) => Interpreter.InterpretProgram(prog, this);
        public Term Interpret(CoreForm prog, System.Action<int, MachineState> postStepHook)
            => Interpreter.Interpret(prog, this, postStepHook);

        
        public static Term Process(string source, string input, Processor pross)
        {
            CoreForm program = pross.Parse(pross.Expand(pross.Read(pross.Lex(source, input))));
            return pross.Interpret(program);
        }

    }
}

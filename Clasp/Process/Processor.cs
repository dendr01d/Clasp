using System.Collections.Generic;
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
        public BindingStore Bindings { get; private set; }

        private readonly ScopeTokenGenerator _tokenGen;

        public Processor()
        {
            TopLevelEnv = null!;
            ReloadEnv();

            Bindings = new BindingStore();

            _tokenGen = new ScopeTokenGenerator();
        }

        public void ReloadEnv()
        {
            TopLevelEnv = StandardEnv.CreateNew();
        }

        public IEnumerable<Token> Lex(string source, string input) => Lexer.LexText(source, input);
        public Syntax Read(IEnumerable<Token> tokens) => Reader.ReadTokens(tokens);
        public Syntax Expand(Syntax stx) => Expander.ExpandSyntax(stx, TopLevelEnv, Bindings, _tokenGen);
        public CoreForm Parse(Syntax stx) => Parser.ParseSyntax(stx, Bindings, 0);

        public Term Interpret(CoreForm prog) => Interpreter.InterpretProgram(prog, TopLevelEnv);
        public Term Interpret(CoreForm prog, System.Action<int, MachineState> postStepHook)
            => Interpreter.Interpret(prog, TopLevelEnv, postStepHook);
    }
}

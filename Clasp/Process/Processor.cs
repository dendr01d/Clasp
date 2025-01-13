using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Text;

namespace Clasp.Process
{
    internal class Processor
    {
        public GlobalEnvironment TopLevelEnv { get; private set; }
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

        public IEnumerable<Token> Lex(string source, string input) => Lexer.Lex(source, input);
        public Syntax Read(IEnumerable<Token> tokens) => Reader.Read(tokens);
        public Syntax Expand(Syntax stx) => Expander.Expand(stx, TopLevelEnv, Bindings, _tokenGen);
        public AstNode Parse(Syntax stx) => Parser.ParseAST(stx, Bindings, 0);

        public Term Interpret(AstNode prog) => Interpreter.Interpret(prog, TopLevelEnv);
        public Term Interpret(AstNode prog, System.Action<int, MachineState> postStepHook)
            => Interpreter.Interpret(prog, TopLevelEnv, postStepHook);
    }
}

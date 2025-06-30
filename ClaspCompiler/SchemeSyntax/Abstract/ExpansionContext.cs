using ClaspCompiler.CompilerData;
using ClaspCompiler.LexicalScope;

namespace ClaspCompiler.SchemeSyntax.Abstract
{
    internal enum ContextType { Standard, TopLevel, Partial, Completional };

    internal sealed record ExpansionContext(
        SymbolFactory SymFactory,
        BindingStore BindingStore,
        Dictionary<object, object> CompilationEnv, // env used for expansion
        Dictionary<object, object> ExecutionEnv, // env used for runtime execution (null at phase zero)
        ContextType Lexicon,
        int Phase)
    {
        private ExpansionContext? _nextPhase = null;

        public static ExpansionContext NewTopLevelContext(SymbolFactory symGen)
        {
            return new(symGen, new(), [], [], ContextType.TopLevel, 0);
        }

        public ExpansionContext InSubLevel() => this with { Lexicon = Lexicon == ContextType.TopLevel ? ContextType.Standard : Lexicon };
        public ExpansionContext InStandardContext() => this with { Lexicon = ContextType.Standard };
        public ExpansionContext InPartialContext() => this with { Lexicon = ContextType.Partial };
        public ExpansionContext InCompletionalContext() => this with { Lexicon = ContextType.Completional };

        public ExpansionContext InNextPhase()
        {
            _nextPhase ??= this with
            {
                CompilationEnv = [],
                ExecutionEnv = CompilationEnv,
                Lexicon = (Lexicon != ContextType.TopLevel) ? ContextType.Standard : Lexicon,
                Phase = Phase + 1
            };
            return _nextPhase;
        }

        public sealed override string ToString() => $"{Lexicon} Context in phase {Phase}";
    }
}

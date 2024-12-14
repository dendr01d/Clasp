using Clasp.Binding;
using Clasp.Data.Terms;

namespace Clasp.Data.Metadata
{
    internal sealed class ExpansionHarness
    {
        public readonly CompileTimeEnvironment Chi;
        public readonly ScopeSet LexicalScope;
        public readonly BindingStore Sigma;
        public readonly int PhaseLevel;

        private readonly ScopeTokenGenerator _tokenGen;

        public ExpansionHarness(Environment enclosingEnv)
        {
            Chi = new CompileTimeEnvironment(enclosingEnv);
            LexicalScope = new ScopeSet();

            Sigma = new BindingStore();
            PhaseLevel = 0;

            _tokenGen = new ScopeTokenGenerator();
        }

        private ExpansionHarness(
            CompileTimeEnvironment enclosingEnv,
            ScopeSet currentScope,
            BindingStore bStore,
            int phaseLevel,
            ScopeTokenGenerator tokenGen)
        {
            Chi = enclosingEnv;
            LexicalScope = currentScope;
            Sigma = bStore;
            PhaseLevel = phaseLevel;
            _tokenGen = tokenGen;
        }

        // ---

        public ExpansionHarness ExtendScope()
        {
            return new ExpansionHarness(
                this.Chi,
                this.LexicalScope.Extend(_tokenGen.FreshToken()),
                this.Sigma,
                this.PhaseLevel,
                this._tokenGen);
        }

        public ExpansionHarness EnterSubExpansion()
        {
            return new ExpansionHarness(
                this.Chi,
                new ScopeSet(),
                this.Sigma, //do the bindings get inherited?
                this.PhaseLevel + 1,
                this._tokenGen);
        }

        // ---

        public string ResolveBindingName(Identifier id)
        {
            return Sigma.ResolveBindingName(id.WrappedValue.Name, id.Context[PhaseLevel]);
        }

        public Term? ResolveBoundValue(Identifier id)
        {
            return Chi.TryGetValue(ResolveBindingName(id), out Term t)
                ? t
                : null;
        }
    }
}

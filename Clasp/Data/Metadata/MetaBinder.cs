using Clasp.Binding;
using Clasp.Data.Terms;

namespace Clasp.Data.Metadata
{
    internal sealed class MetaBinder
    {
        public readonly CompileTimeEnvironment Chi;
        //public readonly ScopeSet LexicalScope;
        public readonly Syntax ScopeProxy; //instead of keeping track of a scopeset directly, we can use the one contained in this syntax
        public readonly BindingStore Sigma;
        public readonly int PhaseLevel;

        private readonly ScopeTokenGenerator _tokenGen;

        public ScopeSet CurrentScope => ScopeProxy.Context[PhaseLevel];

        public MetaBinder(Syntax proxy, EnvFrame enclosingEnv)
        {
            Chi = new CompileTimeEnvironment(enclosingEnv);
            //LexicalScope = new ScopeSet();
            ScopeProxy = proxy;

            Sigma = new BindingStore();
            PhaseLevel = 0;

            _tokenGen = new ScopeTokenGenerator();
        }

        private MetaBinder(
            CompileTimeEnvironment enclosingEnv,
            //ScopeSet currentScope,
            Syntax proxy,
            BindingStore bStore,
            int phaseLevel,
            ScopeTokenGenerator tokenGen)
        {
            Chi = enclosingEnv;
            //LexicalScope = currentScope;
            ScopeProxy = proxy;
            Sigma = bStore;
            PhaseLevel = phaseLevel;
            _tokenGen = tokenGen;
        }

        // ---

        public MetaBinder ExtendScope(Syntax newProxy)
        {
            uint token = _tokenGen.FreshToken();
            newProxy.Paint(PhaseLevel, token);

            return new MetaBinder(
                this.Chi,
                //this.LexicalScope.Extend(token),
                newProxy,
                this.Sigma,
                this.PhaseLevel,
                this._tokenGen);
        }

        public MetaBinder EnterSubExpansion(Syntax proxy)
        {
            return new MetaBinder(
                this.Chi,
                //new ScopeSet(),
                proxy,
                this.Sigma, //do the bindings get inherited?
                this.PhaseLevel + 1,
                this._tokenGen);
        }

        // ---

        public Identifier CreateFreshBinding(Identifier id)
        {
            string freshName = Chi.CreateFreshName(id.Name);
            Symbol freshSym = Symbol.Intern(freshName);
            Identifier output = new Identifier(freshSym, id);

            Sigma.BindName(id.Name, CurrentScope, freshName);
            Chi.Add(freshName, output);

            return output;
        }

        public Identifier CreateFreshTransformerBinding(Identifier id, MacroProcedure tx)
        {
            Identifier newKey = CreateFreshBinding(id);
            Chi.Add(newKey.WrappedValue.Name, tx);

            return newKey;
        }

        public string ResolveBindingName(Identifier id)
        {
            return Sigma.ResolveBindingName(id.WrappedValue.Name, id.Context[PhaseLevel]);
        }

        public Term? ResolveBoundValue(Identifier id)
        {
            return Chi.TryGetValue(ResolveBindingName(id), out Term? t)
                ? t
                : null;
        }
    }
}

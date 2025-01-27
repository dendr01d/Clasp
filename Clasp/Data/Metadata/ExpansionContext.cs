using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Binding.Scopes;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Syntax;

namespace Clasp.Data.Metadata
{
    internal sealed class ExpansionContext
    {
        #region Object Members

        public Scope CurrentScope { get; private set; }
        public Environment CurrentEnv { get; private set; }

        public int Phase { get; private set; }

        /// <summary>IDs of scopes that were introduced for the purpose of binding identifiers</summary>
        private readonly HashSet<uint> _localDefinitionScopes;

        /// <summary>IDs of scopes that were introduced during macro use/introduction.</summary>
        private readonly HashSet<uint> _localMacroScopes;

        public SyntacticContext CurrentContext { get; private set; }

        /// <summary>True iff the current expansion should stop at core forms.</summary>
        //public bool RestrictedToImmediate { get; private set; }

        private readonly ScopeFactory _factory;

        #endregion
        private ExpansionContext(
            Scope scp,
            Environment env,
            int phase,
            ScopeFactory factory,
            IEnumerable<uint> bindingScopes,
            IEnumerable<uint> macroScopes,
            SyntacticContext currentCtx
            //bool restrictToImmediate
            )
        {
            CurrentScope = scp;
            CurrentEnv = env;
            Phase = phase;

            _localDefinitionScopes = new(bindingScopes);
            _localMacroScopes = new(macroScopes);

            //CurrentContext = currentCtx;
            //RestrictedToImmediate = restrictToImmediate;

            _factory = factory;
        }

        private ExpansionContext(ExpansionContext original)
            : this(original.CurrentScope, original.CurrentEnv, original.Phase, original._factory, original._localDefinitionScopes, original._localMacroScopes, original.CurrentContext)
        { }

        public static ExpansionContext MakeFresh(ScopeTokenGenerator gen)
        {
            ScopeFactory factory = new ScopeFactory(gen);

            return new ExpansionContext(factory.NewScope(), StandardEnv.CreateNew(), 1, factory, [], [], SyntacticContext.TopLevel);
        }

        public ExpansionContext CrossToNextPhase()
        {
            return new ExpansionContext(_factory.NewScope(), StandardEnv.CreateNew(), Phase + 1, _factory, [], [], SyntacticContext.TopLevel);
        }

        public ExpansionContext EnterNewLocalDefinitionScope()
        {
            return new ExpansionContext(
                _factory.NewScopeInside(CurrentScope), CurrentEnv.Enclose(), Phase, _factory,)
        }

        public Scope NewLocalDefScope(Scope definitionContext)
        {
            Scope output = _factory.NewScopeInside(definitionContext);
            _localDefinitionScopes.Add(output.Id);
            return output;
        }

        public Scope NewLocalMacroScope(Scope definitionContext)
        {
            Scope output = _factory.NewScopeInside(definitionContext);
            _localMacroScopes.Add(output.Id);
            return output;
        }

        #region Context Mutation

        public void PaintScopeInCurrentPhase(Syntax stx, params uint[] scopes) => ScopeAdjuster.Paint(stx, Phase, scopes);
        public void FlipScopeInCurrentPhase(Syntax stx, params uint[] scopes) => ScopeAdjuster.Flip(stx, Phase, scopes);
        public void RemoveScopeInCurrentPhase(Syntax stx, params uint[] scopes) => ScopeAdjuster.Remove(stx, Phase, scopes);

        //public void PaintScopeInAllPhases(Syntax stx, params uint[] scopes) => ScopeAdjuster.PaintAll(stx, scopes);
        //public void FlipScopeInAllPhases(Syntax stx, params uint[] scopes) => ScopeAdjuster.FlipInAll(stx, scopes);
        //public void RemoveScopeInAllPhases(Syntax stx, params uint[] scopes) => ScopeAdjuster.RemoveFromAll(stx, scopes);

        public Syntax StripMacroScopes(Identifier bindingId)
        {
            Syntax output = Syntax.FromSyntax(bindingId);
            RemoveScopeInCurrentPhase(output, _localMacroScopes.ToArray());
            return output;
        }

        public Syntax StripQuotedSyntax(Syntax quotedSyntax)
        {
            return quotedSyntax.StripFromPhase(Phase);
        }

        #endregion

        #region BindingResolution

        public ExpansionBinding ResolveBinding(Identifier id, Scope scp)
        {
            ExpansionBinding[] matches = scp.ResolveBindings(id.SymbolicName, id.GetScopeSet(Phase)).ToArray();

            if (matches.Length == 0)
            {
                throw new ExpanderException.UnboundIdentifier(id);
            }
            else if (matches.Length > 1)
            {
                throw new ExpanderException.AmbiguousIdentifier(id, matches);
            }
            else
            {
                return matches.Single();
            }
        }


        #endregion

        #region Env Helpers

        public void BindMacro(Identifier bindingId, MacroProcedure macro)
        {
            CurrentEnv[bindingId.SymbolicName] = macro;
        }

        public bool TryGetMacro(Identifier bindingId,
            [NotNullWhen(true)] out MacroProcedure? macro)
        {
            return CurrentEnv.TryGetValue(bindingId.SymbolicName, out macro);
        }

        #endregion
    }
}

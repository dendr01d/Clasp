using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding;
using Clasp.Data.Terms;

namespace Clasp.Data.Metadata
{
    internal sealed class ExpansionContext
    {
        #region Object Members

        public Environment Env { get; private set; }
        public BindingStore Store { get; private set; }
        public int Phase { get; private set; }

        /// <summary>
        /// Scopes that have been painted via this <see cref="ExpansionContext"/>,
        /// with the intent that they can be pruned back out of quote-syntax forms.
        /// </summary>
        public readonly HashSet<uint> NewScopes;

        /// <summary>
        /// Scopes that need to be pruned from binders (???)
        /// </summary>
        public readonly HashSet<uint> UseSiteScopes;

        public ExpandingType CurrentContext { get; private set; }

        /// <summary>True iff the current expansion should stop at core forms.</summary>
        public bool RestrictedToImmediate { get; private set; }


        public readonly ScopeTokenGenerator TokenGen;

        #endregion

        #region Constructors

        // Full Constructor
        private ExpansionContext(Environment env, BindingStore bs, int phase, ScopeTokenGenerator gen,
            ExpandingType currentCtx, IEnumerable<uint> newScopes, IEnumerable<uint> useSiteScopes,
            bool restrictToImmediate)
        {
            Env = env;
            Store = bs;
            Phase = phase;

            NewScopes = new(newScopes);
            UseSiteScopes = new(useSiteScopes);

            CurrentContext = currentCtx;
            RestrictedToImmediate = restrictToImmediate;

            TokenGen = gen;
        }

        // Public Restricted Constructor
        public ExpansionContext(Environment env, BindingStore bs, int phase, ScopeTokenGenerator gen, ExpandingType currentCtx)
            : this(env, bs, phase, gen, currentCtx, [], [], false)
        { }

        // Private Copy Constructor
        private ExpansionContext(ExpansionContext prev)
            : this(prev.Env, prev.Store, prev.Phase, prev.TokenGen, prev.CurrentContext, prev.NewScopes, prev.UseSiteScopes, prev.RestrictedToImmediate)
        { }

        #endregion

        #region Context Extension

        public ExpansionContext WithSubEnv()
        {
            return new ExpansionContext(this)
            {
                Env = new EnvFrame(this.Env)
            };
        }

        public ExpansionContext WithNextPhase()
        {
            return new ExpansionContext(this)
            {
                Phase = this.Phase + 1,
                Env = new EnvFrame(this.Env.TopLevel), // is this right?
                CurrentContext = ExpandingType.TopLevel // or this?
            };
        }

        #endregion

        #region Context Mutation

        public void PaintScope(Syntax stx, params uint[] scopes)
        {
            Syntax.PaintScope(stx, Phase, scopes);
            NewScopes.UnionWith(scopes);
        }

        #endregion


        public string ResolveBindingName(Syntax stx, string symbolicName) => Store.ResolveBindingName(stx, symbolicName, Phase);
        public void RenameInCurrentScope(Syntax stx, string symbolicName, string bindingName) => Store.RenameInScope(stx, symbolicName, Phase, bindingName);

        public bool TryResolveBindingName(Syntax stx, string symbolicName,
            [NotNullWhen(true)] out string? bindingName)
        {
            return Store.TryResolveBindingName(stx, symbolicName, Phase, out bindingName);
        }

        //public string? MaybeDereferenceBinding(string bindingName)
        //{
        //    if (Env.TryGetValue(bindingName, out Term? deref)
        //        && deref is Syntax<Symbol> derefId)
        //    {
        //        return derefId.Expose.Name;
        //    }
        //    return null;
        //}

        public void FlipScope(Syntax stx, params uint[] scopes) => Syntax.FlipScope(stx, Phase, scopes);

        #region Env Helpers

        private static readonly Symbol _variableMarker = new GenSym("variable");

        public bool IsVariable(string bindingName)
        {
            if (Env.TryGetValue(bindingName, out Term? value))
            {
                return value == _variableMarker;
            }
            return false;
        }

        public void MarkVariable(string bindingName)
        {
            Env[bindingName] = _variableMarker;
        }

        public void BindMacro(string bindingName, MacroProcedure macro)
        {
            Env[bindingName] = macro;
        }

        public bool TryGetMacro(string bindingName,
            [NotNullWhen(true)] out MacroProcedure? macro)
        {
            if (Env.TryGetValue(bindingName, out Term? value)
                && value is MacroProcedure result)
            {
                macro = result;
                return true;
            }

            macro = null;
            return false;
        }

        #endregion
    }
}

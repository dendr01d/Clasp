using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Clasp.Binding;
using Clasp.Data.Terms;

namespace Clasp.Data.Metadata
{
    internal sealed class ExpansionState
    {
        public readonly Environment Env;
        public readonly BindingStore Store;
        public readonly int Phase;

        // Keep track of new scopes we paint on, so that we can strip them off quoted syntax
        public readonly HashSet<uint> NewScopes;

        public readonly HashSet<uint> CurrentMacroIntroductionScope; //???

        public readonly ScopeTokenGenerator TokenGen;

        public ExpansionState(Environment env, BindingStore bs, int phase, ScopeTokenGenerator gen)
        {
            Env = env;
            Store = bs;
            Phase = phase;

            NewScopes = new HashSet<uint>();

            TokenGen = gen;
        }

        public ExpansionState WithExtendedEnv() => new ExpansionState(new EnvFrame(Env), Store, Phase, TokenGen);
        public ExpansionState WithNextPhase() => new ExpansionState(new EnvFrame(Env.TopLevel), Store, Phase + 1, TokenGen);


        public string ResolveBindingName(Syntax stx) => Store.ResolveBindingName(stx, Phase);
        public void RenameInCurrentScope(Syntax stx, string bindingName) => Store.RenameInScope(stx, Phase, bindingName);

        public string? MaybeDereferenceBinding(string bindingName)
        {
            if (Env.TryGetValue(bindingName, out Term? deref)
                && deref is Syntax<Symbol> derefId)
            {
                return derefId.Expose.Name;
            }
            return null;
        }

        public void PaintScope(Syntax stx, params uint[] scopes)
        {
            Syntax.PaintScope(stx, Phase, scopes);
            NewScopes.UnionWith(scopes);
        }
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

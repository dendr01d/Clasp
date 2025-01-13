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

        public readonly ScopeTokenGenerator TokenGen;

        public ExpansionState(Environment env, BindingStore bs, int phase, ScopeTokenGenerator gen)
        {
            Env = env;
            Store = bs;
            Phase = phase;
            TokenGen = gen;
        }

        public ExpansionState WithExtendedEnv() => new ExpansionState(new EnvFrame(Env), Store, Phase, TokenGen);
        public ExpansionState WithNextPhase() => new ExpansionState(new EnvFrame(Env.TopLevel), Store, Phase + 1, TokenGen);

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

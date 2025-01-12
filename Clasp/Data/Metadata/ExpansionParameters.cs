using Clasp.Binding;

namespace Clasp.Data.Metadata
{
    internal sealed class ExpansionParameters
    {
        public readonly ExpansionEnv Env;
        public readonly BindingStore Store;
        public readonly int Phase;

        public readonly ScopeTokenGenerator TokenGen;

        public ExpansionParameters(ExpansionEnv env, BindingStore bs, int phase, ScopeTokenGenerator gen)
        {
            Env = env;
            Store = bs;
            Phase = phase;
            TokenGen = gen;
        }

        public ExpansionParameters WithExtendedEnv() => new ExpansionParameters(new ExpansionEnv(Env), Store, Phase, TokenGen);
        public ExpansionParameters WithNextPhase() => new ExpansionParameters(new ExpansionEnv(Env.TopLevel), Store, Phase + 1, TokenGen);
    }
}

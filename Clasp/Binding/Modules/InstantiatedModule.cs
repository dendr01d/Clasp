using Clasp.Binding.Environments;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Binding.Modules
{
    internal sealed class InstantiatedModule : ParsedModule
    {
        public readonly RootEnv EnrichedEnvironment;
        public override bool Visited => true;
        public override bool Instantiated => true;

        public InstantiatedModule(string name, Identifier[] ids, Scope scp, RootEnv env)
            : base(name, ids, scp)
        {
            EnrichedEnvironment = env;
        }
    }
}

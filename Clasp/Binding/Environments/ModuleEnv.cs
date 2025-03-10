using System.Diagnostics.CodeAnalysis;

using Clasp.Binding.Modules;
using Clasp.Data.Terms;

namespace Clasp.Binding.Environments
{
    internal sealed class ModuleEnv : DynamicEnv
    {
        private readonly InstantiatedModule _mdl;

        public ModuleEnv(ClaspEnvironment pred, InstantiatedModule moduleRef) : base(pred)
        {
            _mdl = moduleRef;
        }

        public override bool TryGetValue(string key, [NotNullWhen(true)] out Term? value)
        {
            if (!_mdl.EnrichedEnvironment.TryGetValue(key, out value))
            {
                return Predecessor.TryGetValue(key, out value);
            }
            return true;
        }
    }
}

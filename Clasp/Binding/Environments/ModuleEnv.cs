using System.Diagnostics.CodeAnalysis;

using Clasp.Binding.Modules;
using Clasp.Data.Terms;

namespace Clasp.Binding.Environments
{
    internal sealed class ModuleEnv : DynamicEnv
    {
        Module Handle;

        public ModuleEnv(ClaspEnvironment pred, Module moduleRef) : base(pred)
        {
            Handle = moduleRef;
        }

        public override bool TryGetValue(string key, [NotNullWhen(true)] out Term? value)
        {
            return Handle.TryLookup(key, out value);
        }
    }
}

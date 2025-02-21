using Clasp.Data.Terms;
using System.Diagnostics.CodeAnalysis;

using Clasp.Data.Terms.SyntaxValues;
using Clasp.Modules;

namespace Clasp.Binding.Environments
{
    internal sealed class ImportedEnv : DynamicEnv
    {
        public readonly string ModuleName;
        public Module Module { get; private set; }

        public override RootEnv Root { get; }

        public ImportedEnv(RootEnv root, Module mdl) : base(root.Predecessor)
        {
            ModuleName = mdl.Name;
            Module = mdl;
            Root = root;
        }
        public override bool TryGetValue(string key, [NotNullWhen(true)] out Term? value)
        {
            if (_definitions.TryGetValue(key, out value))
            {
                return true;
            }
            else if (Predecessor is not null)
            {
                return Predecessor.TryGetValue(key, out value);
            }

            value = null;
            return false;
        }
    }
}

using System.Collections.Generic;
using System.Linq;

using Clasp.Binding.Modules;

namespace Clasp.Binding.Environments
{
    /// <summary>
    /// Represents the "top level" environment of a program or module
    /// </summary>
    internal sealed class RootEnv : MutableEnv
    {
        private readonly Dictionary<string, ModuleEnv> _modules;
        public readonly List<Scope> ImplicitScopes;

        public override RootEnv Root => this;

        public RootEnv() : base(StaticEnv.Instance)
        {
            _modules = new Dictionary<string, ModuleEnv>();
            ImplicitScopes = new List<Scope>() { StaticEnv.ImplicitScope };
        }

        public RootEnv(RootEnv original) : base(original.Predecessor)
        {
            _modules = original._modules.ToDictionary(x => x.Key, x => x.Value);
            ImplicitScopes = original.ImplicitScopes.ToList();
        }

        public void InstallModule(InstantiatedModule mdl)
        {
            if (!_modules.ContainsKey(mdl.Name))
            {
                ModuleEnv mEnv = new ModuleEnv(this.Predecessor, mdl);
                Predecessor = mEnv; // insert at base of linked list

                _modules.Add(mdl.Name, mEnv);
            }
        }
    }
}

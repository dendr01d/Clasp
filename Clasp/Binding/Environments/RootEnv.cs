using System.Collections.Generic;
using System.Linq;

using Clasp.Binding.Modules;
using Clasp.Exceptions;

namespace Clasp.Binding.Environments
{
    /// <summary>
    /// Represents the "top level" environment of a program or module
    /// </summary>
    internal sealed class RootEnv : MutableEnv
    {
        private readonly Dictionary<string, ModuleEnv> _modules;

        public override RootEnv Root => this;

        public RootEnv() : base(StaticEnv.Instance)
        {
            _modules = new Dictionary<string, ModuleEnv>();
        }

        public RootEnv(RootEnv original) : base(original.Predecessor)
        {
            _modules = original._modules.ToDictionary(x => x.Key, x => x.Value);
        }

        public void InstallModule(InstantiatedModule mdl)
        {
            if (!_modules.ContainsKey(mdl.Name))
            {
                ModuleEnv mEnv = new ModuleEnv(this.Predecessor, mdl);
                Predecessor = mEnv; // insert at base of linked list

                _modules.Add(mdl.Name, mEnv);

                foreach(ModuleEnv otherEnv in _modules.Values)
                {
                    foreach(var key in mdl.ExportedIds)
                    {
                        if (otherEnv.ContainsKey(key.Expose().Name))
                        {
                            throw new ClaspGeneralException("Collision error on {0} whilst installing module '{1}'.", key.Expose().Name, mdl.Name);
                        }
                    }
                }
            }
        }
    }
}

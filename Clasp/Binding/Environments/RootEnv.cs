using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Exceptions;
using Clasp.Modules;

namespace Clasp.Binding.Environments
{
    internal sealed class RootEnv : MutableEnv
    {
        private readonly Dictionary<string, ImportedEnv> _modules;
        public readonly List<Scope> ImplicitScopes;

        public override RootEnv Root => this;

        public RootEnv() : base(StaticEnv.Instance)
        {
            _modules = new Dictionary<string, ImportedEnv>();
            ImplicitScopes = new List<Scope>() { StaticEnv.Instance.ImplicitScope };
        }

        public void InstallModule(CompiledModule mdl)
        {
            if (!_modules.ContainsKey(mdl.Name))
            {
                ImportedEnv mEnv = new ImportedEnv(this, mdl);
                Predecessor = mEnv; // insert at base of linked list
                ImplicitScopes.Add(mdl.OutsideEdge);
            }
        }

        public bool TryGetModule(string name, out Module? module)
        {
            if (_modules.TryGetValue(name, out ImportedEnv? env))
            {
                module = env.Module;
                return true;
            }

            module = null;
            return false;
        }
    }
}

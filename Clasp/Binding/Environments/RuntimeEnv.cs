using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Binding.Modules;
using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Exceptions;

namespace Clasp.Binding.Environments
{
    /// <summary>
    /// The "base" environment of a clasp program. 
    /// </summary>
    internal sealed class RuntimeEnv : MutableEnv
    {
        public readonly Processor ParentProcess;

        private LibraryEnv _predecessor;
        public override LibraryEnv Predecessor => _predecessor;
        public override RuntimeEnv Runtime => this;
        public readonly StaticEnv StaticLibrary;

        private readonly Dictionary<string, ModuleEnv> _modules;
        public readonly List<Scope> ImplicitScopes;

        public RuntimeEnv(Processor processor) : base()
        {
            ParentProcess = processor;
            StaticLibrary = new StaticEnv(this);
            _predecessor = StaticLibrary;

            _modules = new Dictionary<string, ModuleEnv>();
            ImplicitScopes = new List<Scope>() { _predecessor.ImplicitScope };
        }

        public void InstallModule(string name, Syntax stx)
        {
            if (!_modules.ContainsKey(name))
            {
                ModuleEnv newModule = new ModuleEnv(this, name, stx);
                _predecessor = newModule; // insert at base of linked list
                ImplicitScopes.Add(newModule.ImplicitScope);
            }
        }

        public bool TryGetModule(string name, out Module? module)
        {
            if (_modules.TryGetValue(name, out ModuleEnv? env))
            {
                module = env.Module;
                return true;
            }

            module = null;
            return false;
        }
    }
}

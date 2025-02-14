using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;
using Clasp.Process;

namespace Clasp.Binding.Environments
{
    internal sealed class SuperEnvironment : Environment
    {
        // There are a lot of operational values that would ordinarily be squirreled away somewhere in the environment
        // I'm just going to link back to the host processor so I can store and access these things directly.
        public readonly Processor ParentProcess;

        // Holds the intrinsic bindings for things like special keywords and primitive procedures
        private readonly Dictionary<string, Term> _staticBindings;

        // Holds definitions imported from modules, indexed by module name
        private readonly List<KeyValuePair<string, Environment>> _importedModules;


        public override SuperEnvironment GlobalEnv => this;

        public SuperEnvironment(Processor processor) : base(0)
        {
            ParentProcess = processor;
            _staticBindings = new Dictionary<string, Term>();
            _importedModules = new();
        }

        public void DefineInitial(string key, Term value)
        {
            _staticBindings.Add(key, value);
        }

        public void DefineCoreForm(Symbol sym) => DefineInitial(sym.Name, sym);

        public override Term LookUp(string name)
        {
            if (_mutableBindings.TryGetValue(name, out Term? result1))
            {
                return result1;
            }

            foreach(var kvp in _importedModules)
            {
                if (kvp.Value.TryGetValue(name, out Term? result2))
                {
                    return result2;
                }
            }
            
            
            if (_staticBindings.TryGetValue(name, out Term? result3))
            {
                return result3;
            }

            throw new ClaspGeneralException("Tried to access binding of '{0}' that doesn't exist.", name);

        }

        public override bool ContainsKey(string key)
        {
            return _mutableBindings.ContainsKey(key)
                || _importedModules.Any(x => x.Value.ContainsKey(key))
                || _staticBindings.ContainsKey(key);
        }

        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out Term value)
        {
            if (_mutableBindings.TryGetValue(key, out Term? mutableValue))
            {
                value = mutableValue;
                return true;
            }

            foreach(var kvp in _importedModules)
            {
                if (kvp.Value.TryGetValue(key, out Term? moduleValue))
                {
                    value = moduleValue;
                    return true;
                }
            }

            if (_staticBindings.TryGetValue(key, out Term? staticValue))
            {
                value = staticValue;
                return true;
            }

            value = null;
            return false;
        }
    }
}

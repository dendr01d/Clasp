using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;

namespace Clasp.Binding.Environments
{
    internal sealed class SuperEnvironment : Environment
    {
        private readonly Dictionary<string, Term> _staticBindings;

        public override SuperEnvironment TopLevel => this;
        public override bool IsTopLevel => true;

        public SuperEnvironment() : base(0)
        {
            _staticBindings = new Dictionary<string, Term>();
        }

        protected override IEnumerable<Environment> EnumerateScope()
        {
            yield return this;
            yield break;
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
            else if (_staticBindings.TryGetValue(name, out Term? result2))
            {
                return result2;
            }
            else
            {
                throw new MissingBindingException(name);
            }
        }

        public override bool StaticallyBinds(string name) => _staticBindings.ContainsKey(name);

        public override bool ContainsKey(string key) => _mutableBindings.ContainsKey(key) || _staticBindings.ContainsKey(key);
        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out Term value)
        {
            if (_mutableBindings.TryGetValue(key, out Term? mutableValue))
            {
                value = mutableValue;
                return true;
            }
            else if (_staticBindings.TryGetValue(key, out Term? staticValue))
            {
                value = staticValue;
                return true;
            }

            value = null;
            return false;
        }
    }
}

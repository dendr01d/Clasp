using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Clasp.Data.Terms;
using Clasp.Exceptions;

namespace Clasp.Binding.Environments
{
    internal abstract class MutableEnv : ClaspEnvironment
    {
        protected readonly Dictionary<string, Term> _definitions;
        public abstract RootEnv Root { get; }

        protected MutableEnv(ClaspEnvironment? pred) : base(pred)
        {
            _definitions = new Dictionary<string, Term>();
        }

        public override bool ContainsKey(string key) => _definitions.ContainsKey(key);

        public Closure Enclose() => new Closure(this);

        public void Define(string key, Term value)
        {
            if (_definitions.ContainsKey(key))
            {
                throw new ClaspGeneralException(
                    "Cannot define '{0}' as {1} in environment -- '{0}' is already defined as {2}.",
                    key, value, _definitions[key]);
            }
            _definitions[key] = value;
        }

        public void Mutate(string key, Term value)
        {
            if (!_definitions.ContainsKey(key))
            {
                throw new ClaspGeneralException(
                    "Cannot mutate definition of '{0}' in environment -- '{0}' is undefined.", key);
            }
            _definitions[key] = value;
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

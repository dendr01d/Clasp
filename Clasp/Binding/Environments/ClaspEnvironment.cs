using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Data.Terms;
using Clasp.Exceptions;

namespace Clasp.Binding.Environments
{
    internal abstract class ClaspEnvironment
    {
        protected readonly Dictionary<string, Term> _definitions;

        public abstract ClaspEnvironment Predecessor { get; }
        public abstract RuntimeEnv Runtime { get; }

        protected ClaspEnvironment()
        {
            _definitions = new Dictionary<string, Term>();
        }

        public Term Lookup(string name) => TryGetValue(name, out Term? value) ? value : Undefined.Value;

        public bool Defines(string name) => _definitions.ContainsKey(name);

        public void Define(string key, Term value)
        {
            if (_definitions.ContainsKey(key))
            {
                throw new ClaspGeneralException("Cannot define '{0}' as {1} in environment -- '{0}' is already defined as {2}.",
                    key, value, _definitions[key]);
            }
            _definitions[key] = value;
        }

        public virtual bool TryGetValue(string key, [MaybeNullWhen(false)] out Term value)
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

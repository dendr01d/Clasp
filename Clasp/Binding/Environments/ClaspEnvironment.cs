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

        protected ClaspEnvironment()
        {
            _definitions = new Dictionary<string, Term>();
        }

        public Term Lookup(string name) => TryGetValue(name, out Term? value) ? value : Undefined.Value;

        public abstract bool TryGetValue(string key, [NotNullWhen(true)] out Term? value);

    }
}

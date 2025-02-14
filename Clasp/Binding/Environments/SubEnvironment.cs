using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;

namespace Clasp.Binding.Environments
{
    internal class SubEnvironment : Environment
    {
        protected readonly Environment _next;
        public override SuperEnvironment GlobalEnv { get; }

        public SubEnvironment(Environment ancestor) : base(ancestor.Depth + 1)
        {
            _next = ancestor;
            GlobalEnv = ancestor.GlobalEnv;
        }

        public override Term LookUp(string name)
        {
            if (_mutableBindings.TryGetValue(name, out Term? result))
            {
                return result;
            }
            else
            {
                return _next.LookUp(name);
            }
        }

        public override bool ContainsKey(string key) => _mutableBindings.ContainsKey(key) || _next.ContainsKey(key);
        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out Term value)
        {
            return _mutableBindings.TryGetValue(key, out value)
                || _next.TryGetValue(key, out value);
        }
    }
}

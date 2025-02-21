using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;
using Clasp.Exceptions;

namespace Clasp.Binding.Environments
{
    internal abstract class DynamicEnv : ClaspEnvironment
    {
        public ClaspEnvironment Predecessor { get; protected set; }
        public abstract RootEnv Root { get; }

        protected DynamicEnv(ClaspEnvironment pred)
        {
            Predecessor = pred;
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

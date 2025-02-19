using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;
using Clasp.Exceptions;

namespace Clasp.Binding.Environments
{
    internal abstract class MutableEnv : ClaspEnvironment
    {
        public Closure Enclose() => new Closure(this);

        public void Mutate(string key, Term value)
        {
            if (!_definitions.ContainsKey(key))
            {
                throw new ClaspGeneralException("Cannot mutate definition of '{0}' in environment -- '{0}' is undefined.", key);
            }
            _definitions[key] = value;
        }
    }
}

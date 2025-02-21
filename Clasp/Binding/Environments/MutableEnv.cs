using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;
using Clasp.Exceptions;

namespace Clasp.Binding.Environments
{
    internal abstract class MutableEnv : DynamicEnv
    {
        protected MutableEnv(ClaspEnvironment pred) : base(pred) { }

        public Closure Enclose() => new Closure(this);

        public virtual void Define(string key, Term value)
        {
            if (_definitions.ContainsKey(key))
            {
                throw new ClaspGeneralException(
                    "Cannot define '{0}' as {1} in environment -- '{0}' is already defined as {2}.",
                    key, value, _definitions[key]);
            }
            _definitions[key] = value;
        }

        public virtual void Mutate(string key, Term value)
        {
            if (!_definitions.ContainsKey(key))
            {
                throw new ClaspGeneralException(
                    "Cannot mutate definition of '{0}' in environment -- '{0}' is undefined.", key);
            }
            _definitions[key] = value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;

namespace Clasp.Ops.Functions
{
    internal abstract class FunctionBase
    {
        public readonly int Arity;
        public readonly bool Variadic;

        protected FunctionBase(int arity, bool variadic)
        {
            Arity = arity;
            Variadic = variadic;
        }

        public bool TryOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            result = null;
            if (Variadic && args.Length >= Arity
                || args.Length == Arity)
            {
                return TryMatchAndOperate(args, out result);
            }
            return false;
        }

        protected abstract bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result);
    }
}

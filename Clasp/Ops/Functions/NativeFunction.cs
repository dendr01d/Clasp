using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops.Functions
{
    /// <summary>
    /// Functions that work strictly on native CLASP <see cref="Term"/> objects.
    /// </summary>
    internal abstract class NativeFunction : FunctionBase
    {
        protected NativeFunction(int arity, bool variadic) : base(arity, variadic) { }
    }

    internal sealed class NativeNullary : NativeFunction
    {
        private readonly Func<Term> _operation;
        public NativeNullary(Func<Term> op) : base(0, false) => _operation = op;
        protected override bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            result = _operation();
            return true;
        }
    }

    internal sealed class NativeUnary<T0> : NativeFunction
        where T0 : Term
    {
        private readonly Func<T0, Term> _operation;
        public NativeUnary(Func<T0, Term> op) : base(1, false) => _operation = op;
        protected override bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args[0] is T0 arg0)
            {
                result = _operation(arg0);
                return true;
            }
            result = null;
            return false;
        }
    }

    internal sealed class NativeBinary<T0, T1> : NativeFunction
        where T0 : Term
        where T1 : Term
    {
        private readonly Func<T0, T1, Term> _operation;
        public NativeBinary(Func<T0, T1, Term> op) : base(2, false) => _operation = op;
        protected override bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args[0] is T0 arg0 && args[1] is T1 arg1)
            {
                result = _operation(arg0, arg1);
                return true;
            }
            result = null;
            return false;
        }
    }

    internal sealed class NativeTernary<T0, T1, T2> : NativeFunction
        where T0 : Term
        where T1 : Term
        where T2 : Term
    {
        private readonly Func<T0, T1, T2, Term> _operation;
        public NativeTernary(Func<T0, T1, T2, Term> op) : base(3, false) => _operation = op;
        protected override bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args[0] is T0 arg0 && args[1] is T1 arg1 && args[2] is T2 arg2)
            {
                result = _operation(arg0, arg1, arg2);
                return true;
            }
            result = null;
            return false;
        }
    }

    internal sealed class NativeVariadic<V> : NativeFunction
        where V : Term
    {
        private readonly Func<V[], Term> _operation;
        public NativeVariadic(Func<V[], Term> op) : base(-1, true) => _operation = op;
        protected override bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args.Cast<V>().ToArray() is V[] varArgs && !varArgs.Any(x => x is null))
            {
                result = _operation(varArgs);
                return true;
            }
            result = null;
            return false;
        }
    }
}

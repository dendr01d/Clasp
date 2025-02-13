using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal abstract class NativeProcedure
    {
        public readonly int Arity;
        public readonly bool Variadic;
        public readonly bool Pure;

        protected NativeProcedure(int arity, bool variadic, bool pure)
        {
            Arity = arity;
            Variadic = variadic;
            Pure = pure;
        }

        public bool TryOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            result = null;
            if ((Variadic && args.Length >= Arity)
                || args.Length == Arity)
            {
                return TryMatchAndOperate(args, out result);
            }
            return false;
        }

        protected abstract bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result);
    }

    internal sealed class NullaryOp : NativeProcedure
    {
        private readonly Func<Term> _operation;
        public NullaryOp(Func<Term> op) : base(0, false, true) => _operation = op;
        protected override bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            result = _operation();
            return true;
        }
    }

    internal sealed class UnaryOp<T0> : NativeProcedure
        where T0 : Term
    {
        private readonly Func<T0, Term> _operation;
        public UnaryOp(Func<T0, Term> op) : base(1, false, true) => _operation = op;
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

    internal sealed class BinaryOp<T0, T1> : NativeProcedure
        where T0 : Term
        where T1 : Term
    {
        private readonly Func<T0, T1, Term> _operation;
        public BinaryOp(Func<T0, T1, Term> op) : base(2, false, true) => _operation = op;
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

    internal sealed class TernaryOp<T0, T1, T2> : NativeProcedure
        where T0 : Term
        where T1 : Term
        where T2 : Term
    {
        private readonly Func<T0, T1, T2, Term> _operation;
        public TernaryOp(Func<T0, T1, T2, Term> op) : base(3, false, true) => _operation = op;
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

    internal sealed class VarOp<V> : NativeProcedure
        where V : Term
    {
        private readonly Func<V[], Term> _operation;
        public VarOp(Func<V[], Term> op) : base(-1, true, true) => _operation = op;
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Data.Terms;
using Clasp.Data.VirtualMachine;

namespace Clasp.Ops.Functions
{
    internal abstract class PrimitiveOperation
    {
        public readonly int Arity;
        public readonly bool Variadic;

        protected PrimitiveOperation(int arity, bool variadic)
        {
            Arity = arity;
            Variadic = variadic;
        }

        public bool TryOperate(MachineState mx, object[] args, [NotNullWhen(true)] out Term? result)
        {
            result = null;
            if (Variadic && args.Length >= Arity
                || args.Length == Arity)
            {
                return TryMatchAndOperate(mx, args, out result);
            }
            return false;
        }

        protected abstract bool TryMatchAndOperate(MachineState mx, object[] args, [NotNullWhen(true)] out Term? result);
    }

    internal sealed class NullaryFn : PrimitiveOperation
    {
        private readonly Func<Term> _operation;
        public NullaryFn(Func<Term> op) : base(0, false) => _operation = op;
        protected override bool TryMatchAndOperate(MachineState mx, object[] args, [NotNullWhen(true)] out Term? result)
        {
            result = _operation();
            return true;
        }
    }

    internal sealed class NullaryMxFn : PrimitiveOperation
    {
        private readonly Func<MachineState, Term> _operation;
        public NullaryMxFn(Func<MachineState, Term> op) : base(0, false) => _operation = op;
        protected override bool TryMatchAndOperate(MachineState mx, object[] args, [NotNullWhen(true)] out Term? result)
        {
            result = _operation(mx);
            return true;
        }
    }

    internal sealed class UnaryFn<T0> : PrimitiveOperation
        where T0 : Term
    {
        private readonly Func<T0, Term> _operation;
        public UnaryFn(Func<T0, Term> op) : base(1, false) => _operation = op;
        protected override bool TryMatchAndOperate(MachineState mx, object[] args, [NotNullWhen(true)] out Term? result)
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

    internal sealed class UnaryMxFn<T0> : PrimitiveOperation
        where T0 : Term
    {
        private readonly Func<MachineState, T0, Term> _operation;
        public UnaryMxFn(Func<MachineState, T0, Term> op) : base(1, false) => _operation = op;
        protected override bool TryMatchAndOperate(MachineState mx, object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args[0] is T0 arg0)
            {
                result = _operation(mx, arg0);
                return true;
            }
            result = null;
            return false;
        }
    }

    internal sealed class BinaryFn<T0, T1> : PrimitiveOperation
        where T0 : Term
        where T1 : Term
    {
        private readonly Func<T0, T1, Term> _operation;
        public BinaryFn(Func<T0, T1, Term> op) : base(2, false) => _operation = op;
        protected override bool TryMatchAndOperate(MachineState mx, object[] args, [NotNullWhen(true)] out Term? result)
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

    internal sealed class BinaryMxFn<T0, T1> : PrimitiveOperation
        where T0 : Term
        where T1 : Term
    {
        private readonly Func<MachineState, T0, T1, Term> _operation;
        public BinaryMxFn(Func<MachineState, T0, T1, Term> op) : base(2, false) => _operation = op;
        protected override bool TryMatchAndOperate(MachineState mx, object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args[0] is T0 arg0 && args[1] is T1 arg1)
            {
                result = _operation(mx, arg0, arg1);
                return true;
            }
            result = null;
            return false;
        }
    }

    internal sealed class TernaryFn<T0, T1, T2> : PrimitiveOperation
        where T0 : Term
        where T1 : Term
        where T2 : Term
    {
        private readonly Func<T0, T1, T2, Term> _operation;
        public TernaryFn(Func<T0, T1, T2, Term> op) : base(3, false) => _operation = op;
        protected override bool TryMatchAndOperate(MachineState mx, object[] args, [NotNullWhen(true)] out Term? result)
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

    internal sealed class TernaryMxFn<T0, T1, T2> : PrimitiveOperation
        where T0 : Term
        where T1 : Term
        where T2 : Term
    {
        private readonly Func<MachineState, T0, T1, T2, Term> _operation;
        public TernaryMxFn(Func<MachineState, T0, T1, T2, Term> op) : base(3, false) => _operation = op;
        protected override bool TryMatchAndOperate(MachineState mx, object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args[0] is T0 arg0 && args[1] is T1 arg1 && args[2] is T2 arg2)
            {
                result = _operation(mx, arg0, arg1, arg2);
                return true;
            }
            result = null;
            return false;
        }
    }

    internal sealed class VariadicFn<V> : PrimitiveOperation
        where V : Term
    {
        private readonly Func<V[], Term> _operation;
        public VariadicFn(Func<V[], Term> op) : base(-1, true) => _operation = op;
        protected override bool TryMatchAndOperate(MachineState mx, object[] args, [NotNullWhen(true)] out Term? result)
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

    internal sealed class VariadicMxFn<V> : PrimitiveOperation
        where V : Term
    {
        private readonly Func<MachineState, V[], Term> _operation;
        public VariadicMxFn(Func<MachineState, V[], Term> op) : base(-1, true) => _operation = op;
        protected override bool TryMatchAndOperate(MachineState mx, object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args.Cast<V>().ToArray() is V[] varArgs && !varArgs.Any(x => x is null))
            {
                result = _operation(mx, varArgs);
                return true;
            }
            result = null;
            return false;
        }
    }
}

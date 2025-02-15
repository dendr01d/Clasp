using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;
using Clasp.Data.VirtualMachine;

namespace Clasp.Ops.Functions
{
    /// <summary>
    /// Functions that perform system-level work parameterized with <see cref="Term"/> objects.
    /// </summary>
    internal abstract class SystemFunction : FunctionBase
    {
        protected SystemFunction(int arity, bool variadic) : base(arity, variadic) { }
    }

    internal sealed class SystemNullary : SystemFunction
    {
        private readonly Func<MachineState, Term> _operation;
        public SystemNullary(Func<MachineState, Term> op) : base(0, false) => _operation = op;
        protected override bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args[0] is MachineState mx)
            {
                result = _operation(mx);
                return true;
            }
            result = null;
            return false;
        }
    }

    internal sealed class SystemUnary<T1> : SystemFunction
        where T1 : Term
    {
        private readonly Func<MachineState, T1, Term> _operation;
        public SystemUnary(Func<MachineState, T1, Term> op) : base(1, false) => _operation = op;
        protected override bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args[0] is MachineState mx && args[1] is T1 arg1)
            {
                result = _operation(mx, arg1);
                return true;
            }
            result = null;
            return false;
        }
    }

    internal sealed class SystemBinary<T1, T2> : SystemFunction
        where T1 : Term
        where T2 : Term
    {
        private readonly Func<MachineState, T1, T2, Term> _operation;
        public SystemBinary(Func<MachineState, T1, T2, Term> op) : base(2, false) => _operation = op;
        protected override bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args[0] is MachineState mx && args[1] is T1 arg1 && args[2] is T2 arg2)
            {
                result = _operation(mx, arg1, arg2);
                return true;
            }
            result = null;
            return false;
        }
    }

    internal sealed class SystemTernary<T1, T2, T3> : SystemFunction
        where T1 : Term
        where T2 : Term
        where T3 : Term
    {
        private readonly Func<MachineState, T1, T2, T3, Term> _operation;
        public SystemTernary(Func<MachineState, T1, T2, T3, Term> op) : base(3, false) => _operation = op;
        protected override bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args[0] is MachineState mx && args[1] is T1 arg1 && args[2] is T2 arg2 && args[3] is T3 arg3)
            {
                result = _operation(mx, arg1, arg2, arg3);
                return true;
            }
            result = null;
            return false;
        }
    }

    internal sealed class SystemVariadic<V> : SystemFunction
        where V : Term
    {
        private readonly Func<MachineState, V[], Term> _operation;
        public SystemVariadic(Func<MachineState, V[], Term> op) : base(-1, true) => _operation = op;
        protected override bool TryMatchAndOperate(object[] args, [NotNullWhen(true)] out Term? result)
        {
            if (args[0] is MachineState mx
                && args[1..].Cast<V>().ToArray() is V[] varArgs
                && !varArgs.Any(x => x is null))
            {
                result = _operation(mx, varArgs);
                return true;
            }
            result = null;
            return false;
        }
    }
}

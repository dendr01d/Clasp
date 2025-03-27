using System;

using Clasp.Data.VirtualMachine;
using Clasp.Exceptions;
using Clasp.Ops.Functions;

namespace Clasp.Data.Terms
{
    internal readonly struct PrimitiveProcedure : ITerm, IEquatable<PrimitiveProcedure>
    {
        public readonly Symbol Name;
        private readonly PrimitiveOperation[] _ops;

        public PrimitiveProcedure(string name, params PrimitiveOperation[] ops)
        {
            Name = Symbol.Intern(name);
            _ops = ops;
        }

        public bool Equals(PrimitiveProcedure other) => Name.Equals(other.Name);
        public bool Equals(ITerm? other) => other is PrimitiveProcedure pp && Equals(pp);
        public override bool Equals(object? other) => other is PrimitiveProcedure pp && Equals(pp);
        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => $"#<{Name}>";


        public ITerm Operate(MachineState mx, ITerm[] args)
        {
            foreach (PrimitiveOperation fun in _ops)
            {
                if (fun.TryOperate(mx, args, out ITerm? result))
                {
                    return result;
                }
            }

            throw new ProcessingException.InvalidPrimitiveArgumentsException(args);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.VirtualMachine;
using Clasp.Exceptions;

using Clasp.Ops.Functions;

namespace Clasp.Data.Terms.Procedures
{
    internal abstract class PrimitiveProcedure : Procedure
    {
        public readonly Symbol OpSymbol;

        public PrimitiveProcedure(Symbol opSym) => OpSymbol = opSym;

        public override string ToString() => string.Format("#<{0}>", OpSymbol);
    }

    internal sealed class NativeProcedure : PrimitiveProcedure, IEnumerable<NativeFunction>
    {
        private readonly List<NativeFunction> _nativeOps;

        public NativeProcedure(Symbol opSym, params NativeFunction[] nativeOps) : base(opSym)
        {
            _nativeOps = new List<NativeFunction>(nativeOps);
        }

        public NativeProcedure(string opName, params NativeFunction[] nativeOps)
            : this(Symbol.Intern(opName), nativeOps)
        { }

        public Term Operate(Term[] args)
        {
            foreach (NativeFunction fun in _nativeOps)
            {
                if (fun.TryOperate(args, out Term? result))
                {
                    return result;
                }
            }

            throw new ProcessingException.InvalidPrimitiveArgumentsException(args);
        }

        public IEnumerator<NativeFunction> GetEnumerator() => ((IEnumerable<NativeFunction>)_nativeOps).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_nativeOps).GetEnumerator();
        public void Add(NativeFunction nativeOp) => _nativeOps.Add(nativeOp);

        protected override string FormatType() => string.Format("Nat-Prim({0}:{1})", OpSymbol, _nativeOps.Count);
        internal override string DisplayDebug() => string.Format("{0} ({1}): {2}", nameof(NativeProcedure), nameof(PrimitiveProcedure), OpSymbol);
    }


    internal sealed class SystemProcedure : PrimitiveProcedure, IEnumerable<SystemFunction>
    {
        private readonly List<SystemFunction> _systemOps;

        public SystemProcedure(Symbol opSym, params SystemFunction[] systemOps) : base(opSym)
        {
            _systemOps = new List<SystemFunction>(systemOps);
        }

        public SystemProcedure(string opName, params SystemFunction[] systemOps)
            : this(Symbol.Intern(opName), systemOps)
        { }

        public Term Operate(MachineState mx, Term[] args)
        {
            foreach (SystemFunction fun in _systemOps)
            {
                if (fun.TryOperate([mx, args], out Term? result))
                {
                    return result;
                }
            }

            throw new ProcessingException.InvalidPrimitiveArgumentsException(args);
        }

        public IEnumerator<SystemFunction> GetEnumerator() => ((IEnumerable<SystemFunction>)_systemOps).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_systemOps).GetEnumerator();
        public void Add(SystemFunction systemOp) => _systemOps.Add(systemOp);

        protected override string FormatType() => string.Format("Sys-Prim({0}:{1})", OpSymbol, _systemOps.Count);
        internal override string DisplayDebug() => string.Format("{0} ({1}): {2}", nameof(SystemProcedure), nameof(PrimitiveProcedure), OpSymbol);
    }
}

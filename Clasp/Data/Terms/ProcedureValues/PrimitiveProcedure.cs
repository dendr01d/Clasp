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
    internal sealed class PrimitiveProcedure : Procedure, IEnumerable<PrimitiveOperation>
    {
        public readonly Symbol OpSymbol;
        private readonly List<PrimitiveOperation> _ops;

        public PrimitiveProcedure(Symbol opSym, params PrimitiveOperation[] ops)
        {
            OpSymbol = opSym;
            _ops = ops.ToList();
        }

        public PrimitiveProcedure(string opName, params PrimitiveOperation[] ops)
            : this(Symbol.Intern(opName), ops)
        { }


        public IEnumerator<PrimitiveOperation> GetEnumerator() => ((IEnumerable<PrimitiveOperation>)_ops).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_ops).GetEnumerator();
        public void Add(PrimitiveOperation nativeOp) => _ops.Add(nativeOp);

        protected override string FormatType() => string.Format("Prim({0}:{1})", OpSymbol, _ops.Count);
        public override string ToString() => string.Format("#<{0}>", OpSymbol);
    }
}

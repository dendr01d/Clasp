using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;

namespace Clasp.Ops
{
    internal class ProcedureOps
    {
        public static bool ProcedureCompound(Term t) => t is CompoundProcedure;

        public static bool ProcedurePrimitive(Term t) => t is PrimitiveProcedure;

        public static Term CompoundProcedureArity(Term t)
        {
            return t is CompoundProcedure proc
                ? new Integer(proc.Arity)
                : Data.Terms.Boolean.False;
        }

    }
}

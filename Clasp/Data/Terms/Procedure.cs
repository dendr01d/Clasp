using System.Linq;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;

namespace Clasp.Data.Terms
{
    internal abstract class Procedure : Atom { }

    internal sealed class PrimitiveProcedure : Procedure
    {
        public readonly Symbol Op;
        public PrimitiveProcedure(Symbol op) => Op = op;
        public override string ToString() => string.Format("#<{0}>", Op.ToString().ToLower());
    }

    internal sealed class CompoundProcedure : Procedure
    {
        public readonly string[] Parameters;
        public readonly string? FinalParameter;
        public readonly Environment CapturedEnv;
        public readonly SequentialForm Body;

        public CompoundProcedure(string[] parameters, Environment enclosing, SequentialForm body)
        {
            Parameters = parameters;
            FinalParameter = null;
            CapturedEnv = enclosing;
            Body = body;
        }

        public CompoundProcedure(string[] parameters, string finalParameter, Environment enclosing, SequentialForm body)
            : this(parameters, enclosing, body)
        {
            FinalParameter = finalParameter;
        }


        public override string ToString()
        {
            return string.Format("#<lambda({0})>", Parameters);
        }
    }
}

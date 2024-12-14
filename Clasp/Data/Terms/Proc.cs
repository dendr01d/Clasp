using System.Linq;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;

namespace Clasp.Data.Terms
{
    internal abstract class Proc : Atom { }

    internal sealed class PrimProc : Proc
    {
        public readonly Symbol Op;
        public PrimProc(Symbol op) => Op = op;
        public override string ToString() => string.Format("#<{0}>", Op.ToString().ToLower());
    }

    internal sealed class CompProc : Proc
    {
        public readonly Term Parameters;
        public readonly Environment Closure;
        public readonly Term Body;

        public CompProc(Term parameters, Environment enclosing, Term body)
        {
            Parameters = parameters;
            Closure = new Environment(enclosing);
            Body = body;
        }

        public override string ToString()
        {
            return string.Format("#<lambda({0})>", string.Join(", ", Parameters.ToArray<object>()));
        }
    }
}

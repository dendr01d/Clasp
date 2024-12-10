using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clasp.Binding;
using Clasp.Data.AbstractSyntax;

namespace Clasp.Data.Terms
{
    internal abstract class Proc : Atom { }

    internal sealed class PrimProc : Proc
    {
        public readonly Primitives.Primitive Op;
        public PrimProc(Primitives.Primitive op) => Op = op;
        public override string ToString() => string.Format("#<{0}>", Op.ToString().ToLower());
    }

    internal sealed class CompProc : Proc
    {
        public readonly Variable[] Formals;
        public readonly BindFixed[] Informals;
        public readonly Binding.Environment Closure;
        public readonly Sequence Body;

        public CompProc(Variable[] formals, BindFixed[] informals, Binding.Environment enclosing, Sequence body)
        {
            Formals = formals;
            Informals = informals;
            Closure = new Binding.Environment(enclosing);
            Body = body;
        }

        public override string ToString()
        {
            return string.Format("#<lambda({0})>", string.Join(", ", Formals.ToArray<object>()));
        }
    }

    internal sealed class Transformer : Proc
    {
        public readonly Variable Formal;
        public readonly BindFixed[] Informals;
        public readonly Binding.Environment Closure;
        public readonly Sequence Body;

        public Transformer(Variable formal, BindFixed[] informals, Binding.Environment enclosing, Sequence body)
        {
            Formal = formal;
            Informals = informals;
            Closure = new Binding.Environment(enclosing);
            Body = body;
        }

        public override string ToString()
        {
            return string.Format("#<macro({0})>", Formal);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clasp.Binding;

namespace Clasp.AST
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
        public readonly Var[] Formals;
        public readonly BindFixed[] Informals;
        public readonly Binding.Environment Closure;
        public readonly Sequence Body;

        public CompProc(Var[] formals, BindFixed[] informals, Binding.Environment enclosing, Sequence body)
        {
            Formals = formals;
            Informals = informals;
            //Closure = enclosing.Close();
            Body = body;
        }

        public override string ToString()
        {
            return string.Format("#<lambda({0})>", string.Join(", ", Formals.ToArray<object>()));
        }
    }
}

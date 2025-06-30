using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClaspCompiler.SchemeSemantics;

namespace ClaspCompiler.CompilerPasses
{
    internal static class SimplifyAlgebra
    {
        public static ProgR1 Execute(ProgR1 program)
        {
            // not implementing this yet
            //but it would be for things like (+ x x) -> (* 2 x)
            //reducing the number of variable loads when possible
            //also maybe some other algebraic tricks not covered by constant-folding

            return program;
        }
    }
}

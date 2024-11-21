using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clasp
{
    internal abstract class Procedure : Atom { }

    internal class CompoundProcedure : Procedure
    {
        public readonly Expression Parameters;
        public readonly Expression Body;
        public readonly Environment EnvClosure;

        public CompoundProcedure(Expression parameters, Expression body, Environment outerEnv)
        {
            Parameters = parameters;
            Body = body;
            EnvClosure = new Environment(outerEnv);
        }

        public override string Write() => Pair.ListStar(Symbol.Lambda, Parameters, Body).Write();
        public override string Display() => string.Format("<λ{0}>", Parameters.Display());
    }

    internal class PrimitiveProcedure : Procedure
    {
        private readonly Symbol _referant;
        private readonly Func<Expression, Expression> _operation;

        private PrimitiveProcedure(Symbol referant, Func<Expression, Expression> op)
        {
            _referant = referant;
            _operation = op;
        }

        public static void Manifest(Environment env, string name, Func<Expression, Expression> op)
        {
            Symbol sym = Symbol.Ize(name);
            env.Bind(sym, new PrimitiveProcedure(sym, op));
        }

        public Expression Apply(Expression input) => _operation(input);

        public override string Write() => _referant.Write();
        public override string Display() => string.Format("<{0}>", _referant);
    }

    internal class Macro : Procedure
    {
        private readonly Symbol _referant;
        public readonly Symbol Parameter;
        public readonly Expression Body;
        public readonly Environment Closure;

        public Macro(Symbol referant, Symbol parameter, Expression body, Environment closure)
        {
            _referant = referant;
            Parameter = parameter;
            Body = body;
            Closure = closure;
        }

        public override string Write() => _referant.Write();
        public override string Display() => string.Format("<{0}>", _referant);
    }
}

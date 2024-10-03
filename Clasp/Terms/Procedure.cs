using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clasp
{
    internal abstract class Procedure : Atom
    {
        /// <summary>
        /// Indicates whether the procedure's arguments are evaluated BEFORE the application of the procedure itself
        /// </summary>
        public abstract bool ApplicativeOrder { get; }
    }

    internal class CompoundProcedure : Procedure
    {
        public readonly Expression Parameters;
        public readonly Expression Body;
        public readonly Environment Closure;
        public override bool ApplicativeOrder => true;

        public CompoundProcedure(Expression parameters, Expression body, Environment outerEnv)
        {
            Parameters = parameters;
            Body = body;
            Closure = outerEnv.Enclose();
        }

        public override Expression Deconstruct() => Pair.ListStar(Symbol.Lambda, Parameters.Deconstruct(), Body);
        public override string Serialize() => Deconstruct().Serialize();
        public override string Print() => string.Format("<λ{0}>", Parameters.Print());
    }

    internal class PrimitiveProcedure : Procedure
    {
        private readonly Symbol _referant;
        private readonly Func<Expression, Expression> _operation;
        public override bool ApplicativeOrder => true;

        private PrimitiveProcedure(Symbol referant, Func<Expression, Expression> op)
        {
            _referant = referant;
            _operation = op;
        }

        public static void Manifest(Environment env, string name, Func<Expression, Expression> op)
        {
            Symbol sym = Symbol.Ize(name);
            env.BindNew(sym, new PrimitiveProcedure(sym, op));
        }

        public Expression Apply(Expression input) => _operation(input);

        public override Expression Deconstruct() => _referant;
        public override string Serialize() => _referant.Serialize();
        public override string Print() => string.Format("<{0}>", _referant);
    }
    
    internal class SpecialForm : Procedure
    {
        private readonly Symbol _referant;
        public readonly Evaluator2.Label OpCode;
        public override bool ApplicativeOrder => false;

        private SpecialForm(Symbol referant, Evaluator2.Label op)
        {
            _referant = referant;
            OpCode = op;
        }

        public static void Manifest(Environment env, string name, Evaluator2.Label op)
        {
            Symbol sym = Symbol.Ize(name);
            env.BindNew(sym, new SpecialForm(sym, op));
        }

        public override Expression Deconstruct() => _referant;
        public override string Serialize() => _referant.Serialize();
        public override string Print() => string.Format("<{0}>", _referant);
    }

    internal class Macro : Procedure
    {
        private readonly Symbol _referant;
        public readonly SyntaxTransformer Transformer;
        public readonly Environment Closure;
        public override bool ApplicativeOrder => false;

        private Macro(Symbol referant, SyntaxTransformer xFormer, Environment closure)
        {
            _referant = referant;
            Transformer = xFormer;
            Closure = closure;
        }

        public static void Manifest(Environment env, string name, SyntaxTransformer xFormer)
        {
            Symbol sym = Symbol.Ize(name);
            env.BindNew(sym, new Macro(sym, xFormer, env));
        }

        public override Expression Deconstruct() => _referant;
        public override string Serialize() => _referant.Serialize();
        public override string Print() => string.Format("<{0}>", _referant);
    }

    internal class SyntaxTransformer : Procedure
    {
        public readonly Symbol Parameter;
        public readonly Expression Body;
        public override bool ApplicativeOrder => false;

        public SyntaxTransformer(Symbol parameter, Expression body)
        {
            Parameter = parameter;
            Body = body;
        }

        public override Expression Deconstruct() => Pair.ListStar(Symbol.Mu, Pair.List(Parameter), Body);
        public override string Serialize() => Deconstruct().Serialize();
        public override string Print() => string.Format("<μ({0})>", Parameter.Print());
    }
}

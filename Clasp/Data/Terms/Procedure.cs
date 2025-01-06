using System.Linq;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;

namespace Clasp.Data.Terms
{
    internal abstract class Procedure : Atom
    {
        public abstract int Arity { get; }
        public abstract bool IsVariadic { get; }
    }

    internal sealed class PrimitiveProcedure : Procedure
    {
        public readonly Primitives.Primitive OpCode;
        public readonly System.Func<Term, Term> Operation;

        public override int Arity { get; }
        public override bool IsVariadic { get; }

        public PrimitiveProcedure(Primitives.Primitive op, System.Func<Term, Term> fun, int arity, bool variadic)
        {
            OpCode = op;
            Operation = fun;

            Arity = arity;
            IsVariadic = variadic;
        }
        public override string ToString() => string.Format("#<{0}>", OpCode.ToString());
    }

    internal sealed class CompoundProcedure : Procedure
    {
        public readonly string[] Parameters;
        public readonly string? FinalParameter;
        public readonly EnvFrame CapturedEnv;
        public readonly SequentialForm Body;

        public override int Arity { get; }
        public override bool IsVariadic { get; }

        public CompoundProcedure(string[] parameters, EnvFrame enclosing, SequentialForm body)
        {
            Parameters = parameters;
            FinalParameter = null;
            CapturedEnv = enclosing;
            Body = body;

            Arity = parameters.Length;
            IsVariadic = false;
        }

        public CompoundProcedure(string[] parameters, string? finalParameter, EnvFrame enclosing, SequentialForm body)
            : this(parameters, enclosing, body)
        {
            FinalParameter = finalParameter;
            IsVariadic = FinalParameter is not null;
        }

        public override string ToString() => string.Format("#<lambda({0}{1})>",
                string.Join(", ", Parameters),
                FinalParameter is null ? string.Empty : string.Format("; {0}", FinalParameter));
    }

    internal sealed class MacroProcedure : Procedure
    {
        public readonly string Parameter;
        public readonly SequentialForm Body;

        public override int Arity => 1;
        public override bool IsVariadic => false;

        public MacroProcedure(string parameter, SequentialForm body)
        {
            Parameter = parameter;
            Body = body;
        }

        public override string ToString() => string.Format("#<macro({0})>", Parameter);
    }
}

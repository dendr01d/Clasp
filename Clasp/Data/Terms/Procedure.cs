using System.Linq;

using Clasp.Binding;
using Clasp.Data.AbstractSyntax;

namespace Clasp.Data.Terms
{
    internal abstract class Procedure : Atom
    {
        public abstract int Arity { get; }
        public abstract bool IsVariadic { get; }

        protected override string FormatType() => string.Format("{0}{1}", Arity, IsVariadic ? "+" : string.Empty);
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

        protected override string FormatType() => string.Format("primitive({0})", base.FormatType());
    }

    internal class CompoundProcedure : Procedure
    {
        public readonly string[] Parameters;
        public readonly string? VariadicParameter;
        public readonly Environment CapturedEnv;
        public readonly SequentialForm Body;

        public override int Arity { get; }
        public override bool IsVariadic { get; }

        public CompoundProcedure(string[] parameters, Environment enclosing, SequentialForm body)
        {
            Parameters = parameters;
            VariadicParameter = null;
            CapturedEnv = enclosing;
            Body = body;

            Arity = parameters.Length;
            IsVariadic = false;
        }

        public CompoundProcedure(string[] parameters, string? finalParameter, Environment enclosing, SequentialForm body)
            : this(parameters, enclosing, body)
        {
            VariadicParameter = finalParameter;
            IsVariadic = VariadicParameter is not null;
        }

        public override string ToString() => string.Format("#<lambda({0}{1})>",
                string.Join(", ", Parameters),
                VariadicParameter is null ? string.Empty : string.Format("; {0}", VariadicParameter));

        protected override string FormatType() => string.Format("compound({0})", base.FormatType());
    }

    internal sealed class MacroProcedure : CompoundProcedure
    {
        public override int Arity => 1;
        public override bool IsVariadic => false;

        public MacroProcedure(string parameter, SequentialForm body)
            : base(new string[] { parameter }, StandardEnv.CreateNew(), body)
        { }

        public override string ToString() => "#<macro>";

        protected override string FormatType() => string.Format("macro({0})", base.FormatType());
    }
}

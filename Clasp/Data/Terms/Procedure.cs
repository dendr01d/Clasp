using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;

namespace Clasp.Data.Terms
{
    internal delegate Term PrimitiveOperation(MachineState mx, params Term[] terms);

    internal abstract class Procedure : Atom
    {
        public abstract int[] Arities { get; }
        public abstract bool IsVariadic { get; }

        protected override string FormatType() => string.Format("({0}){1}", string.Join(", ", Arities), IsVariadic ? "+" : string.Empty);
    }

    internal sealed class PrimitiveProcedure : Procedure
    {
        public readonly Symbol OpSymbol;
        public readonly PrimitiveOperation Operation;

        public readonly bool Pure;
        public override int[] Arities { get; }
        public override bool IsVariadic { get; }

        public PrimitiveProcedure(Symbol opSym, PrimitiveOperation fun, bool variadic, params int[] arities)
        {
            OpSymbol = opSym;
            Operation = fun;

            Arities = arities;
            IsVariadic = variadic;
        }

        public PrimitiveProcedure(string name, PrimitiveOperation fun, bool variadic, params int[] arities)
            : this(Symbol.Intern(name), fun, variadic, arities)
        { }

        public override string ToString() => string.Format("#<{0}>", OpSymbol);
        protected override string FormatType() => string.Format("Prim({0})", base.FormatType());
    }

    internal class CompoundProcedure : Procedure
    {
        public readonly string[] Parameters;
        public readonly string? VariadicParameter;
        public readonly string[] InformalParameters;
        public readonly Environment CapturedEnv;
        public readonly SequentialForm Body;

        public override int[] Arities { get; }
        public override bool IsVariadic { get; }

        public CompoundProcedure(string[] parameters, string[] informals, Environment enclosing, SequentialForm body)
        {
            Parameters = parameters;
            VariadicParameter = null;
            InformalParameters = informals;

            CapturedEnv = enclosing;
            Body = body;

            Arities = new int[] { parameters.Length };
            IsVariadic = false;
        }

        public CompoundProcedure(string[] parameters, string? finalParameter, string[] informals, Environment enclosing, SequentialForm body)
            : this(parameters, informals, enclosing, body)
        {
            VariadicParameter = finalParameter;
            IsVariadic = VariadicParameter is not null;
        }

        public override string ToString() => string.Format(
            "#<lambda({0}{1})>",
            string.Join(' ', Parameters),
            VariadicParameter is null ? string.Empty : string.Format(" . {0}", VariadicParameter));
        protected override string FormatType() => string.Format("Lambda({0})", base.FormatType());
    }

    internal sealed class MacroProcedure : CompoundProcedure
    {
        private static readonly int[] _arities = new int[] { 1 };

        public override int[] Arities => _arities;
        public override bool IsVariadic => false;

        public MacroProcedure(string parameter, SequentialForm body)
            : base([parameter], [], StandardEnv.CreateNew(), body)
        { }

        public override string ToString() => string.Format("#<macro({0})>", Parameters[0]);

        protected override string FormatType() => string.Format("Macro({0})", base.FormatType());
    }
}

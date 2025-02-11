using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Ops;

namespace Clasp.Data.Terms
{
    internal abstract class Procedure : Atom
    {
    }

    internal sealed class PrimitiveProcedure : Procedure, IEnumerable<NativeProcedure>
    {
        public readonly Symbol OpSymbol;
        private readonly List<NativeProcedure> _nativeOps;

        public PrimitiveProcedure(Symbol opSym, params NativeProcedure[] nativeOps)
        {
            OpSymbol = opSym;
            _nativeOps = new List<NativeProcedure>(nativeOps);
        }

        public PrimitiveProcedure(string opName, params NativeProcedure[] nativeOps)
            : this(Symbol.Intern(opName), nativeOps)
        { }

        public Term Operate(Term[] args)
        {
            foreach (NativeProcedure np in _nativeOps)
            {
                if (np.TryOperate(args, out Term? result))
                {
                    return result;
                }
            }

            throw new ProcessingException.InvalidPrimitiveArgumentsException(args);
        }

        public IEnumerator<NativeProcedure> GetEnumerator() => ((IEnumerable<NativeProcedure>)_nativeOps).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_nativeOps).GetEnumerator();
        public void Add(NativeProcedure nativeOp) => _nativeOps.Add(nativeOp);

        public override string ToString() => string.Format("#<{0}>", OpSymbol);
        protected override string FormatType() => string.Format("Prim({0}:{1})", OpSymbol, _nativeOps.Count);
    }

    internal class CompoundProcedure : Procedure
    {
        public readonly string[] Parameters;
        public readonly string? VariadicParameter;
        public readonly string[] InformalParameters;
        public readonly Environment CapturedEnv;
        public readonly SequentialForm Body;

        public readonly int Arity;
        public readonly bool IsVariadic;

        public CompoundProcedure(string[] parameters, string? finalParameter, string[] informals, Environment enclosing, SequentialForm body)
        {
            Parameters = parameters;
            VariadicParameter = null;
            InformalParameters = informals;

            CapturedEnv = enclosing;
            Body = body;

            Arity = parameters.Length;
            VariadicParameter = finalParameter;
            IsVariadic = VariadicParameter is not null;
        }

        public CompoundProcedure(string[] parameters, string[] informals, Environment enclosing, SequentialForm body)
            : this(parameters, null, informals, enclosing, body)
        { }

        public override string ToString() => string.Format(
            "#<lambda({0}{1})>",
            string.Join(' ', Parameters),
            VariadicParameter is null ? string.Empty : string.Format(" . {0}", VariadicParameter));
        protected override string FormatType() => string.Format("Lambda({0}{1})", Arity, IsVariadic ? "+" : string.Empty);
    }

    internal sealed class MacroProcedure : CompoundProcedure
    {
        public MacroProcedure(string parameter, SequentialForm body)
            : base([parameter], [], StandardEnv.CreateNew(), body)
        { }

        public override string ToString() => string.Format("#<macro({0})>", Parameters[0]);

        protected override string FormatType() => "Macro";
    }
}

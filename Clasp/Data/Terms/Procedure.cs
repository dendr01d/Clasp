using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.VirtualMachine;
using Clasp.Exceptions;
using Clasp.Ops.Functions;

namespace Clasp.Data.Terms
{
    internal abstract class Procedure : Atom
    {
    }

    internal abstract class PrimitiveProcedure : Procedure
    {
        public readonly Symbol OpSymbol;

        public PrimitiveProcedure(Symbol opSym) => OpSymbol = opSym;

        public override string ToString() => string.Format("#<{0}>", OpSymbol);
    }

    internal sealed class NativeProcedure : PrimitiveProcedure, IEnumerable<NativeFunction>
    {
        private readonly List<NativeFunction> _nativeOps;

        public NativeProcedure(Symbol opSym, params NativeFunction[] nativeOps) : base(opSym)
        {
            _nativeOps = new List<NativeFunction>(nativeOps);
        }

        public NativeProcedure(string opName, params NativeFunction[] nativeOps)
            : this(Symbol.Intern(opName), nativeOps)
        { }

        public Term Operate(Term[] args)
        {
            foreach (NativeFunction fun in _nativeOps)
            {
                if (fun.TryOperate(args, out Term? result))
                {
                    return result;
                }
            }

            throw new ProcessingException.InvalidPrimitiveArgumentsException(args);
        }

        public IEnumerator<NativeFunction> GetEnumerator() => ((IEnumerable<NativeFunction>)_nativeOps).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_nativeOps).GetEnumerator();
        public void Add(NativeFunction nativeOp) => _nativeOps.Add(nativeOp);

        protected override string FormatType() => string.Format("Nat-Prim({0}:{1})", OpSymbol, _nativeOps.Count);
        internal override string DisplayDebug() => string.Format("{0} ({1}): {2}", nameof(NativeProcedure), nameof(PrimitiveProcedure), OpSymbol);
    }

    internal sealed class SystemProcedure : PrimitiveProcedure, IEnumerable<SystemFunction>
    {
        private readonly List<SystemFunction> _systemOps;

        public SystemProcedure(Symbol opSym, params SystemFunction[] systemOps) : base(opSym)
        {
            _systemOps = new List<SystemFunction>(systemOps);
        }

        public SystemProcedure(string opName, params SystemFunction[] systemOps)
            : this(Symbol.Intern(opName), systemOps)
        { }

        public Term Operate(MachineState mx, Term[] args)
        {
            foreach (SystemFunction fun in _systemOps)
            {
                if (fun.TryOperate([mx, args], out Term? result))
                {
                    return result;
                }
            }

            throw new ProcessingException.InvalidPrimitiveArgumentsException(args);
        }

        public IEnumerator<SystemFunction> GetEnumerator() => ((IEnumerable<SystemFunction>)_systemOps).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_systemOps).GetEnumerator();
        public void Add(SystemFunction systemOp) => _systemOps.Add(systemOp);

        protected override string FormatType() => string.Format("Sys-Prim({0}:{1})", OpSymbol, _systemOps.Count);
        internal override string DisplayDebug() => string.Format("{0} ({1}): {2}", nameof(SystemProcedure), nameof(PrimitiveProcedure), OpSymbol);
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

            CapturedEnv = enclosing.Enclose();
            Body = body;

            Arity = parameters.Length;
            VariadicParameter = finalParameter;
            IsVariadic = VariadicParameter is not null;
        }

        public CompoundProcedure(string[] parameters, string[] informals, Environment enclosing, SequentialForm body)
            : this(parameters, null, informals, enclosing, body)
        { }

        public bool TryCoerceMacro([NotNullWhen(true)] out MacroProcedure? macro)
        {
            if (Arity == 1 && !IsVariadic)
            {
                macro = new MacroProcedure(Parameters[0], CapturedEnv, Body);
                return true;
            }

            macro = null;
            return false;
        }

        public override string ToString() => string.Format(
            "#<lambda({0}{1})>",
            string.Join(' ', Parameters),
            VariadicParameter is null ? string.Empty : string.Format(" . {0}", VariadicParameter));
        protected override string FormatType() => string.Format("Lambda({0}{1})", Arity, IsVariadic ? "+" : string.Empty);
        internal override string DisplayDebug() => string.Format("{0}: {1}", nameof(CompoundProcedure), ToString());
    }

    internal sealed class MacroProcedure : CompoundProcedure
    {
        public MacroProcedure(string parameter, Environment enclosing, SequentialForm body)
            : base([parameter], [], enclosing, body)
        { }

        public override string ToString() => string.Format("#<macro({0})>", Parameters[0]);

        protected override string FormatType() => "Macro";
    }
}

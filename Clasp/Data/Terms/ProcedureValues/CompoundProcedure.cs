using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding.Environments;

using Clasp.Data.AbstractSyntax;

namespace Clasp.Data.Terms.Procedures
{

    internal class CompoundProcedure : Procedure
    {
        public readonly string[] Parameters;
        public readonly string? VariadicParameter;
        public readonly string[] InformalParameters;
        public readonly MutableEnv CapturedEnv;
        public readonly Sequential Body;

        public readonly int Arity;
        public readonly bool IsVariadic;

        public CompoundProcedure(string[] parameters, string? finalParameter, string[] informals, MutableEnv enclosing, Sequential body)
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

        public CompoundProcedure(string[] parameters, string[] informals, MutableEnv enclosing, Sequential body)
            : this(parameters, null, informals, enclosing, body)
        { }

        public Closure GetClosure() => CapturedEnv.Enclose();

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
    }
}

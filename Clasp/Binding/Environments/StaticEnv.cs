using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;
using Clasp.Data.Text;
using Clasp.Exceptions;

namespace Clasp.Binding.Environments
{
    /// <summary>
    /// A static singleton environment that sits at the base of all other environment. It contains definitions
    /// for all of CLASP's core form symbols and primitive procedures.
    /// </summary>
    internal sealed class StaticEnv : ClaspEnvironment
    {
        private static readonly Dictionary<string, Term> _definitions = new Dictionary<string, Term>();

        public static readonly StaticEnv Instance = new StaticEnv();
        public static readonly Scope ImplicitScope = new Scope("Static Global", SourceCode.StaticSource);

        public static string ClaspSourceDir = string.Empty;

        private StaticEnv() { }

        static StaticEnv()
        {
            foreach (Symbol sym in CoreKeywords)
            {
                ImplicitScope.AddStaticBinding(sym.Name, BindingType.Special);
                _definitions.Add(sym.Name, sym);
            }

            foreach (PrimitiveProcedure pp in Primitives.PrimitiveProcs)
            {
                ImplicitScope.AddStaticBinding(pp.OpSymbol.Name, BindingType.Primitive);
                _definitions.Add(pp.OpSymbol.Name, pp);
            }
        }

        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out Term value)
        {
            if (_definitions.TryGetValue(key, out value))
            {
                return true;
            }

            // end of the line
            value = null;
            return false;
        }

        private static readonly Symbol[] CoreKeywords = new Symbol[]
        {
            Symbols.Quote,
            Symbols.Quasiquote,
            Symbols.Unquote,
            Symbols.UnquoteSplicing,

            Symbols.QuoteSyntax,
            Symbols.Quasisyntax,
            Symbols.Unsyntax,
            Symbols.UnsyntaxSplicing,

            Symbols.Define,
            Symbols.Set,

            Symbols.If,
            Symbols.Begin,
            Symbols.Apply,
            Symbols.Lambda,

            Symbols.Module,
            Symbols.Import,
            Symbols.Export,

            Symbols.DefineSyntax,
            Symbols.ImportForSyntax,
            Symbols.BeginForSyntax,

            Symbols.Syntax,
            Symbols.Ellipsis,

            Symbols.S_TopBegin,
            Symbols.S_TopDefine,
            Symbols.S_TopVar,

            Symbols.S_Module,
            Symbols.S_Module_Begin,
            Symbols.S_Import,

            Symbols.S_Set,

            Symbols.S_If,
            Symbols.S_Begin,
            Symbols.S_Apply,
            Symbols.S_Lambda,

            Symbols.S_Var,
            Symbols.S_Const,
            Symbols.S_Const_Syntax,

            Symbols.S_PartialDefine,
            Symbols.S_VisitModule
        };

    }
}

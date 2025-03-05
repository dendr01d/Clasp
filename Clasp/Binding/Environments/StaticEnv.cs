using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Clasp.Binding.Modules;
using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Exceptions;
using Clasp.Ops;
using Clasp.Ops.Functions;

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
        public static readonly Scope ImplicitScope = new Scope(SourceCode.StaticSource);

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
            throw new ClaspGeneralException("Could not find definition of '{0}' in environment chain.", key);
        }

        private static readonly Symbol[] CoreKeywords = new Symbol[]
        {
            Symbols.Quote,
            Symbols.QuoteSyntax,

            Symbols.Apply,

            Symbols.Define,
            Symbols.Set,

            Symbols.Lambda,

            Symbols.If,

            Symbols.Begin,

            Symbols.Module,
            Symbols.Import,
            Symbols.Export,

            Symbols.DefineSyntax,
            Symbols.BeginForSyntax,
            Symbols.ImportForSyntax
        };

    }
}

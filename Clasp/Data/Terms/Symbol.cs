using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;

using Clasp.Data.Static;
using Clasp.Data.Terms.ProductValues;

namespace Clasp.Data.Terms
{
    internal class Symbol : Atom
    {
        public string Name { get; protected init; }
        protected Symbol(string name)
        {
            Name = name;
            _interned.Add(name, this);
        }

        private static readonly Dictionary<string, Symbol> _interned = [];
        protected static bool IsInterned(string name) => _interned.ContainsKey(name);

        public static Symbol Intern(string name)
        {
            if (!_interned.ContainsKey(name))
            {
                _interned[name] = new Symbol(name);
            }
            return _interned[name];
        }

        public override string ToString() => Name;
        protected override string FormatType() => "Symbol";
        internal override string DisplayDebug() => string.Format("{0}: {1}", nameof(Symbol), Name);
    }

    internal class GenSym : Symbol
    {
        // Gamma, for "GenSym"
        private const string _SEP = "Γ";

        private static string GenerateUniqueName(string partial)
        {
            string output = partial;
            uint counter = 1;

            while (IsInterned(output))
            {
                output = string.Format("{0}{1}{2}", partial, _SEP, counter++);
            }
            return output;
        }

        public GenSym(string fromName) : base(GenerateUniqueName(fromName)) { }
        public GenSym() : this("GenSym") { }

        protected override string FormatType() => "GenSym";
        internal override string DisplayDebug() => string.Format("{0} ({1}): {2}", nameof(GenSym), nameof(Symbol), Name);
    }

    /// <summary>
    /// Special symbols that cannot be linguistically represented. They act as
    /// "unshadowable" representations of certain important keywords by dint of being
    /// an entirely different Type.
    /// </summary>
    internal sealed class ReservedSymbol : Symbol
    {
        public ReservedSymbol(string name) : base(name) { }

        protected override string FormatType() => "ImpSymbol";
        internal override string DisplayDebug() => string.Format("{0} ({1}): {2}", nameof(ReservedSymbol), nameof(Symbol), Name);
    }

    internal static class Symbols
    {

        public static readonly Symbol Define = Symbol.Intern(Keywords.DEFINE);
        public static readonly Symbol DefineSyntax = Symbol.Intern(Keywords.DEFINE_SYNTAX);
        public static readonly Symbol Set = Symbol.Intern(Keywords.SET);

        public static readonly Symbol Begin = Symbol.Intern(Keywords.BEGIN);
        public static readonly Symbol If = Symbol.Intern(Keywords.IF);
        public static readonly Symbol Lambda = Symbol.Intern(Keywords.LAMBDA);

        public static readonly Symbol Quote = Symbol.Intern(Keywords.QUOTE);
        public static readonly Symbol Quasiquote = Symbol.Intern(Keywords.QUASIQUOTE);
        public static readonly Symbol Unquote = Symbol.Intern(Keywords.UNQUOTE);
        public static readonly Symbol UnquoteSplicing = Symbol.Intern(Keywords.UNQUOTE_SPLICING);

        public static readonly Symbol QuoteSyntax = Symbol.Intern(Keywords.QUOTE_SYNTAX);
        public static readonly Symbol Quasisyntax = Symbol.Intern(Keywords.QUASISYNTAX);
        public static readonly Symbol Unsyntax = Symbol.Intern(Keywords.UNSYNTAX);
        public static readonly Symbol UnsyntaxSplicing = Symbol.Intern(Keywords.UNSYNTAX_SPLICING);

        public static readonly Symbol Syntax = Symbol.Intern(Keywords.SYNTAX);
        public static readonly Symbol Ellipsis = Symbol.Intern(Keywords.ELLIPSIS);

        public static readonly Symbol Apply = Symbol.Intern(Keywords.APPLY);

        public static readonly Symbol Export = Symbol.Intern(Keywords.EXPORT);
        public static readonly Symbol Import = Symbol.Intern(Keywords.IMPORT);
        public static readonly Symbol ImportForSyntax = Symbol.Intern(Keywords.IMPORT_FOR_SYNTAX);
        public static readonly Symbol BeginForSyntax = Symbol.Intern(Keywords.BEGIN_FOR_SYNTAX);

        public static readonly Symbol Module = Symbol.Intern(Keywords.MODULE);

        public static readonly ReservedSymbol StaticTop = new(Keywords.STATIC_TOP);
        public static readonly ReservedSymbol StaticVar = new(Keywords.STATIC_VAR);

        public static readonly ReservedSymbol StaticApply = new(Keywords.STATIC_APPLY);
        //public static readonly Implicit Sp_Datum = new(Keyword.IMP_DATUM);
        //public static readonly Implicit Sp_Top = new(Keyword.IMP_TOP);
        public static readonly ReservedSymbol StaticLambda = new(Keywords.STATIC_LAMBDA);
        //public static readonly Implicit Sp_Var = new(Keyword.IMP_VAR);
        public static readonly ReservedSymbol StaticParDef = new(Keywords.STATIC_PARDEF);
        //public static readonly Implicit Sp_Begin = new(Keyword.IMP_BEGIN);
        //public static readonly Implicit Sp_Module_Begin = new(Keyword.IMP_MODULE_BEGIN);
        public static readonly ReservedSymbol StaticModuleBody = new(Keywords.STATIC_MODULE_BEGIN);

        public static readonly ReservedSymbol StaticParMod = new(Keywords.STATIC_PARMOD);

        public static readonly ReservedSymbol StaticQuote = new(Keywords.STATIC_QUOTE);

        public static readonly ReservedSymbol StaticBegin = new(Keywords.STATIC_BEGIN);
    }
}

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

        #region Default static Symbols

        public static readonly Symbol Define = Intern(Keyword.DEFINE);
        public static readonly Symbol DefineSyntax = Intern(Keyword.DEFINE_SYNTAX);
        public static readonly Symbol Set = Intern(Keyword.SET);

        public static readonly Symbol Begin = Intern(Keyword.BEGIN);
        public static readonly Symbol If = Intern(Keyword.IF);
        public static readonly Symbol Lambda = Intern(Keyword.LAMBDA);

        public static readonly Symbol Quote = Intern(Keyword.QUOTE);
        public static readonly Symbol Quasiquote = Intern(Keyword.QUASIQUOTE);
        public static readonly Symbol Unquote = Intern(Keyword.UNQUOTE);
        public static readonly Symbol UnquoteSplicing = Intern(Keyword.UNQUOTE_SPLICING);

        public static readonly Symbol QuoteSyntax = Intern(Keyword.QUOTE_SYNTAX);
        public static readonly Symbol LetSyntax = Intern(Keyword.LET_SYNTAX);

        public static readonly Symbol Syntax = Intern(Keyword.SYNTAX);
        public static readonly Symbol Quasisyntax = Intern(Keyword.QUASISYNTAX);
        public static readonly Symbol Unsyntax = Intern(Keyword.UNSYNTAX);
        public static readonly Symbol UnsyntaxSplicing = Intern(Keyword.UNSYNTAX_SPLICING);

        public static readonly Symbol Ellipsis = Intern(Keyword.ELLIPSIS);

        public static readonly Symbol Apply = Intern(Keyword.APPLY);

        public static readonly Symbol Export = Intern(Keyword.EXPORT);
        public static readonly Symbol Import = Intern(Keyword.IMPORT);
        public static readonly Symbol ImportForSyntax = Intern(Keyword.IMPORT_FOR_SYNTAX);
        public static readonly Symbol BeginForSyntax = Intern(Keyword.BEGIN_FOR_SYNTAX);

        public static readonly Symbol Module = Intern(Keyword.MODULE);

        #endregion


        protected override string FormatType() => "Symbol";
        internal override string DisplayDebug() => string.Format("{0}: {1}", nameof(Symbol), Name);
    }

    internal class GenSym : Symbol
    {
        // Gamma (G) for GenSym
        // I also considered using ⌠ instead because it interpolates more cleanly
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
    /// Special symbols that cannot be linguistically represented.
    /// They act as "unshadowable" representations of implicit core forms.
    /// </summary>
    /// <remarks>
    /// By separating them into a unique type, they can never be equal to ordinary symbols,
    /// even if they technically have matching names.
    /// </remarks>
    internal sealed class Implicit : Symbol
    {
        private Implicit(string name) : base(name) { }

        public static readonly Implicit Sp_Apply = new(Keyword.IMP_APP);
        public static readonly Implicit Sp_Datum = new(Keyword.IMP_DATUM);
        public static readonly Implicit Sp_Top = new(Keyword.IMP_TOP);
        public static readonly Implicit Sp_Lambda = new(Keyword.IMP_LAMBDA);
        public static readonly Implicit Sp_Var = new(Keyword.IMP_VAR);
        public static readonly Implicit Par_Def = new(Keyword.IMP_PARDEF);
        public static readonly Implicit Sp_Begin = new(Keyword.IMP_BEGIN);
        public static readonly Implicit Sp_Module_Begin = new Implicit(Keyword.IMP_MODULE_BEGIN);

        protected override string FormatType() => "ImpSymbol";
        internal override string DisplayDebug() => string.Format("{0} ({1}): {2}", nameof(Implicit), nameof(Symbol), Name);
    }
}

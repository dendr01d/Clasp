using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

using Clasp.Interfaces;

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

        private static readonly Dictionary<string, Symbol> _interned = new Dictionary<string, Symbol>();
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

        //public static readonly Symbol ImplicitApp = Intern(Keyword.IMP_APP);
        //public static readonly Symbol ImplicitDatum = Intern(Keyword.IMP_DATUM);
        //public static readonly Symbol ImplicitTop = Intern(Keyword.IMP_TOP);
        //public static readonly Symbol ImplicitExpression = Intern(Keyword.IMP_EXPRESSION);
        //public static readonly Symbol ImplicitLambda = Intern(Keyword.IMP_LAMBDA);

        #endregion


        protected override string FormatType() => "Symbol";
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

        public static readonly Implicit SpApply = new Implicit(Keyword.IMP_APP);
        public static readonly Implicit SpDatum = new Implicit(Keyword.IMP_DATUM);
        public static readonly Implicit SpTop = new Implicit(Keyword.IMP_TOP);
        public static readonly Implicit SpLambda = new Implicit(Keyword.IMP_LAMBDA);

        public static readonly Implicit SpVar = new Implicit(Keyword.IMP_VAR);

        public static readonly Implicit ParDef = new Implicit(Keyword.IMP_PARDEF);

        protected override string FormatType() => "ImpSymbol";
    }
}

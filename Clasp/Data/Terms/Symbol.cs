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
        protected Symbol(string name) => Name = name;

        protected static readonly Dictionary<string, Symbol> Interned = new Dictionary<string, Symbol>();
        public static Symbol Intern(string name)
        {
            if (!Interned.ContainsKey(name))
            {
                Interned[name] = new Symbol(name);
            }
            return Interned[name];
        }

        public override string ToString() => Name;

        #region Default static Symbols

        public static readonly Symbol Define = Intern("define");
        public static readonly Symbol DefineSyntax = Intern("define-syntax");
        public static readonly Symbol Set = Intern("set!");

        public static readonly Symbol Begin = Intern("begin");
        public static readonly Symbol If = Intern("if");
        public static readonly Symbol Lambda = Intern("lambda");

        public static readonly Symbol Quote = Intern("quote");
        public static readonly Symbol Quasiquote = Intern("quasiquote");
        public static readonly Symbol Unquote = Intern("unquote");
        public static readonly Symbol UnquoteSplicing = Intern("unquote-splicing");

        public static readonly Symbol QuoteSyntax = Intern("quote-syntax");
        public static readonly Symbol LetSyntax = Intern("let-syntax");

        public static readonly Symbol Syntax = Intern("syntax");
        public static readonly Symbol Quasisyntax = Intern("quasisyntax");
        public static readonly Symbol Unsyntax = Intern("unsyntax");
        public static readonly Symbol UnsyntaxSplicing = Intern("unsyntax-splicing");

        public static readonly Symbol Ellipsis = Intern("...");

        public static readonly Symbol ImplicitApp = Intern("#%app");
        public static readonly Symbol ImplicitDatum = Intern("#%datum");
        public static readonly Symbol ImplicitTop = Intern("#%top");

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

            while (Interned.ContainsKey(output))
            {
                output = string.Format("{0}{1}{2}", partial, _SEP, counter);
            }
            return output;
        }

        public GenSym(string fromName) : base(GenerateUniqueName(fromName)) { }
        public GenSym() : this("GenSym") { }

        protected override string FormatType() => "gensym";
    }
}

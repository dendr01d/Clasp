using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Interfaces;

namespace Clasp.Data.Terms
{
    internal class Symbol : Atom, IBindable
    {
        public string Name { get; protected init; }
        protected Symbol(string name) => Name = name;

        private static readonly Dictionary<string, Symbol> _internment = new Dictionary<string, Symbol>();
        public static Symbol Intern(string name)
        {
            if (!_internment.ContainsKey(name))
            {
                _internment[name] = new Symbol(name);
            }
            return _internment[name];
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

        #endregion
    }

    internal class GenSym : Symbol, IBindable
    {
        private static uint _globalCounter = 0;
        private const string _SEP = "⌠";

        private static string GenerateUniqueName(string partial) => string.Format("{0}{1}{2}", partial, _SEP, _globalCounter++);
        public GenSym() : base(GenerateUniqueName("GenSym")) { }
        public GenSym(string fromName) : base(GenerateUniqueName(fromName)) { }
    }
}

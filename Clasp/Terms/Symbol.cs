using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal class Symbol : Atom
    {
        private static readonly Dictionary<string, Symbol> _internment = new Dictionary<string, Symbol>();

        public readonly string Name;
        protected Symbol(string name) => Name = name;
        public static Symbol Ize(string name)
        {
            if (_internment.TryGetValue(name, out Symbol? interred))
            {
                return interred;
            }
            else
            {
                Symbol output = new Symbol(name);
                _internment[name] = output;
                return output;
            }
        }

        public override string ToPrinted() => Name;
        public override string ToSerialized() => Name;

        #region Standard Symbols

        public static readonly Symbol Lambda = Ize("lambda");
        public static readonly Symbol Flambda = Ize("ƒlambda");
        public static readonly Symbol If = Ize("if");
        public static readonly Symbol Cond = Ize("cond");
        public static readonly Symbol Begin = Ize("begin");
        public static readonly Symbol Eq = Ize("eq?");
        public static readonly Symbol Case = Ize("case");

        public static readonly Symbol Ok = Ize("ok");
        public static readonly Symbol CondElse = Ize("else");

        public static readonly Symbol Quote = Ize("quote");
        public static readonly Symbol Quasiquote = Ize("quasiquote");
        public static readonly Symbol Unquote = Ize("unquote");
        public static readonly Symbol UnquoteSplicing = Ize("unquote-splicing");

        public static readonly Symbol Ellipsis = Ize("...");
        public static readonly Symbol Underscore = Ize("_");

        #endregion
    }

    internal class GenSym : Symbol
    {
        private static int counter = 0;

        public GenSym() : base($"${++counter}") { }
    }
}

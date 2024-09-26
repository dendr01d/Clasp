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
        private static uint _genCounter = 0;

        public readonly string Name;
        private Symbol(string name) => Name = name;

        #region (public) Construction

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

        public static Symbol Generate(string name)
        {
            string check = name;
            uint counter = 0;

            while (_internment.ContainsKey(check))
            {
                check = string.Format("{0}${1}", name, ++counter);
            }

            return Ize(check);
        }

        public static Symbol Generate(Symbol sym) => Generate(sym.Name);

        #endregion
        public override Expression Deconstruct() => this;
        public override string Serialize() => Name;
        public override string Print() => Name;

        #region Standard Symbols

        public static readonly Symbol Lambda = Ize("lambda");
        //public static readonly Symbol Flambda = Ize("ƒlambda");
        public static readonly Symbol Begin = Ize("begin");

        public static readonly Symbol Ok = Ize("ok");

        public static readonly Symbol Macro = Ize("macro");

        public static readonly Symbol Quote = Ize("quote");
        public static readonly Symbol Quasiquote = Ize("quasiquote");
        public static readonly Symbol Unquote = Ize("unquote");
        public static readonly Symbol UnquoteSplicing = Ize("unquote-splicing");

        public static readonly Symbol Ellipsis = Ize("...");
        public static readonly Symbol Underscore = Ize("_");

        public static readonly Symbol Error = Ize("error");
        public static readonly Symbol Undefined = Ize("undefined");

        #endregion
    }
}

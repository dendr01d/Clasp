using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clasp
{
    internal class Symbol : Atom
    {
        protected static readonly Dictionary<string, Symbol> _internment = new Dictionary<string, Symbol>();

        public readonly string Name;

        private Symbol(string name)
        {
            Name = name;
        }

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

        public static Symbol GenSym(string name)
        {
            string target = name.Trim();
            uint counter = 0;

            while (_internment.ContainsKey(target) || string.IsNullOrWhiteSpace(name))
            {
                ++counter;
                target = $"{name}${counter}";
            }

            return Ize(target);
        }

        public static Symbol GenSym() => GenSym(string.Empty);

        public static Symbol GenSym(Symbol sym) => GenSym(sym.Name);

        #region Standard Symbols

        public static readonly Symbol Lambda = Ize("lambda");
        public static readonly Symbol Mu = Ize("mu");

        public static readonly Symbol Begin = Ize("begin");

        public static readonly Symbol Ok = Ize("ok");

        public static readonly Symbol Macro = Ize("macro");

        public static readonly Symbol Quote = Ize("quote");
        public static readonly Symbol Syntax = Ize("syntax");
        public static readonly Symbol Quasiquote = Ize("quasiquote");
        public static readonly Symbol Unquote = Ize("unquote");
        public static readonly Symbol UnquoteSplicing = Ize("unquote-splicing");

        public static readonly Symbol Ellipsis = Ize("...");
        public static readonly Symbol Underscore = Ize("_");

        public static readonly Symbol Error = Ize("error");
        public static readonly Symbol Undefined = Ize("undefined");

        #endregion

        public override string Write() => Name;
        public override string Display() => Name;
    }
}

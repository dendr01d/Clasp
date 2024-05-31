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
        public static Symbol New(string name)
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

        public override string ToString() => Name;

        #region Standard Symbols

        public static readonly Symbol Lambda = New("lambda");
        public static readonly Symbol If = New("if");
        public static readonly Symbol Cond = New("cond");
        public static readonly Symbol Begin = New("begin");
        public static readonly Symbol Eq = New("eq?");
        public static readonly Symbol Case = New("case");

        public static readonly Symbol And = New("and");
        public static readonly Symbol Or = New("or");

        public static readonly Symbol Ok = New("ok");
        public static readonly Symbol CondElse = New("else");
        //public static readonly Symbol RestArgs = New("&rest");

        #endregion
    }

    internal class GenSym : Symbol
    {
        private static int counter = 0;

        public GenSym() : base($"${++counter}") { }
    }
}

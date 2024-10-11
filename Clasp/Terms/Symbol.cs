using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal abstract class Symbol : Atom
    {
        protected static readonly Dictionary<string, Symbol> _internment = new Dictionary<string, Symbol>();
        
        public abstract string Name { get; }

        #region Static Constructors

        public static Symbol Ize(string name)
        {
            if (_internment.TryGetValue(name, out Symbol? interred))
            {
                return interred;
            }
            else
            {
                Symbol output = new StdSymbol(name);
                _internment[name] = output;
                return output;
            }
        }

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

        public override Expression Mark(params int[] marks) => Identifier.Wrap(this).Mark(marks);
        public override Expression Subst(Identifier id, Symbol sym) => Identifier.Wrap(this).Subst(id, sym);
    }

    internal class StdSymbol : Symbol
    {
        private string _name;

        public override string Name => _name;

        public StdSymbol(string name)
        {
            _name = name;
        }
    }

    internal class GenSym : Symbol
    {
        private static uint _globalCounter = 0;

        private string _name;
        private uint _id;

        public override string Name => FormatName(_name, _id);

        public GenSym(string name)
        {
            string target = name;
            uint counter = 0;

            while (_internment.ContainsKey(target))
            {
                target = FormatName(name, ++counter);
            }

            _name = target;
            _id = counter;            
        }

        public GenSym(Symbol mimic) : this(mimic.Name) { }

        private static string FormatName(string name, uint id) => $"{name}_${id}";
    }

    internal sealed class Identifier : Symbol
    {
        public readonly Symbol SymbolicName;
        public Symbol BindingName { get; private set; }

        private HashSet<int> _marks;

        public override string Name => BindingName.Name;

        private Identifier(Symbol sym)
        {
            SymbolicName = sym;
            BindingName = SymbolicName;
            _marks = new HashSet<int>();
        }

        public static Identifier Wrap(Symbol sym)
        {
            return sym switch
            {
                Identifier id => id,
                _ => new Identifier(sym)
            };
        }

        public override Expression Mark(params int[] marks)
        {
            _marks.SymmetricExceptWith(marks);
            return this;
        }

        public override HashSet<int> GetMarks() => _marks;

        public override Expression Subst(Identifier id, Symbol sym)
        {
            if (Pred_BoundIdEq(id))
            {
                BindingName = sym;
            }
            return this;
        }

        public override Expression Resolve() => BindingName;
        public override Expression Strip() => SymbolicName;

        public override string Print()
        {
            return Pred_Eq(SymbolicName, BindingName)
                ? SymbolicName.Serialize()
                : string.Format("<{0}, {1}, {{{2}}}>",
                    SymbolicName,
                    BindingName,
                    string.Join(", ", _marks));
        }
    }
}

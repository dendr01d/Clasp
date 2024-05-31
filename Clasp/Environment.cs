namespace Clasp
{
    internal abstract class Environment
    {
        protected readonly Dictionary<string, Expression> _bindings;
        public abstract Environment GlobalContext { get; }

        protected Environment()
        {
            _bindings = new Dictionary<string, Expression>();
        }

        #region Dictionary Access

        public abstract Expression LookUp(Symbol sym);
        public abstract void SetBang(Symbol sym, Expression def);
        public void Define(Symbol sym, Expression def)
        {
            if (_bindings.ContainsKey(sym.Name))
            {
                throw new DuplicateBindingException(sym);
            }
            else
            {
                _bindings[sym.Name] = def;
            }
        }

        public Environment DefineMany(Pair keys, Pair values)
        {
            return BindSeries(keys, values);
        }

        private Environment BindSeries(Expression keys, Expression values)
        {
            if (keys.IsNil && values.IsNil)
            {
                return this;
            }
            else if (keys.IsAtom && !values.IsAtom)
            {
                //if the keys list ends in a dotted pair, the last arg
                //encapsulates the remaining values
                Define(keys.Expect<Symbol>(), values);
                return this;
            }
            else if (keys.IsNil || values.IsNil)
            {
                throw new MissingArgumentException("C# DefineMany");
            }
            else
            {
                Define(keys.Car.Expect<Symbol>(), values.Car);
                return BindSeries(keys.Cdr, values.Cdr);
            }
        }

        public abstract int CountBindings();

        #endregion

        public Frame Close()
        {
            return new Frame(this);
        }
    }

    internal class GlobalEnvironment : Environment
    {
        public override Environment GlobalContext => this;

        private GlobalEnvironment() { }

        public override Expression LookUp(Symbol sym)
        {
            if (_bindings.TryGetValue(sym.Name, out Expression? expr))
            {
                return expr;
            }
            else
            {
                throw new MissingBindingException(sym);
            }
        }

        public override void SetBang(Symbol sym, Expression def)
        {
            if (_bindings.ContainsKey(sym.Name))
            {
                _bindings[sym.Name] = def;
            }
            else
            {
                throw new MissingBindingException(sym);
            }
        }

        public override int CountBindings() => _bindings.Count();

        public static Environment Empty()
        {
            return new GlobalEnvironment();
        }

        public static Environment Standard()
        {
            GlobalEnvironment ge = new GlobalEnvironment();
            foreach(var def in PrimitiveProcedure.NativeOps)
            {
                ge.Define(Symbol.New(def.Key), def.Value);
            }

            foreach(var def in CompoundProcedure.DerivedOps)
            {
                ge.Define(Symbol.New(def.Key), Evaluator.Evaluate(Parser.Parse(def.Value), ge));
            }

            return ge.Close();
        }
    }

    internal class Frame : Environment
    {
        private readonly Environment _ancestor;

        public Frame(Environment ancestor) : base()
        {
            _ancestor = ancestor;
        }

        public override Environment GlobalContext => _ancestor.GlobalContext;

        public override Expression LookUp(Symbol sym)
        {
            if (_bindings.TryGetValue(sym.Name, out Expression? expr))
            {
                return expr;
            }
            else
            {
                return _ancestor.LookUp(sym);
            }
        }

        public override void SetBang(Symbol sym, Expression def)
        {
            if (_bindings.ContainsKey(sym.Name))
            {
                _bindings[sym.Name] = def;
            }
            else
            {
                _ancestor.SetBang(sym, def);
            }
        }
        public override int CountBindings() => _bindings.Count() + _ancestor.CountBindings();
    }
}

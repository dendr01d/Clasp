using System;

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
        public abstract void RebindExisting(Symbol sym, Expression def);
        public void BindNew(Symbol sym, Expression def)
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

        public abstract bool HasBound(Symbol sym);
        public bool HasLocal(Symbol sym) => _bindings.ContainsKey(sym.Name);

        public abstract int CountBindings();
        public Frame Close()
        {
            return new Frame(this);
        }

        #endregion
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

        public override void RebindExisting(Symbol sym, Expression def)
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

        public override bool HasBound(Symbol sym) => HasLocal(sym);

        public override int CountBindings() => _bindings.Count();

        public static Environment Empty()
        {
            return new GlobalEnvironment();
        }

        private const string _STD_LIBRARY = @".\StdLibrary.scm";

        public static Environment LoadStandard()
        {
            GlobalEnvironment ge = new GlobalEnvironment();

            foreach(var special in Evaluator.SpecialFormRouting)
            {
                ge.BindNew(Symbol.Ize(special.Key), new SpecialForm(special.Key, special.Value));
            }

            foreach(var def in PrimitiveProcedure.NativeOps)
            {
                ge.BindNew(Symbol.Ize(def.Key), def.Value);
            }

            if (File.Exists(_STD_LIBRARY))
            {
                foreach (Expression expr in Parser.ParseFile(_STD_LIBRARY))
                {
                    Evaluator.Evaluate(expr, ge);
                }
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

        public override void RebindExisting(Symbol sym, Expression def)
        {
            if (_bindings.ContainsKey(sym.Name))
            {
                _bindings[sym.Name] = def;
            }
            else
            {
                _ancestor.RebindExisting(sym, def);
            }
        }

        public override bool HasBound(Symbol sym) => HasLocal(sym) || _ancestor.HasBound(sym);

        public override int CountBindings() => _bindings.Count() + _ancestor.CountBindings();
    }
}

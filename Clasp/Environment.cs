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

        public bool HasBound(Symbol sym) => _bindings.ContainsKey(sym.Name);

        //public Environment DefineMany(Pair keys, Pair values)
        //{
        //    return BindSeries(keys, values);
        //}

        //private Environment BindSeries(Expression keys, Expression values)
        //{
        //    if (keys.IsNil && values.IsNil)
        //    {
        //        return this;
        //    }
        //    else if (keys.IsAtom && !values.IsAtom)
        //    {
        //        //if the keys list ends in a dotted pair, the last arg
        //        //encapsulates the remaining values
        //        Define(keys.Expect<Symbol>(), values);
        //        return this;
        //    }
        //    else if (keys.IsNil || values.IsNil)
        //    {
        //        throw new MissingArgumentException("C# DefineMany");
        //    }
        //    else
        //    {
        //        Define(keys.Car.Expect<Symbol>(), values.Car);
        //        return BindSeries(keys.Cdr, values.Cdr);
        //    }
        //}

        public bool Unifies(Expression form, Expression pattern)
        {
            return Unify(form, pattern, false);
        }

        public abstract int CountBindings();
        public Frame Close()
        {
            return new Frame(this);
        }

        #endregion

        #region Pattern Unification

        private bool Unify(Expression form, Expression pattern, bool trailingContext)
        {
            if (pattern == Symbol.Underscore)
            {
                return true;
            }
            else if (pattern is Symbol sym)
            {
                if (trailingContext)
                {
                    BindOrAppendLast(sym, form);
                    return true;
                }
                else if (HasBound(sym))
                {
                    return Expression.Pred_Equal(this.LookUp(sym), form);
                }
                else
                {
                    Define(sym, form);
                    return true;
                }
            }
            else if (pattern is Pair p)
            {
                if (p.Car == Symbol.Quote && p.Cdr is Pair p2)
                {
                    return Expression.Pred_Eq(p2.Car, form);
                }
                else if (p.Cdr is Pair p3 && p3.Car == Symbol.Ellipsis && p3.Cdr.IsNil)
                {
                    InitializeListBindings(p.Car);
                    return UnifyTrailing(form, p.Car);
                }
                else if (form is Pair f)
                {
                    return Unify(f.Car, p.Car, trailingContext)
                        && Unify(f.Cdr, p.Cdr, trailingContext);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return Expression.Pred_Equal(form, pattern);
            }
        }

        private void InitializeListBindings(Expression pattern)
        {
            if (pattern is Symbol sym)
            {
                Define(sym, Expression.Nil);
            }
            else if (pattern is Pair p)
            {
                InitializeListBindings(p.Car);
                InitializeListBindings(p.Cdr);
            }
        }

        private bool UnifyTrailing(Expression form, Expression pattern)
        {
            return form.IsNil
                ? true
                : Unify(form.Car, pattern, true)
                    && UnifyTrailing(form.Cdr, pattern);
        }

        private void BindOrAppendLast(Symbol key, Expression def)
        {
            SetBang(key, Pair.AppendLast(LookUp(key), def));
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
                ge.Define(Symbol.Ize(def.Key), def.Value);
            }

            foreach(var def in CompoundProcedure.DerivedOps)
            {
                ge.Define(Symbol.Ize(def.Key), Evaluator.Evaluate(Parser.Parse(def.Value), ge));
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

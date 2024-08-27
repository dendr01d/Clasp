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

        /// <summary>
        /// Attempts to unify the two expressions by recursively binding elements of <paramref name="form"/>
        /// to symbols in <paramref name="pattern"/>. Returns true if unification succeeds. May mutate the environment
        /// even when unification does NOT succeed.
        /// </summary>
        public bool TryUnify(Expression form, Expression pattern)
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
            return pattern switch
            {
                Symbol s => UnifyWithSymbol(form, s, trailingContext),
                Pair p => UnifyWithPair(form, p, trailingContext),
                _ => form.Equal(pattern)
            };
        }

        private bool UnifyWithSymbol(Expression form, Symbol pattern, bool trailingContext)
        {
            if (pattern == Symbol.Underscore)
            {
                return true;
            }
            else if (trailingContext)
            {
                RebindExisting(pattern, Pair.AppendLast(LookUp(pattern), form));
                return true;
            }
            else if (HasBound(pattern))
            {
                return LookUp(pattern).Equal(form);
            }
            else
            {
                BindNew(pattern, form);
                return true;
            }
        }

        private bool UnifyWithPair(Expression form, Pair pattern, bool trailingContext)
        {
            if (pattern is Quoted quoted)
            {
                return quoted.TaggedValue.Equal(form);
            }
            else if (pattern is EllipticPattern elliptic)
            {
                InitializeListBindings(elliptic.TaggedValue);
                return UnifyTrailing(form, elliptic.TaggedValue);
            }
            else if (form is Pair f)
            {
                return Unify(f.Car, pattern.Car, trailingContext)
                    && Unify(f.Cdr, pattern.Cdr, trailingContext);
            }

            return false;
        }

        private void InitializeListBindings(Expression pattern)
        {
            if (pattern is Symbol sym && sym != Symbol.Underscore)
            {
                BindNew(sym, Expression.Nil);
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

        public override int CountBindings() => _bindings.Count();

        public static Environment Empty()
        {
            return new GlobalEnvironment();
        }

        private const string _STD_LIBRARY = @".\StdLibrary.scm";

        public static Environment LoadStandard()
        {
            GlobalEnvironment ge = new GlobalEnvironment();
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
        public override int CountBindings() => _bindings.Count() + _ancestor.CountBindings();
    }
}

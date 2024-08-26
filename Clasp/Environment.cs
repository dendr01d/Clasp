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

        public abstract int CountBindings();
        public Frame Close()
        {
            return new Frame(this);
        }

        #endregion

        #region Secret Methods

        /// <summary>
        /// Copy the bindings from <paramref name="enriched"/> to this environment. If bindings already exist,
        /// append the new definitions to the old ones to form a list.
        /// </summary>
        public void SubsumeAndAppend(Environment enriched)
        {
            foreach(var binding in enriched._bindings)
            {
                if (_bindings.TryGetValue(binding.Key, out Expression? extant))
                {
                    _bindings[binding.Key] = Pair.AppendLast(extant, binding.Value);
                }
                else
                {
                    _bindings[binding.Key] = binding.Value;
                }
            }
        }

        /// <summary>
        /// Check to see if all the keys provided are bound to nil
        /// </summary>
        public bool AllKeysExhausted(IEnumerable<Symbol> keys)
        {
            return keys.All(x => _bindings.TryGetValue(x.Name, out Expression? def) && def.IsNil);
        }

        /// <summary>
        /// Attempt to create a new environment where the specified keys are rebound to the cars
        /// of their current binding values. The current environment will be mutated. The method 
        /// fails if any of the keys are not bound to list values.
        public bool TryBumpBindings(IEnumerable<Symbol> keys, out Environment output)
        {
            output = Close();

            foreach(Symbol key in keys)
            {
                if (_bindings.TryGetValue(key.Name, out Expression? def)
                    && def is Pair p)
                {
                    output.BindNew(key, p.Car);
                    _bindings[key.Name] = p.Cdr;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Create a closure of this environment that replicates the bindings of the specified keys.
        /// i.e. create a subset-copy of the environment you can fuck up withour remorse.
        /// </summary>
        public Environment DescendAndCopy(IEnumerable<Symbol> keys)
        {
            Environment output = Close();

            foreach(Symbol key in keys)
            {
                if (_bindings.TryGetValue(key.Name, out Expression? value))
                {
                    output.BindNew(key, value);
                }
            }

            return output;
        }

        #endregion


        #region Pattern Unification

        private bool Unify(Expression form, Expression pattern, bool trailingContext)
        {
            return pattern switch
            {
                Symbol s => UnifyWithSymbol(form, s, trailingContext),
                Pair p => UnifyWithPair(form, p, trailingContext),
                _ => Expression.Pred_Equal(form, pattern)
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
                return Expression.Pred_Equal(LookUp(pattern), form);
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
                return Expression.Pred_Eq(quoted.TaggedValue, form);
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

        public override bool HasBound(Symbol sym) => HasLocal(sym);

        public override int CountBindings() => _bindings.Count();

        public static Environment Empty()
        {
            return new GlobalEnvironment();
        }

        private const string _STD_LIBRARY = @".\StdLibrary.scm";

        public static Environment Standard()
        {
            GlobalEnvironment ge = new GlobalEnvironment();
            foreach(var def in PrimitiveProcedure.NativeOps)
            {
                ge.BindNew(Symbol.Ize(def.Key), def.Value);
            }

            if (File.Exists(_STD_LIBRARY))
            {
                Expression fileContents = Parser.ParseFile(_STD_LIBRARY);

                while (!fileContents.IsNil)
                {
                    Evaluator.Evaluate(fileContents.Car, ge);
                    fileContents = fileContents.Cdr;
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

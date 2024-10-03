using System;
using System.Linq;

namespace Clasp
{
    internal abstract class Environment
    {
        protected readonly Dictionary<string, Expression> _bindings;
        protected readonly Dictionary<string, int> _recurrenceLevels;

        public abstract Environment GlobalContext { get; }

        protected Environment()
        {
            _bindings = new Dictionary<string, Expression>();
            _recurrenceLevels = new Dictionary<string, int>();
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

        public abstract bool Binds(Symbol sym);
        public bool BindsLocally(Symbol sym) => _bindings.ContainsKey(sym.Name);

        public abstract int CountBindings();
        public Frame Enclose()
        {
            return new Frame(this);
        }

        #endregion

        #region Supplementary functions for Syntactic Manipulation

        /// <summary>
        /// Absorb all of the local bindings in <paramref name="subEnv"/> into this environment. It's an error
        /// to subsume a binding that shadows one at this level.
        /// </summary>
        /// <param name="subEnv"></param>
        public void Subsume(Environment subEnv)
        {
            foreach(var binding in subEnv._bindings)
            {
                _bindings.Add(binding.Key, binding.Value);
            }
        }

        //TODO maybe I need a special frame-type environment that enforced its flatness?

        public void SubsumeRecurrent(Environment subEnv)
        {
            foreach(var binding in subEnv._bindings)
            {
                if (_bindings.TryGetValue(binding.Key, out Expression extantValue))
                {
                    _bindings[binding.Key] = Pair.Append(extantValue, Pair.List(binding.Value));
                }
                else
                {
                    _bindings[binding.Key] = Pair.List(binding.Value);

                    if (subEnv._recurrenceLevels.TryGetValue(binding.Key, out int level))
                    {
                        _recurrenceLevels[binding.Key] = level + 1;
                    }
                    else
                    {
                        _recurrenceLevels[binding.Key] = 1;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the environment contains any recurrent bindings that can be further split
        /// </summary>
        public bool MoreRecurrent() => _recurrenceLevels.Keys
            .Select(x => _bindings[x])
            .Any(x => !x.IsNil);

        public Environment SplitRecurrent()
        {
            if (!_recurrenceLevels.Any(x => x.Value > 0))
            {
                throw new UncategorizedException("No recurrent elements present in environment by which to split.");
            }
            else if (_recurrenceLevels.Where(x => x.Value > 0).Any(x => !_bindings[x.Key].IsPair))
            {
                throw new UncategorizedException("Variable list lengths among recurrent elements of environment.");
            }

            Environment subEnv = Enclose();
            foreach(string key in _recurrenceLevels.Keys)
            {
                if (_recurrenceLevels[key] == 0)
                {
                    subEnv._bindings.Add(key, _bindings[key]);
                }
                else
                {
                    subEnv._bindings.Add(key, _bindings[key].Car);
                    subEnv._recurrenceLevels[key] = _recurrenceLevels[key] - 1;

                    _bindings[key] = _bindings[key].Cdr;
                }
            }

            return subEnv;
        }

        #endregion

        //public Environment DeconstructElliptic(Expression patternVars)
        //{
        //    Environment output = Enclose();

        //}
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

        public override bool Binds(Symbol sym) => BindsLocally(sym);

        public override int CountBindings() => _bindings.Count();

        public static Environment Empty()
        {
            return new GlobalEnvironment();
        }

        private const string _STD_LIBRARY = @"C:\Users\Duncan\source\repos\Clasp\Code\StdLibrary.scm";

        public static Environment LoadStandard()
        {
            GlobalEnvironment ge = new GlobalEnvironment();

            List<Exception> errors = new List<Exception>();

            ge.PopulateSpecialForms();
            ge.PopulatePrimitiveProcs();

            if (File.Exists(_STD_LIBRARY))
            {
                try
                {
                    IEnumerable<Expression> exprs = Parser.ParseFile(_STD_LIBRARY);

                    foreach (Expression expr in exprs)
                    {
                        try
                        {
                            Evaluator.SilentEval(expr, ge);
                        }
                        catch (Exception ex)
                        {
                            string msg = $"Error evaluating entry from standard library: {ex.Message}";
                            errors.Add(new Exception(msg, ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = $"Error reading standard library: {ex.Message}";
                    errors.Add(new Exception(msg, ex));
                }
            }

            if (errors.Any())
            {
                throw new AggregateException(errors);
            }

            return ge.Enclose();
        }
    }

    internal class Frame : Environment
    {
        private readonly Environment _ancestor;

        public Environment Enclosing => _ancestor;

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

        public override bool Binds(Symbol sym) => BindsLocally(sym) || _ancestor.Binds(sym);

        public override int CountBindings() => _bindings.Count() + _ancestor.CountBindings();
    }


    internal static class EnvironmentDefaults
    {
        public static void PopulateSpecialForms(this GlobalEnvironment ge)
        {
            foreach(var def in _specialForms)
            {
                SpecialForm.Manifest(ge, def.Key, def.Value);
            }
        }

        private static readonly Dictionary<string, Evaluator2.Label> _specialForms = new()
        {
            { "eval", Evaluator2.Label.Apply_Eval },
            { "apply", Evaluator2.Label.Apply_Apply },

            { "quote", Evaluator2.Label.Eval_Quote },
            { "quasiquote", Evaluator2.Label.Eval_Quasiquote },
            { "lambda", Evaluator2.Label.Eval_Lambda },

            { "begin", Evaluator2.Label.Eval_Begin },
            { "if", Evaluator2.Label.Eval_If },

            { "define", Evaluator2.Label.Apply_Define },
            { "set!", Evaluator2.Label.Apply_Set },
            { "define-syntax", Evaluator2.Label.Apply_DefineSyntax },
            { "set-car!", Evaluator2.Label.Set_Car },
            { "set-cdr!", Evaluator2.Label.Set_Cdr },
        };

        public static void PopulatePrimitiveProcs(this GlobalEnvironment ge)
        {
            foreach(var def in _primProcs)
            {
                PrimitiveProcedure.Manifest(ge, def.Key, def.Value);
            }
        }

        private static readonly Dictionary<string, Func<Expression, Expression>> _primProcs = new()
        {
            { "gensym", x => x.IsNil ? Symbol.Generate() : Symbol.Generate(x.Cadr.Expect<Symbol>()) },

            { "cons", x => Pair.Cons(x.Car, x.Cadr) },
            { "car", x => x.Caar },
            { "cdr", x => x.Cdar },

            { "eq?", x => Pair.Enumerate(x).PairwiseSelect(Expression.Pred_Eq).AllTrue() },
            { "eqv?", x => Pair.Enumerate(x).PairwiseSelect(Expression.Pred_Eqv).AllTrue() },
            { "equal?", x => Pair.Enumerate(x).PairwiseSelect(Expression.Pred_Equal).AllTrue() },

            #region Numerical Operations
            
            { "=", x => Pair.Enumerate<SimpleNum>(x).PairwiseSelect(SimpleNum.NumEquals).AllTrue() },
            { "<", x => Pair.Enumerate<SimpleNum>(x).PairwiseSelect(SimpleNum.LessThan).AllTrue() },
            { ">", x => Pair.Enumerate<SimpleNum>(x).PairwiseSelect(SimpleNum.GreatherThan).AllTrue() },
            { "<=", x => Pair.Enumerate<SimpleNum>(x).PairwiseSelect(SimpleNum.Leq).AllTrue() },
            { ">=", x => Pair.Enumerate<SimpleNum>(x).PairwiseSelect(SimpleNum.Geq).AllTrue() },

            { "+", x => Pair.Enumerate<SimpleNum>(x).Aggregate(SimpleNum.Add) },
            { "-", x => x.Cdr.IsNil
                ? SimpleNum.Negate(x.Car.Expect<SimpleNum>())
                : SimpleNum.Subtract(x.Car.Expect<SimpleNum>(), Pair.Enumerate<SimpleNum>(x.Cdr).Aggregate(SimpleNum.Add)) },
            { "*", x => Pair.Enumerate<SimpleNum>(x).Aggregate(SimpleNum.Multiply) },
            { "/", x => x.Cdr.IsNil
                ? SimpleNum.Divide(SimpleNum.One, x.Car.Expect<SimpleNum>())
                : SimpleNum.Divide(x.Car.Expect<SimpleNum>(), Pair.Enumerate<SimpleNum>(x.Cdr).Aggregate(SimpleNum.Multiply)) },

            { "quotient", x => SimpleNum.Calculate(x.Car, x.Cadr, SimpleNum.Quotient) },
            { "remainder", x => SimpleNum.Calculate(x.Car, x.Cadr, SimpleNum.Remainder) },
            { "modulo", x => SimpleNum.Calculate(x.Car, x.Cadr, SimpleNum.Modulo) },

            { "floor", x => SimpleNum.Floor(x.Car.Expect<SimpleNum>()) },
            { "ceiling", x => SimpleNum.Ceiling(x.Car.Expect<SimpleNum>()) },
            { "truncate", x => SimpleNum.Truncate(x.Car.Expect<SimpleNum>()) },

            #endregion

            #region Type-checking
            
            { "atom?", x => x.IsAtom },
            { "null?", x => x.IsNil },
            { "list?", x => x.IsList },
            { "pair?", x => x.IsPair },
            { "symbol?", x => x is Symbol },
            { "procedure?", x => x is Procedure },
            { "boolean?", x => x is Boolean },
            { "number?", x => x is SimpleNum },
            { "vector?", x => x is Vector }

            #endregion

        };
    }
}

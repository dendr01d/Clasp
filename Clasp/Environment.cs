using System;
using System.Linq;

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

        internal bool ContainsKey(string name) => _bindings.ContainsKey(name);

        public abstract int CountBindings();
        public Frame Enclose()
        {
            return new Frame(this);
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

        public override bool HasBound(Symbol sym) => HasLocal(sym);

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

        public override bool HasBound(Symbol sym) => HasLocal(sym) || _ancestor.HasBound(sym);

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

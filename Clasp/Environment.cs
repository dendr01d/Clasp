using System;
using System.Linq;
using Clasp.ExtensionMethods;

namespace Clasp
{
    internal class Environment
    {
        private readonly Dictionary<string, Expression> _bindings;
        private readonly Environment? _next;

        public Environment()
        {
            _bindings = new Dictionary<string, Expression>();
            _next = null;
        }

        public Environment(Environment ancestor) : this()
        {
            _next = ancestor;
        }

        #region Dictionary Access

        public Expression LookUp(Symbol sym)
        {
            if (_bindings.TryGetValue(sym.Name, out Expression? result))
            {
                return result;
            }
            else if (_next is null)
            {
                throw new MissingBindingException(sym);
            }
            else
            {
                return _next.LookUp(sym);
            }
        }

        public void Bind(Symbol sym, Expression def)
        {
            _bindings[sym.Name] = def;
        }

        public void BindArgs(Expression parameters, List<Expression> values)
        {
            int index = 0;
            while (!parameters.IsAtom)
            {
                Bind(parameters.Car.Expect<Symbol>(), values[index++]);
                parameters = parameters.Cdr;
            }

            if (parameters is Symbol sym)
            {
                Bind(sym, Pair.List(values.Skip(index).ToArray()));
            }
        }

        private bool FindContext(Symbol sym, out Environment? context)
        {
            if (_bindings.ContainsKey(sym.Name))
            {
                context = this;
                return true;
            }
            else if (_next is null)
            {
                context = null;
                return false;
            }
            else
            {
                return _next.FindContext(sym, out context);
            }
        }

        public bool Binds(Symbol sym) => FindContext(sym, out _);

        public bool BindsLocally(Symbol sym) => _bindings.ContainsKey(sym.Name);

        public int CountBindings() => _bindings.Count + (_next?.CountBindings() ?? 0);

        #endregion

        #region Supplementary functions for Syntactic Manipulation

        /// <summary>
        /// Absorb all of the local bindings in <paramref name="subEnv"/> into this environment. It's an error
        /// to subsume a binding that shadows one at this level.
        /// </summary>
        /// <param name="subEnv"></param>
        //public void Subsume(Environment subEnv)
        //{
        //    foreach(var binding in subEnv._bindings)
        //    {
        //        _bindings.Add(binding.Key, binding.Value);
        //    }
        //}

        //public virtual void SubsumeRecurrent(ExpansionFrame subEnv) { }

        //public virtual bool MoreRecurrent() => false;

        //public virtual ExpansionFrame SplitRecurrent() => new ExpansionFrame(this);

        #endregion
    }

    internal static class EnvironmentDefaults
    {
        //public static void PopulateSpecialForms(this Environment ge)
        //{
        //    foreach(var def in _specialForms)
        //    {
        //        SpecialForm.Manifest(ge, def.Key, def.Value);
        //    }
        //}

        //private static readonly Dictionary<string, Evaluator.Label> _specialForms = new()
        //{
        //    { "eval", Evaluator.Label.Apply_Eval },
        //    { "apply", Evaluator.Label.Apply_Apply },

        //    { "quote", Evaluator.Label.Eval_Quote },
        //    { "syntax", Evaluator.Label.Eval_Syntax },
        //    { "quasiquote", Evaluator.Label.Eval_Quasiquote },
        //    { "lambda", Evaluator.Label.Eval_Lambda },

        //    { "begin", Evaluator.Label.Eval_Begin },
        //    { "if", Evaluator.Label.Eval_If },

        //    { "define", Evaluator.Label.Apply_Define },
        //    { "set!", Evaluator.Label.Apply_Set },
        //    { "define-syntax", Evaluator.Label.Apply_DefineSyntax },
        //    { "set-car!", Evaluator.Label.Set_Car },
        //    { "set-cdr!", Evaluator.Label.Set_Cdr },
        //};

        public static void PopulatePrimitiveProcs(this Environment ge)
        {
            foreach(var def in _primProcs)
            {
                PrimitiveProcedure.Manifest(ge, def.Key, def.Value);
            }
        }

        private static readonly Dictionary<string, Func<Expression, Expression>> _primProcs = new()
        {
            { "gensym", x => x.IsNil ? Symbol.GenSym() : Symbol.GenSym(x.Cadr.Expect<Symbol>()) },

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
            { "pair?", x => x.IsPair },
            { "symbol?", x => x is Symbol },
            { "procedure?", x => x is Procedure },
            { "boolean?", x => x is Boolean },
            { "number?", x => x is SimpleNum },
            //{ "vector?", x => x is Vector }

            #endregion

        };
    }
}

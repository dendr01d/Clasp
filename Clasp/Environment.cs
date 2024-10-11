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
        public virtual Binding LookupBindingType(Symbol sym) => Binding.NA;
        public virtual int LookupRecurrenceLevel(Symbol sym) => -1;

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
        public virtual void BindNew(Symbol sym, Expression def, Binding bType) => BindNew(sym, def);

        public abstract bool Binds(Symbol sym);
        public bool BindsLocally(Symbol sym) => _bindings.ContainsKey(sym.Name);
        public virtual bool BindsAs(Symbol sym, Binding bType) => Binds(sym);

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

        public virtual void SubsumeRecurrent(ExpansionFrame subEnv) { }

        public virtual bool MoreRecurrent() => false;

        public virtual ExpansionFrame SplitRecurrent() => new ExpansionFrame(this);

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

            foreach(var special in Evaluator.SpecialFormRouting)
            {
                try
                {
                    ge.BindNew(Symbol.Ize(special.Key), new SpecialForm(special.Key, special.Value));

                }
                catch (Exception ex)
                {
                    string msg = $"Error defining special form '{special.Key}': {ex.Message}";
                    errors.Add(new Exception(msg, ex));
                }
            }

            foreach(var def in PrimitiveProcedure.NativeOps)
            {
                try
                {
                    ge.BindNew(Symbol.Ize(def.Key), def.Value);
                }
                catch (Exception ex)
                {
                    string msg = $"Error defining simple procedure '{def.Key}': {ex.Message}";
                    errors.Add(new Exception(msg, ex));
                }
            }

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
                            string msg = $"Error evaluating entry from standard library {{{expr.ToPrinted()}}}: {ex.Message}";
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
        protected readonly Environment _ancestor;

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

    internal enum Binding
    {
        NA,
        Variable, PatternVariable, Transformer,
        SpecialQuote, SpecialLambda
    }

    internal class ExpansionFrame : Frame
    {
        private readonly Dictionary<string, Binding> _bindingTypes;
        private readonly Dictionary<string, int> _recurrenceLevels;

        public ExpansionFrame(Environment pred) : base(pred)
        {
            _bindingTypes = new Dictionary<string, Binding>();
            _recurrenceLevels = new Dictionary<string, int>();
        }

        public override Binding LookupBindingType(Symbol sym)
        {
            if (_bindingTypes.TryGetValue(sym.Name, out Binding b))
            {
                return b;
            }
            else
            {
                return _ancestor.LookupBindingType(sym);
            }
        }

        public override int LookupRecurrenceLevel(Symbol sym)
        {
            if (_recurrenceLevels.TryGetValue(sym.Name, out int level))
            {
                return level;
            }
            else
            {
                return _ancestor.LookupRecurrenceLevel(sym);
            }
        }

        public override void BindNew(Symbol sym, Expression def, Binding bType)
        {
            BindNew(sym, def);
            _bindingTypes[sym.Name] = bType;
        }

        public override bool BindsAs(Symbol sym, Binding bType)
        {
            return Binds(sym)
                && _bindingTypes.TryGetValue(sym.Name, out Binding b)
                && bType == b;
        }

        public override void SubsumeRecurrent(ExpansionFrame subEnv)
        {
            foreach (var binding in subEnv._bindings)
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
        public override bool MoreRecurrent() => _recurrenceLevels.Keys
            .Select(x => _bindings[x])
            .Any(x => !x.IsNil);

        public override ExpansionFrame SplitRecurrent()
        {
            if (!_recurrenceLevels.Any(x => x.Value > 0))
            {
                throw new UncategorizedException("No recurrent elements present in environment by which to split.");
            }
            else if (_recurrenceLevels.Where(x => x.Value > 0).Any(x => !_bindings[x.Key].IsPair))
            {
                throw new UncategorizedException("Variable list lengths among recurrent elements of environment.");
            }

            ExpansionFrame subEnv = new ExpansionFrame(this); //enclose
            foreach (string key in _recurrenceLevels.Keys)
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
    }
}

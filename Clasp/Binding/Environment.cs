using System;
using System.Linq;
using Clasp.ExtensionMethods;

namespace Clasp.Binding
{
    internal class Environment
    {
        public readonly string Name;

        //private readonly Dictionary<string, Expression> _bindings;
        //private readonly Environment? _next;

        //public Environment()
        //{
        //    _bindings = new Dictionary<string, Expression>();
        //    _next = null;
        //}

        //public Environment(Environment ancestor) : this()
        //{
        //    _next = ancestor;
        //}

        //#region Dictionary Access

        //public Expression LookUp(Symbol sym)
        //{
        //    if (_bindings.TryGetValue(sym.Name, out Expression? result))
        //    {
        //        return result;
        //    }
        //    else if (_next is null)
        //    {
        //        throw new MissingBindingException(sym);
        //    }
        //    else
        //    {
        //        return _next.LookUp(sym);
        //    }
        //}

        //public void Bind(Symbol sym, Expression def)
        //{
        //    _bindings[sym.Name] = def;
        //}

        //public void BindArgs(Expression parameters, List<Expression> values)
        //{
        //    int index = 0;
        //    while (!parameters.IsAtom)
        //    {
        //        Bind(parameters.Car.Expect<Symbol>(), values[index++]);
        //        parameters = parameters.Cdr;
        //    }

        //    if (parameters is Symbol sym)
        //    {
        //        Bind(sym, Pair.List(values.Skip(index).ToArray()));
        //    }
        //}

        //private bool FindContext(Symbol sym, out Environment? context)
        //{
        //    if (_bindings.ContainsKey(sym.Name))
        //    {
        //        context = this;
        //        return true;
        //    }
        //    else if (_next is null)
        //    {
        //        context = null;
        //        return false;
        //    }
        //    else
        //    {
        //        return _next.FindContext(sym, out context);
        //    }
        //}

        //public bool Binds(Symbol sym) => FindContext(sym, out _);

        //public bool BindsLocally(Symbol sym) => _bindings.ContainsKey(sym.Name);

        //public int CountBindings() => _bindings.Count + (_next?.CountBindings() ?? 0);

        //#endregion

        //#region Supplementary functions for Syntactic Manipulation

        ///// <summary>
        ///// Absorb all of the local bindings in <paramref name="subEnv"/> into this environment. It's an error
        ///// to subsume a binding that shadows one at this level.
        ///// </summary>
        ///// <param name="subEnv"></param>
        ////public void Subsume(Environment subEnv)
        ////{
        ////    foreach(var binding in subEnv._bindings)
        ////    {
        ////        _bindings.Add(binding.Key, binding.Value);
        ////    }
        ////}

        ////public virtual void SubsumeRecurrent(ExpansionFrame subEnv) { }

        ////public virtual bool MoreRecurrent() => false;

        ////public virtual ExpansionFrame SplitRecurrent() => new ExpansionFrame(this);

        //#endregion
    }
}

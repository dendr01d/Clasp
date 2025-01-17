using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Data.Terms;

namespace Clasp.Binding
{
    // how much do I REALLY need this to implement IDictionary?
    internal abstract class Environment : IDictionary<string, Term>
    {
        protected readonly Dictionary<string, Term> _mutableBindings;
        public readonly int Depth;

        public abstract GlobalEnvironment TopLevel { get; }
        public abstract bool IsTopLevel { get; }

        protected Environment(int depth)
        {
            _mutableBindings = new Dictionary<string, Term>();
            Depth = depth;
        }

        #region Utilities

        public abstract Term LookUp(string name);

        protected abstract IEnumerable<Environment> EnumerateScope();

        private IEnumerable<KeyValuePair<string, Term>> EnumerateAccessibleBindings()
        {
            return EnumerateScope()
                .SelectMany(x => x._mutableBindings)
                .DistinctBy(x => x.Key);
        }

        //public Environment ExtractCompileTimeEnv()
        //{
        //    Environment output = new Environment(EnumerateAccessibleBindings()
        //        .Where(x => x.Value is Variable || x.Value is Fixed));

        //    output.Add(Symbol.Lambda.Name, Symbol.Lambda);
        //    output.Add(Symbol.Define.Name, Symbol.Define);
        //    output.Add(Symbol.DefineSyntax.Name, Symbol.DefineSyntax);
        //    output.Add(Symbol.Quote.Name, Symbol.Quote);
        //    output.Add(Symbol.Syntax.Name, Symbol.Syntax);

        //    return output;
        //}

        #endregion

        #region IDictionary Implementation

        public Term this[string key]
        {
            get => LookUp(key);
            set => _mutableBindings[key] = value;
        }

        public virtual ICollection<string> Keys => EnumerateAccessibleBindings().Select(x => x.Key).ToList();
        public virtual ICollection<Term> Values => EnumerateAccessibleBindings().Select(x => x.Value).ToList();

        public int Count => EnumerateAccessibleBindings().Count();

        public bool IsReadOnly => false;

        public void Add(string key, Term value) => _mutableBindings.Add(key, value);
        public void Add(KeyValuePair<string, Term> item) => _mutableBindings.Add(item.Key, item.Value);

        public void Clear() => _mutableBindings.Clear();

        public bool Contains(KeyValuePair<string, Term> item) => TryGetValue(item.Key, out Term? value) ? item.Value == value : false;

        public abstract bool ContainsKey(string key);

        public void CopyTo(KeyValuePair<string, Term>[] array, int arrayIndex)
        {
            foreach (var kvp in EnumerateAccessibleBindings())
            {
                array[arrayIndex] = kvp;
                ++arrayIndex;
            }
        }

        public bool Remove(string key) => _mutableBindings.Remove(key);
        public bool Remove(KeyValuePair<string, Term> item)
        {
            if (_mutableBindings.TryGetValue(item.Key, out Term? value) && value == item.Value)
            {
                _mutableBindings.Remove(item.Key);
                return true;
            }
            return false;
        }

        public abstract bool TryGetValue(string key, [MaybeNullWhen(false)] out Term value);

        public IEnumerator<KeyValuePair<string, Term>> GetEnumerator() => EnumerateAccessibleBindings().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => EnumerateAccessibleBindings().GetEnumerator();

        #endregion
    }

    internal sealed class GlobalEnvironment : Environment
    {
        private readonly Dictionary<string, Term> _staticBindings;

        public override GlobalEnvironment TopLevel => this;
        public override bool IsTopLevel => true;

        public GlobalEnvironment() : base(0)
        {
            _staticBindings = new Dictionary<string, Term>();
        }

        protected override IEnumerable<Environment> EnumerateScope()
        {
            yield return this;
            yield break;
        }

        public void DefineInitial(string key, Term value)
        {
            _staticBindings.Add(key, value);
        }

        public void DefineCoreForm(Symbol sym) => DefineInitial(sym.Name, sym);

        public override Term LookUp(string name)
        {
            if (_mutableBindings.TryGetValue(name, out Term? result1))
            {
                return result1;
            }
            else if (_staticBindings.TryGetValue(name, out Term? result2))
            {
                return result2;
            }
            else
            {
                throw new MissingBindingException(name);
            }
        }

        public override bool ContainsKey(string key) => _mutableBindings.ContainsKey(key) || _staticBindings.ContainsKey(key);
        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out Term value)
        {
            if (_mutableBindings.TryGetValue(key, out Term? mutableValue))
            {
                value = mutableValue;
                return true;
            }
            else if (_staticBindings.TryGetValue(key, out Term? staticValue))
            {
                value = staticValue;
                return true;
            }

            value = null;
            return false;
        }
    }

    internal class EnvFrame : Environment
    {
        protected readonly Environment _next;
        public override GlobalEnvironment TopLevel { get; }
        public override bool IsTopLevel => false;

        public EnvFrame(Environment ancestor) : base(ancestor.Depth + 1)
        {
            _next = ancestor;
            TopLevel = ancestor.TopLevel;
        }

        protected override IEnumerable<Environment> EnumerateScope()
        {
            Environment? current = this;
            while (current is EnvFrame closure)
            {
                yield return closure;
                current = closure._next;
            }

            yield return current; // the global environment
            yield break;
        }

        public override Term LookUp(string name)
        {
            if (_mutableBindings.TryGetValue(name, out Term? result))
            {
                return result;
            }
            else
            {
                return _next.LookUp(name);
            }
        }

        public override bool ContainsKey(string key) => _mutableBindings.ContainsKey(key) || _next.ContainsKey(key);
        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out Term value)
        {
            return _mutableBindings.TryGetValue(key, out value)
                || _next.TryGetValue(key, out value);
        }
    }

    //internal class ExpansionEnv : EnvFrame
    //{
    //    private static readonly Symbol _variableMarker = new GenSym("variable");

    //    public ExpansionEnv(Environment ancestor) : base(ancestor) { }


    //}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;
using Clasp.ExtensionMethods;
using Clasp.Interfaces;

using System.Collections.Immutable;

namespace Clasp.Binding
{
    internal class Environment : IDictionary<string, Term>
    {
        private readonly Dictionary<string, Term> _bindings;
        private readonly Environment? _next;
        public int Depth { get; private init; }
        public bool IsTopLevel => _next is null;

        public Environment()
        {
            _bindings = new Dictionary<string, Term>();
            _next = null;
            Depth = 0;
        }

        public Environment(Environment ancestor) : this()
        {
            _next = ancestor;
            Depth = ancestor.Depth + 1;
        }

        public Environment(IEnumerable<KeyValuePair<string, Term>> extandBindings) : this()
        {
            _bindings = new Dictionary<string, Term>(extandBindings);
        }

        #region Utilities

        private Term LookUp(string name)
        {
            if (_bindings.TryGetValue(name, out Term? result))
            {
                return result;
            }
            else if (_next is null)
            {
                throw new MissingBindingException(name);
            }
            else
            {
                return _next.LookUp(name);
            }
        }

        private IEnumerable<Environment> EnumerateScope()
        {
            Environment? current = this;
            while (current is not null)
            {
                yield return current;
                current = current._next;
            }
            yield break;
        }

        private IEnumerable<KeyValuePair<string, Term>> EnumerateAccessibleBindings()
        {
            return EnumerateScope()
                .SelectMany(x => x._bindings)
                .DistinctBy(x => x.Key);
        }

        public bool Binds(IBindable key) => ContainsKey(key.Name);

        public bool BindsLocally(IBindable key) => _bindings.ContainsKey(key.Name);

        public bool BindsAtTopLevel(IBindable key) => (_next is null && Binds(key)) || (_next is not null && _next.BindsAtTopLevel(key));

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
            set => _bindings[key] = value;
        }

        public ICollection<string> Keys => EnumerateAccessibleBindings().Select(x => x.Key).ToList();
        public ICollection<Term> Values => EnumerateAccessibleBindings().Select(x => x.Value).ToList();

        public int Count => EnumerateAccessibleBindings().Count();

        public bool IsReadOnly => false;

        public void Add(string key, Term value) => _bindings.Add(key, value);
        public void Add(KeyValuePair<string, Term> item) => _bindings.Add(item.Key, item.Value);

        public void Clear() => _bindings.Clear();

        public bool Contains(KeyValuePair<string, Term> item) => TryGetValue(item.Key, out Term? value) ? item.Value == value : false;

        public bool ContainsKey(string key) => _bindings.ContainsKey(key) || (_next is not null && _next.ContainsKey(key));

        public void CopyTo(KeyValuePair<string, Term>[] array, int arrayIndex)
        {
            foreach(var kvp in EnumerateAccessibleBindings())
            {
                array[arrayIndex] = kvp;
                ++arrayIndex;
            }
        }

        public bool Remove(string key) => _bindings.Remove(key);
        public bool Remove(KeyValuePair<string, Term> item)
        {
            if (_bindings.TryGetValue(item.Key, out Term? value) && value == item.Value)
            {
                _bindings.Remove(item.Key);
                return true;
            }
            return false;
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out Term value)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, Term>> GetEnumerator() => EnumerateAccessibleBindings().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => EnumerateAccessibleBindings().GetEnumerator();

        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Clasp.Data.AbstractSyntax;
using Clasp.Data.Terms;

namespace Clasp.Binding
{
    internal class CompileTimeEnvironment : IDictionary<string, Term>
    {
        private Environment _enclosingEnv;
        //private Dictionary<string, Macro> _boundMacros;
        //private Dictionary<string, Variable> _boundVars;

        //public CompileTimeEnvironment(Environment enclosing)
        //{
        //    _enclosingEnv = enclosing.ExtractCompileTimeEnv();
        //}

        public string CreateFreshName(string initialName)
        {
            string newName = initialName;
            int counter = 1;

            while (_enclosingEnv.ContainsKey(newName))
            {
                newName = string.Format("{0}_{1}", initialName, counter++);
            }

            return newName;
        }

        #region (Automatic) IDictionary Implementation

        public Term this[string key] { get => ((IDictionary<string, Term>)_enclosingEnv)[key]; set => ((IDictionary<string, Term>)_enclosingEnv)[key] = value; }

        public ICollection<string> Keys => ((IDictionary<string, Term>)_enclosingEnv).Keys;

        public ICollection<Term> Values => ((IDictionary<string, Term>)_enclosingEnv).Values;

        public int Count => ((ICollection<KeyValuePair<string, Term>>)_enclosingEnv).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, Term>>)_enclosingEnv).IsReadOnly;

        public void Add(string key, Term value)
        {
            ((IDictionary<string, Term>)_enclosingEnv).Add(key, value);
        }

        public void Add(KeyValuePair<string, Term> item)
        {
            ((ICollection<KeyValuePair<string, Term>>)_enclosingEnv).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, Term>>)_enclosingEnv).Clear();
        }

        public bool Contains(KeyValuePair<string, Term> item)
        {
            return ((ICollection<KeyValuePair<string, Term>>)_enclosingEnv).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, Term>)_enclosingEnv).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, Term>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, Term>>)_enclosingEnv).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, Term>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, Term>>)_enclosingEnv).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, Term>)_enclosingEnv).Remove(key);
        }

        public bool Remove(KeyValuePair<string, Term> item)
        {
            return ((ICollection<KeyValuePair<string, Term>>)_enclosingEnv).Remove(item);
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out Term? value)
        {
            return ((IDictionary<string, Term>)_enclosingEnv).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_enclosingEnv).GetEnumerator();
        }

        #endregion
    }
}

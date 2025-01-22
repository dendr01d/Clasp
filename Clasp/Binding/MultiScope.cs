using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Binding
{
    internal class MultiScope : IDictionary<uint, Scope>
    {
        private readonly Dictionary<uint, Scope> _phasedScopes;

        public MultiScope()
        {
            _phasedScopes = new Dictionary<uint, Scope>();
        }

        private Scope LazilyGet(uint phase)
        {
            if (!_phasedScopes.ContainsKey(phase))
            {
                _phasedScopes[phase] = new Scope();
            }
            return _phasedScopes[phase];
        }

        #region IDictionary Implementation

        public Scope this[uint key]
        {
            get => LazilyGet(key);
            set { _phasedScopes[key] = value; }
        }

        public ICollection<uint> Keys => _phasedScopes.Keys;
        public ICollection<Scope> Values => _phasedScopes.Values;
        public int Count => _phasedScopes.Count;
        public bool IsReadOnly => false;

        public void Add(uint key, Scope value) => _phasedScopes.Add(key, value);
        public void Add(KeyValuePair<uint, Scope> item) => _phasedScopes.Add(item.Key, item.Value);

        public void Clear() => _phasedScopes.Clear();

        public bool Contains(KeyValuePair<uint, Scope> item) => LazilyGet(item.Key) == item.Value;
        public bool ContainsKey(uint key) => _phasedScopes.ContainsKey(key);

        public void CopyTo(KeyValuePair<uint, Scope>[] array, int arrayIndex)
        {
            IEnumerator<KeyValuePair<uint, Scope>> enumerator = GetEnumerator();

            for (int i = arrayIndex; enumerator.MoveNext(); ++i)
            {
                array[i] = enumerator.Current;
            }
        }

        public IEnumerator<KeyValuePair<uint, Scope>> GetEnumerator() => _phasedScopes.GetEnumerator();

        public bool Remove(uint key) => _phasedScopes.Remove(key);

        public bool Remove(KeyValuePair<uint, Scope> item) => _phasedScopes.Remove(item.Key);

        public bool TryGetValue(uint key, [MaybeNullWhen(false)] out Scope value)
        {
            return _phasedScopes.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() => _phasedScopes.GetEnumerator();

        #endregion
    }
}

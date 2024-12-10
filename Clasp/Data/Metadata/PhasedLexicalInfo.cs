using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding;

namespace Clasp.Data.Metadata
{
    internal sealed class PhasedLexicalInfo : IDictionary<int, ScopeSet>
    {
        private readonly Dictionary<int, ScopeSet> _phaseMaps;

        public PhasedLexicalInfo()
        {
            _phaseMaps = new Dictionary<int, ScopeSet>();
        }

        public PhasedLexicalInfo(PhasedLexicalInfo original)
        {
            _phaseMaps = new Dictionary<int, ScopeSet>(original._phaseMaps);
        }

        #region (Auto-generated) IDictionary Implementation

        public ScopeSet this[int key] { get => ((IDictionary<int, ScopeSet>)_phaseMaps)[key]; set => ((IDictionary<int, ScopeSet>)_phaseMaps)[key] = value; }

        public ICollection<int> Keys => ((IDictionary<int, ScopeSet>)_phaseMaps).Keys;

        public ICollection<ScopeSet> Values => ((IDictionary<int, ScopeSet>)_phaseMaps).Values;

        public int Count => ((ICollection<KeyValuePair<int, ScopeSet>>)_phaseMaps).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<int, ScopeSet>>)_phaseMaps).IsReadOnly;

        public void Add(int key, ScopeSet value)
        {
            ((IDictionary<int, ScopeSet>)_phaseMaps).Add(key, value);
        }

        public void Add(KeyValuePair<int, ScopeSet> item)
        {
            ((ICollection<KeyValuePair<int, ScopeSet>>)_phaseMaps).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<int, ScopeSet>>)_phaseMaps).Clear();
        }

        public bool Contains(KeyValuePair<int, ScopeSet> item)
        {
            return ((ICollection<KeyValuePair<int, ScopeSet>>)_phaseMaps).Contains(item);
        }

        public bool ContainsKey(int key)
        {
            return ((IDictionary<int, ScopeSet>)_phaseMaps).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<int, ScopeSet>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<int, ScopeSet>>)_phaseMaps).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<int, ScopeSet>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<int, ScopeSet>>)_phaseMaps).GetEnumerator();
        }

        public bool Remove(int key)
        {
            return ((IDictionary<int, ScopeSet>)_phaseMaps).Remove(key);
        }

        public bool Remove(KeyValuePair<int, ScopeSet> item)
        {
            return ((ICollection<KeyValuePair<int, ScopeSet>>)_phaseMaps).Remove(item);
        }

        public bool TryGetValue(int key, [MaybeNullWhen(false)] out ScopeSet value)
        {
            return ((IDictionary<int, ScopeSet>)_phaseMaps).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_phaseMaps).GetEnumerator();
        }

        #endregion
    }
}

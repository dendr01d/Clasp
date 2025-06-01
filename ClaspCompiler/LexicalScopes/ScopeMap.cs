using System.Collections;
using System.Diagnostics.CodeAnalysis;

using ClaspCompiler.SchemeData;

namespace ClaspCompiler.LexicalScopes
{
    /// <summary>
    /// Maps phased scope sets to renamed symbols
    /// </summary>
    //internal sealed class ScopeMap : IDictionary<ScopeSet, RenameRecord>
    //{
    //    private readonly Dictionary<ScopeSet, RenameRecord> _scopesToRenames;
    //    private IDictionary<ScopeSet, RenameRecord> _asInterface => _scopesToRenames;


    //    public ScopeMap()
    //    {
    //        _scopesToRenames = [];
    //    }

    //    #region Convenience Dictionary Methods
    //    public Symbol this[ScopeSet key1, Symbol key2]
    //    {
    //        get => TryGetValue(key1, out RenameRecord rec) && rec.TryGetValue(key2, out Symbol? value)
    //            ? value
    //            : throw new KeyNotFoundException();
    //        set
    //        {
    //            if (TryGetValue(key1, out RenameRecord rec))
    //            {
    //                rec[key2] = value;
    //            }
    //            throw new KeyNotFoundException();
    //        }
    //    }

    //    public bool TryGetValue(ScopeSet key1, Symbol key2, [NotNullWhen(true)] out Symbol? value)
    //    {
    //        value = null;
    //        return TryGetValue(key1, out RenameRecord? rec)
    //            && rec.TryGetValue(key2, out value);
    //    }
    //    #endregion

    //    #region Native IDictionary Implementation

    //    public int Count => _scopesToRenames.Count;
    //    public bool IsReadOnly => _asInterface.IsReadOnly;
    //    public ICollection<ScopeSet> Keys => _scopesToRenames.Keys;
    //    public ICollection<RenameRecord> Values => _scopesToRenames.Values;
    //    public RenameRecord this[ScopeSet key]
    //    {
    //        get => TryGetValue(key, out RenameRecord value) ? value : throw new KeyNotFoundException();
    //        set => _scopesToRenames[key] = value;
    //    }

    //    public void Add(ScopeSet key, RenameRecord value) => _scopesToRenames.Add(key, value);
    //    public bool ContainsKey(ScopeSet key) => _scopesToRenames.ContainsKey(key);
    //    public bool Remove(ScopeSet key) => _scopesToRenames.Remove(key);
    //    public bool TryGetValue(ScopeSet key, [NotNullWhen(true)] out RenameRecord value)
    //    {
    //        value = _scopesToRenames
    //            .MaxBy(x => x.Key.SubsetScore(key)).Value;
    //        return value is not null;
    //    }
    //    public void Add(KeyValuePair<ScopeSet, RenameRecord> item) => _asInterface.Add(item);
    //    public void Clear() => _scopesToRenames.Clear();
    //    public bool Contains(KeyValuePair<ScopeSet, RenameRecord> item) => _scopesToRenames.Contains(item);
    //    public void CopyTo(KeyValuePair<ScopeSet, RenameRecord>[] array, int arrayIndex) => _asInterface.CopyTo(array, arrayIndex);
    //    public bool Remove(KeyValuePair<ScopeSet, RenameRecord> item) => _asInterface.Remove(item);
    //    public IEnumerator<KeyValuePair<ScopeSet, RenameRecord>> GetEnumerator() => _scopesToRenames.GetEnumerator();
    //    IEnumerator IEnumerable.GetEnumerator() => _scopesToRenames.GetEnumerator();

    //    #endregion
    //}
}

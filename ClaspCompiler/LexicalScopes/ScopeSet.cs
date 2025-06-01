namespace ClaspCompiler.LexicalScopes
{
    //internal sealed class ScopeSet : IPrintable, IEquatable<ScopeSet>
    //{
    //    public readonly uint Phase;
    //    public readonly IReadOnlySet<uint> Ids;

    //    public ScopeSet(uint phase, params uint[] ids)
    //    {
    //        Phase = phase;
    //        Ids = new HashSet<uint>(ids);
    //    }
    //    public ScopeSet(uint phase, IEnumerable<uint> ids) :this(phase, ids.ToArray()) { }

    //    /// <summary>
    //    /// Calculates how many of <paramref name="other"/>'s Ids are subsets of this scope set
    //    /// </summary>
    //    public int SubsetScore(ScopeSet other) => other.Ids.Count - other.Ids.Except(Ids).Count();

    //    public ScopeSet AddScopes(IEnumerable<uint> ids) => new ScopeSet(Phase, Ids.Union(ids));
    //    public ScopeSet SubtractScopes(IEnumerable<uint> ids) => new ScopeSet(Phase, Ids.Except(ids));
    //    public ScopeSet FlipScopes(IEnumerable<uint> ids)
    //    {
    //        HashSet<uint> set = Ids.ToHashSet();
    //        set.SymmetricExceptWith(ids);
    //        return new ScopeSet(Phase, set);
    //    }

    //    public override string ToString() => string.Format("{{{0}}} @ {1}",
    //        string.Join(", ", Ids),
    //        Phase);

    //    public void Print(TextWriter writer, int indent) => writer.Write(ToString());


    //    public bool Equals(ScopeSet? other)
    //    {
    //        return other is not null
    //            && other.Phase == Phase
    //            && other.Ids.SetEquals(Ids);
    //    }
    //    public override bool Equals(object? obj) => obj is ScopeSet ss && Equals(ss);
    //    public override int GetHashCode() => HashCode.Combine(Phase, Ids.ToArray());
    //}
}

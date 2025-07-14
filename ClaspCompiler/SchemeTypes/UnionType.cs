
using System.Collections.Immutable;

namespace ClaspCompiler.SchemeTypes
{
    internal sealed record UnionType : SchemeType
    {
        public ImmutableHashSet<SchemeType> Types { get; init; }
        public string? NameOverride { private get; init; } = null;
        private readonly Lazy<int> _lazyHash;

        private UnionType(IEnumerable<SchemeType> types, string? nameOverride)
        {
            Types = [.. types];
            NameOverride = nameOverride;
            _lazyHash = new(() => CreateVariadicHash(nameof(UnionType), Types));
        }

        public static SchemeType Join(IEnumerable<SchemeType> types, string? nameOverride = null)
        {
            HashSet<SchemeType> uniqueTypes = [];

            foreach(SchemeType t in types)
            {
                if (t is UnionType ut)
                {
                    uniqueTypes.UnionWith(ut.Types);
                }
                else
                {
                    uniqueTypes.Add(t);
                }
            }

            if (uniqueTypes.Count == 1)
            {
                return uniqueTypes.First();
            }

            return new UnionType(uniqueTypes, nameOverride);
        }

        public static SchemeType Join(params SchemeType[] types) => Join(types.AsEnumerable(), null);

        //public UnionType(params SchemeType[] types) : this(types.ToImmutableHashSet()) { }
        //public UnionType(IEnumerable<SchemeType> types, string? nameOverride = null) : this(types.ToImmutableHashSet())
        //{
        //    NameOverride = nameOverride;
        //}

        public override string AsString => NameOverride ?? $"(∪{string.Concat(Types.Select(x => $" {x}"))})";

        public bool Equals(UnionType? other) => other is not null && Types.SetEquals(other.Types);
        public override int GetHashCode() => _lazyHash.Value;

    }
}
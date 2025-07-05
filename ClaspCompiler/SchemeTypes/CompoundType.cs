using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

using ClaspCompiler.SchemeTypes;

namespace MetallicScheme.SchemeTypes
{
    [CollectionBuilder(typeof(CompoundType), nameof(Create))]
    internal sealed record CompoundType(ImmutableArray<SchemeType> InnerTypes) : SchemeType, IReadOnlyCollection<SchemeType>, IEquatable<SchemeType>
    {
        public static CompoundType Create(ReadOnlySpan<SchemeType> values) => new(values);
        public CompoundType(ReadOnlySpan<SchemeType> values) : this(values.ToImmutableArray()) { }

        public CompoundType(params SchemeType[] types) : this(types.ToImmutableArray()) { }
        public CompoundType(IEnumerable<SchemeType> types) : this(types.ToImmutableArray()) { }

        public override string AsString => $"({string.Join(' ', InnerTypes)})";

        public bool Equals(CompoundType? other) => other is not null
            && InnerTypes.SequenceEqual(other.InnerTypes);

        public override int GetHashCode() => InnerTypes.Length == 0
            ? 0
            : RecursiveHash(InnerTypes[0], InnerTypes[1..]);
        private static int RecursiveHash(SchemeType type, ImmutableArray<SchemeType> moreTypes)
        {
            return (moreTypes.Length == 0)
                ? type.GetHashCode()
                : HashCode.Combine(type.GetHashCode(), RecursiveHash(moreTypes[0], moreTypes[1..]));
        }

        public int Count => ((IReadOnlyCollection<SchemeType>)InnerTypes).Count;
        public IEnumerator<SchemeType> GetEnumerator() => ((IEnumerable<SchemeType>)InnerTypes).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)InnerTypes).GetEnumerator();
    }
}

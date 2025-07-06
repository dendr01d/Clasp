using System.Collections.Immutable;

namespace ClaspCompiler.SchemeTypes
{
    internal sealed record VectorType(ImmutableArray<SchemeType> Types) : SchemeType
    {
        private readonly Lazy<int> _lazyHash = new(() => CreateVariadicHash(nameof(VectorType), Types));

        public override string AsString => $"Vector<{string.Join(", ", Types)}>";
        public bool Equals(VectorType? other) => other is not null && Types.SequenceEqual(other.Types);
        public override int GetHashCode() => _lazyHash.Value;
    }
}

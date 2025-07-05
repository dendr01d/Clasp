using System.Collections.Immutable;

namespace ClaspCompiler.SchemeTypes
{
    internal sealed record VectorType(ImmutableArray<SchemeType> InnerTypes) : SchemeType
    {
        public override string AsString => $"Vector<{string.Join(", ", InnerTypes)}>";
    }
}

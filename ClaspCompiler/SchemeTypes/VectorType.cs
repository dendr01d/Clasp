using ClaspCompiler.SchemeTypes;

namespace MetallicScheme.SchemeTypes
{
    internal sealed record VectorType(CompoundType InnerTypes) : SchemeType
    {
        public override string AsString => $"(Vector {string.Join(' ', InnerTypes)})";
    }
}

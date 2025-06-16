using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal sealed class MetaData
    {
        public SchemeType Type { get; init; } = AtomicType.Unknown;
    }
}

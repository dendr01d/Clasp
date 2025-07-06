using ClaspCompiler.CompilerData;

namespace ClaspCompiler.SchemeTypes
{
    internal sealed record AtomicType : SchemeType
    {
        private readonly string _typeName;

        public AtomicType(string typeName)
        {
            _typeName = typeName;
        }

        public override string AsString => _typeName;
    }
}

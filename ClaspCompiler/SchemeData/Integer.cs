using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Integer : ValueBase<int>, IValue
    {
        public Integer(int i) : base(i, AtomicType.Integer) { }
    }
}

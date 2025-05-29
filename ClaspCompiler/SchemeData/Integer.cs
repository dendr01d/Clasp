using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Integer : ValueBase<int>, IValue
    {
        public Integer(int i) : base(i) { }
    }
}

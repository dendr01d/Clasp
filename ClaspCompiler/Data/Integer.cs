namespace ClaspCompiler.Data
{
    internal sealed record Integer : ValueBase<int>
    {
        public Integer(int i) : base(i) { }
    }
}

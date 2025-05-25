namespace ClaspCompiler.Common
{
    internal interface IApplication<T>
        where T : IPrintable
    {
        public T Operator { get; }
        public IEnumerable<T> Arguments { get; }
        public int Adicity { get; }
    }
}

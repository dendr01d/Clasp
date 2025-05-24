namespace ClaspCompiler.Common
{
    internal interface IApplication<T>
        where T : IPrintable
    {
        T Operator { get; }
        IEnumerable<T> GetArguments();
    }
}

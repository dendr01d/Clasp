using ClaspCompiler.Data;

namespace ClaspCompiler.Common
{
    internal sealed record Literal<T> : IAtom<T>
        where T : ITerm
    {
        public TypeName TypeName { get; init; }
        public T Data { get; init; }
        public Literal(TypeName typeName, T value)
        {
            TypeName = typeName;
            Data = value;
        }

        public override string ToString() => string.Format("({0} {1})", TypeName.ToString().ToLower(), Data);
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}

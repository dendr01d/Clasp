using ClaspCompiler.Data;

namespace ClaspCompiler.Common
{
    internal sealed record Literal<T> : ILiteral
        where T : ITerm
    {
        public T Value { get; init; }
        public Literal(T value) => Value = value;
        public ITerm GetValue() => Value;

        public override string ToString() => Value.ToString();
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}

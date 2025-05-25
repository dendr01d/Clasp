using ClaspCompiler.Data;

namespace ClaspCompiler.Common
{
    internal sealed record Literal<T> : ILiteral
        where T : ITerm
    {
        private string _type { get; init; }
        public T Value { get; init; }
        public Literal(T value, string type)
        {
            Value = value;
            _type = type;
        }
        public ITerm GetValue() => Value;
        public string GetTypeName() => _type;

        public override string ToString() => string.Format("({0} {1})", _type, Value);
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}

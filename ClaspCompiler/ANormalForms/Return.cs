namespace ClaspCompiler.ANormalForms
{
    internal sealed class Return : ITail
    {
        public readonly INormExp Value;

        public Return(INormExp value) => Value = value;

        public override string ToString() => $"(return {Value})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(return ", ref indent);
            writer.Write(Value, indent);
            writer.Write(')');
        }
    }
}

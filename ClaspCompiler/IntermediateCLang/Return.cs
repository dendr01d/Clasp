using ClaspCompiler.IntermediateCLang.Abstract;

namespace ClaspCompiler.IntermediateCLang
{
    internal sealed class Return : ITail
    {
        public readonly INormArg Value;

        public Return(INormArg value) => Value = value;

        public bool CanBreak => Value.CanBreak;
        public override string ToString() => $"(return {Value})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(return ", ref indent);
            writer.Write(Value, indent);
            writer.Write(')');
        }
    }
}

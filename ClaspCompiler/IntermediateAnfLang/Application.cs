using ClaspCompiler.IntermediateAnfLang.Abstract;

namespace ClaspCompiler.IntermediateAnfLang
{
    internal sealed class Application : INormApp
    {
        public readonly string Operator;
        public readonly INormArg[] Arguments;

        public Application(string op, params INormArg[] args)
        {
            Operator = op;
            Arguments = args;
        }

        public bool CanBreak => true;
        public override string ToString() => string.Format("({0}{1})",
            Operator,
            string.Concat(string.Join(' ', Arguments.AsEnumerable())));
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(Operator, Arguments, indent);
    }
}

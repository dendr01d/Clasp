using ClaspCompiler.IntermediateCLang.Abstract;

namespace ClaspCompiler.IntermediateCLang
{
    internal sealed class Application : INormApp
    {
        public string Operator { get; init; }
        public INormArg[] Arguments { get; init; }

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

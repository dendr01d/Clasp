
using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed class Application : ISemanticExp
    {
        public readonly string Operator;
        public readonly ISemanticExp[] Arguments;

        public Application(string op, params ISemanticExp[] args)
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

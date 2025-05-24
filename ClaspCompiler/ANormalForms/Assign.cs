using ClaspCompiler.Common;

namespace ClaspCompiler.ANormalForms
{
    internal sealed class Assign : IStatement
    {
        public readonly Var Variable;
        public readonly INormExp Value;

        public Assign(Var var, INormExp value)
        {
            Variable = var;
            Value = value;
        }

        public override string ToString() => $"(assign {Variable} {Value})";
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(assign ", ref indent);
            writer.Write(Variable, indent);

            if (Value is ILiteral)
            {
                writer.Write(' ');
            }
            else
            {
                writer.WriteLineIndent(indent);
            }

            writer.Write(Value, indent);

            writer.Write(')');
        }
    }
}

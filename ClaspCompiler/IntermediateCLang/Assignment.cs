using ClaspCompiler.IntermediateCLang.Abstract;
using ClaspCompiler.CompilerData;

namespace ClaspCompiler.IntermediateCLang
{
    internal sealed class Assignment : IStatement
    {
        public readonly Var Variable;
        public readonly INormExp Value;

        public Assignment(Var var, INormExp value)
        {
            Variable = var;
            Value = value;
        }

        public bool CanBreak => Value.CanBreak;
        public override string ToString() => $"(assign {Variable} {Value})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(Variable, [Value], indent);
    }
}

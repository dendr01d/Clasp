using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCps.Abstract;

namespace ClaspCompiler.IntermediateCps
{
    internal sealed class GoTo : ITail
    {
        public string ControlCode => "JMP";

        public Label Label { get; init; }

        public Dictionary<Var, int> FreeVariables => [];

        public GoTo(Label label)
        {
            Label = label;
        }

        public bool BreaksLine => false;
        public string AsString => $"({ControlCode} {Label})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(ControlCode, [Label], indent);
        public sealed override string ToString() => AsString;
    }
}

using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCps.Abstract;

namespace ClaspCompiler.IntermediateCps
{
    //internal sealed class SideEffect : IStatement
    //{
    //    public string ControlCode => "SFX";
    //    public ICpsExp Value { get; init; }

    //    public Dictionary<VarBase, int> FreeVariables { get; init; }

    //    public SideEffect(ICpsExp value)
    //    {
    //        Value = value;
    //        FreeVariables = value.CountFreeVariables();
    //    }

    //    public bool BreaksLine => Value.BreaksLine;
    //    public string AsString => $"({ControlCode} {Value})";
    //    public void Print(TextWriter writer, int indent) => writer.WriteApplication(ControlCode, [Value], indent);
    //    public sealed override string ToString() => AsString;
    //}
}

using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCps.Abstract;

namespace ClaspCompiler.IntermediateCps
{
    //internal sealed class Assignment : IStatement
    //{
    //    public string ControlCode => "BND";

    //    public VarBase Variable { get; init; }
    //    public ICpsExp Value { get; init; }

    //    public Dictionary<VarBase, int> FreeVariables { get; init; }

    //    public Assignment(VarBase var, ICpsExp value)
    //    {
    //        Variable = var;
    //        Value = value;

    //        FreeVariables = value.CountFreeVariables();
    //        FreeVariables.Remove(var);
    //    }

    //    public bool BreaksLine => Value.BreaksLine;
    //    public string AsString => $"({ControlCode} {Variable} {Value})";
    //    public void Print(TextWriter writer, int indent) => writer.WriteApplication(ControlCode, [Variable, Value], indent);
    //    public sealed override string ToString() => AsString;
    //}
}

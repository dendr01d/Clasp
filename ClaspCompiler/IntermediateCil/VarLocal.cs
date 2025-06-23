using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCil.Abstract;

namespace ClaspCompiler.IntermediateCil
{
    internal sealed record TempVar : IRegister
    {
        public int Index => -1;
        public string Name { get; init; }

        public TempVar(string name) => Name = name;
        public TempVar(Var v) : this(v.Name) { }

        public bool BreaksLine => false;
        public string AsString => $"V_{Name}";
        public void Print(TextWriter writer, int indent) => throw new NotImplementedException();
        public override string ToString() => AsString;
    }
}

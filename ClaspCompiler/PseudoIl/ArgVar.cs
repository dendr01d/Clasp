
using ClaspCompiler.PseudoIl;

internal sealed class ArgMem : IMem, IArgument
{
    public readonly int Index;
    public ArgMem(int index) => Index = index;
    public override string ToString() => $"A{Index}";
    public void Print(TextWriter writer, int indent) => writer.Write(ToString());
}

using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateLocLang.Abstract;

namespace ClaspCompiler.IntermediateLocLang
{
    internal sealed record VarRef : ILocArg
    {
        public readonly Var Variable;

        public VarRef(Var var) => Variable = var;

        public bool CanBreak => false;
        public override string ToString() => $"(deref {Variable.Name})";
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}

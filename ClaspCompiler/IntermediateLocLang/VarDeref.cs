
using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateVarLang.Abstract;

namespace ClaspCompiler.IntermediateVarLang
{
    internal sealed record VarDeref : ILocArg
    {
        public readonly Var Variable;

        public VarDeref(Var var) => Variable = var;

        public bool CanBreak => false;
        public override string ToString() => $"(deref {Variable.Name})";
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}

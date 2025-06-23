using ClaspCompiler.SchemeCore.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeCore
{
    internal sealed class Mutation : ICoreExp
    {
        public readonly ICoreVar Variable;
        public readonly ICoreExp NewValue;
        public SchemeType Type => AtomicType.Void;

        public Mutation(ICoreVar var, ICoreExp newValue)
        {
            Variable = var;
            NewValue = newValue;
        }

        public bool BreaksLine => true;
        public string AsString => $"(set! {Variable} {NewValue})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication("set!", [Variable, NewValue], indent);
        public sealed override string ToString() => AsString;
    }
}

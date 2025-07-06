using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record TypedVariable(Variable Variable, SchemeType
        Type)
        : AnnotatedBase<Variable>(Variable, Type), ISemVar
    {
        public string Name => Variable.Name;
        public ISemVar Parameter => this;
        public ISemParameters? Next => null;
    }
}

using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Quasiquote : AbstractQuote, ISemExp
    {
        protected override string Prefix => "`";
        protected override string Keyword => "quasiquote";

        public override ISemQQ Value { get; }

        public Quasiquote(ISemQQ value) => Value = value;
    }
}

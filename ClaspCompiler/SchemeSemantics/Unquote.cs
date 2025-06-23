using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Unquote : AbstractQuote
    {
        protected override string Prefix => ",";
        protected override string Keyword => "unquote";

        public override ISemQQ Value { get; }

        public Unquote(ISemQQ value) => Value = value;
    }
}

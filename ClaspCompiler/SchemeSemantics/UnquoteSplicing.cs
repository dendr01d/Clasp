using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record UnquoteSplicing : AbstractQuote, ISemExp
    {
        protected override string Prefix => ",@";
        protected override string Keyword => "unquote-splicing";

        public override ISemQQ Value { get; }

        public UnquoteSplicing(ISemQQ value) => Value = value;
    }
}

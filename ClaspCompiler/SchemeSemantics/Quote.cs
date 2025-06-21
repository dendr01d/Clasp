using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Quote : AbstractQuote, ISemExp
    {
        protected override string Prefix => "'";
        protected override string Keyword => "quote";

        public override ISemQQ Value { get; }

        public Quote(ISemQQ value) => Value = value;
    }
}

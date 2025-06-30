using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Quotation(ISchemeExp Value, uint AstId) : ISemLiteral
    {
        public SchemeType Type => Value.Type;

        public bool BreaksLine => Value.BreaksLine;
        public string AsString => $"'{Value}";
        public void Print(TextWriter writer, int indent)
        {
            if (BreaksLine)
            {
                writer.WriteApplication(SpecialKeyword.Quote.Name, [Value], indent);
            }
            else
            {
                writer.Write(AsString);
            }
        }
        public sealed override string ToString() => AsString;
    }
}

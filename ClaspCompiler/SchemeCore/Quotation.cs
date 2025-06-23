using ClaspCompiler.SchemeCore.Abstract;
using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeCore
{
    internal sealed class Quotation : ICoreExp
    {
        public ISchemeExp Value { get; init; }
        public SchemeType Type => Value.Type;

        public Quotation(ISchemeExp value)
        {
            Value = value;
        }
        public bool BreaksLine => Value.BreaksLine;
        public string AsString => $"'{Value}";
        public void Print(TextWriter writer, int indent)
        {
            if (!Value.BreaksLine)
            {
                writer.Write(AsString);
            }
            else
            {
                writer.WriteIndenting("(quote ", ref indent);
                writer.Write(Value, indent);
                writer.Write(')');
            }
        }
        public sealed override string ToString() => AsString;
    }
}

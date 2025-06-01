using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Quotation : IAtom
    {
        public ISchemeExp Quoted { get; init; }
        public bool IsAtom => true;
        public bool IsNil => Quoted is Nil;

        public Quotation(ISchemeExp quoted) => Quoted = quoted;

        public bool CanBreak => Quoted.CanBreak;
        public override string ToString() => Quoted.ToString();
        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting('\'', ref indent);
            writer.Write(Quoted, indent);
        }
    }
}

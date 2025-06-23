using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeSemantics
{
    /// <summary>
    /// Encapsulates a raw piece of scheme data
    /// </summary>
    internal sealed class SemDatum : ISemExp
    {
        public readonly ISchemeExp Datum;
        public MetaData MetaData { get; init; }

        public SemDatum(ISchemeExp exp, MetaData? meta = null)
        {
            Datum = exp;
            MetaData = meta ?? new()
            {
                Type = exp.Type
            };
        }

        public bool BreaksLine => Datum.BreaksLine;
        public string AsString => Datum.AsString;
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}

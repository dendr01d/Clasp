using MetallicScheme.SchemeData.Abstract;
using MetallicScheme.SchemeSemantics.Abstract;
using MetallicScheme.SchemeTypes;

namespace MetallicScheme.SchemeSemantics
{
    internal sealed class PrimitiveApplication : ISemApp
    {
        public SchemeType Type { get; init; }
        public PrimitiveOperator Operator { get; init; }
        public ISemExp[] Arguments { get; init; }

        public PrimitiveApplication(PrimitiveOperator op, params ISemExp[] args)
        {
            Type = op.GetReturnType();
            Operator = op;
            Arguments = args;
        }

        public PrimitiveApplication(PrimitiveOperator op, IEnumerable<ISemExp> args)
            : this(op, args.ToArray())
        { }

        public bool BreaksLine => Arguments.Any(x => x.BreaksLine);
        public string AsString => $"({Operator.Stringify()}{string.Concat(Arguments.Select(x => $" {x}"))})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(Operator.Stringify(), Arguments, indent);
        public sealed override string ToString() => AsString;
    }
}

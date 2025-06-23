using ClaspCompiler.IntermediateCps.Abstract;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.IntermediateCps
{
    internal sealed class Application : ICpsApp
    {
        public bool IOBound { get; }
        public PrimitiveOperator Operator { get; init; }
        public ICpsExp[] Arguments { get; init; }

        public Application(PrimitiveOperator op, params ICpsExp[] args)
        {
            Operator = op;
            Arguments = args;
        }

        public Application(PrimitiveOperator op, IEnumerable<ICpsExp> args)
            : this(op, args.ToArray())
        { }

        public bool BreaksLine => Arguments.Any(x => x.BreaksLine);
        public string AsString => $"({Operator.Stringify()}{string.Concat(Arguments.Select(x => $" {x}"))})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(Operator.Stringify(), Arguments, indent);
        public sealed override string ToString() => AsString;

        public bool Equals(ICpsExp? other) => other is Application app
            && Operator == app.Operator
            && Arguments.SequenceEqual(app.Arguments);
    }
}

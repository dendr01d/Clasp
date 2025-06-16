using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    /// <summary>
    /// Represents the application of a primitive operator to a list of arguments
    /// </summary>
    internal sealed class SemApp : ISemExp
    {
        public ISemExp Procedure { get; init; }
        public ISemExp[] Arguments { get; init; }
        public MetaData MetaData { get; init; }

        private SemApp(ISemExp proc, IEnumerable<ISemExp> args, MetaData? meta)
        {
            Procedure = proc;
            Arguments = [.. args];
            MetaData = meta ?? new();
        }

        public SemApp(Operator op, IEnumerable<ISemExp> args, MetaData? meta = null) : this((ISemExp)op, args, meta) { }
        public SemApp(Lambda lambda, ISemExp arg, MetaData? meta = null) : this(lambda, [arg], meta) { }
        public SemApp(SemVar boundFun, IEnumerable<ISemExp> args, MetaData? meta = null) : this((ISemExp)boundFun, args, meta) { }

        public bool BreaksLine => Arguments.Any(x => x.BreaksLine);
        public string AsString => $"({Procedure}{OneLineArgs(Arguments)})";
        public void Print(TextWriter writer, int indent)
        {
            if (Procedure is Lambda lam)
            {
                PrintAsLet(writer, indent, lam, Arguments[0]);
            }
            else
            {
                writer.WriteApplication(Procedure, Arguments, indent);
            }
        }
        public sealed override string ToString() => AsString;

        // ---

        private static string OneLineArgs(ISemExp[] args) => string.Concat(args.Select(x => $" {x}"));

        private static void PrintAsLet(TextWriter writer, int indent, Lambda proc, ISemExp arg)
        {
            writer.WriteIndenting("(let ", ref indent);
            int restIndent = indent;

            writer.WriteIndenting("([", ref indent);
            writer.WriteIndenting(proc.Variable, ref indent);
            writer.WriteIndenting(' ', ref indent);

            writer.Write(arg, indent);

            writer.WriteLineIndent("])", restIndent);
            writer.Write(proc.Body, restIndent);

            writer.Write(')');
        }
    }
}

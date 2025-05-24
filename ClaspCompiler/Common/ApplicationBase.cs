namespace ClaspCompiler.Common
{
    internal abstract class ApplicationBase<T> : IApplication<T>, IPrintable
        where T : IPrintable
    {
        public T Operator { get; init; }
        private readonly T[] _args;

        protected ApplicationBase(T op, params T[] args)
        {
            Operator = op;
            _args = args;
        }

        public IEnumerable<T> GetArguments() => _args;

        public override string ToString()
        {
            return string.Format("({0}{1})",
                Operator,
                string.Concat(GetArguments().Select(x => $" {x}")));
        }

        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            writer.WriteIndenting(Operator, ref indent);

            if (_args.Length > 0)
            {
                if (_args.All(x => x is ILiteral))
                {
                    foreach (var arg in _args)
                    {
                        writer.Write(' ');
                        arg.Print(writer, indent);
                    }
                }
                else
                {
                    writer.WriteIndenting(' ', ref indent);
                    _args[0].Print(writer, indent);

                    foreach (var arg in _args.Skip(1))
                    {
                        writer.WriteLineIndent(indent);
                        arg.Print(writer, indent);
                    }
                }

            }

            writer.Write(')');
        }
    }
}

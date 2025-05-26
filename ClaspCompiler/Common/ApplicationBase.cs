namespace ClaspCompiler.Common
{
    internal abstract class ApplicationBase<T> : IApplication<T>, IPrintable
        where T : IPrintable
    {
        public T Operator { get; init; }
        public IEnumerable<T> Arguments => _args;
        public int Adicity => _args.Count;

        private readonly List<T> _args;

        protected ApplicationBase(T op, IEnumerable<T> args)
        {
            Operator = op;
            _args = args.ToList();
        }

        public override string ToString()
        {
            return string.Format("({0}{1})",
                Operator is Var v ? v.Data : Operator,
                string.Concat(Arguments.Select(x => $" {x}")));
        }

        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            writer.WriteIndenting(Operator is Var v ? v.Data : Operator, ref indent);

            if (Adicity > 0)
            {
                if (_args.All(x => x is IAtom))
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

                    foreach (var arg in Arguments.Skip(1))
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

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal sealed record ParamsForm(ISemVar[] Parameters, ISemVar? VariadicParameter) : IPrintable
    {
        public bool Any => HasParams || HasVariad;
        public bool NeedsParens => HasParams || !HasVariad;

        private bool HasParams => Parameters.Length > 0;
        private bool HasVariad => VariadicParameter is not null;

        private static string PrintIf(bool cond, string str)
        {
            return cond ? str : string.Empty;
        }

        public bool BreaksLine => false;
        public string AsString => string.Concat([
            string.Join(' ', Parameters.AsEnumerable()),
            PrintIf(HasParams && HasVariad, " . "),
            PrintIf(HasVariad, VariadicParameter!.ToString())
        ]);

        public void Print(TextWriter writer, int indent)
        {
            if (HasParams)
            {
                writer.Write(string.Join(' ', Parameters.AsEnumerable()));

                if (HasVariad)
                {
                    writer.Write(" . ");
                }
            }
            if (HasVariad)
            {
                writer.Write(VariadicParameter!.ToString(), indent);
            }
        }

        public string AsStandalone => NeedsParens ? $"({AsString})" : AsString;
        public void PrintStandalone(TextWriter writer, int indent)
        {
            if (NeedsParens)
            {
                writer.Write('(');
            }

            Print(writer, indent);

            if (NeedsParens)
            {
                writer.Write(')');
            }

        }
    }
}

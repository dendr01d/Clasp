using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Formals(ISemVar[] Parameters, ISemVar? VarParam) : ISemFormals
    {
        private readonly string _formatted = Format(Parameters, VarParam);

        public bool BreaksLine => false;
        public string AsString => _formatted;
        public void Print(TextWriter writer, int indent) => writer.Write(_formatted);
        public sealed override string ToString() => AsString;

        private static string Format(ISemVar[] parms, ISemVar? varPar)
        {
            if (varPar is null)
            {
                if (parms.Length == 0)
                {
                    return "()";
                }
                else
                {
                    return $"({string.Join(' ', parms.AsEnumerable())})";
                }
            }
            else
            {
                if (parms.Length == 0)
                {
                    return varPar.AsString;
                }
                else
                {
                    return $"({string.Join(' ', parms.AsEnumerable())} . {varPar})";
                }
            }
        }
    }
}

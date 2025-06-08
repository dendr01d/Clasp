using ClaspCompiler.SchemeData;

namespace ClaspCompiler.CompilerData
{
    internal static class Keyword
    {
        public const string LET = "let";
        public const string IF = "if";

        public const string PLUS = "+";
        public const string MINUS = "-";

        public const string READ = "read";

        public const string EQ = "eq?";
        public const string LT = "<";
        public const string LTE = "<=";
        public const string GT = ">";
        public const string GTE = ">=";

        public const string NOT = "not";
        public const string AND = "and";
        public const string OR = "or";

        public const string VECTOR = "vector";
        public const string VECTORREF = "vector-ref";
        public const string VECTORSET = "vector-set";

        public static readonly Dictionary<Symbol, Symbol> DefaultBindings = new string[]
        {
            LET, AND, OR, IF,

            PLUS, MINUS,

            NOT,

            EQ,

            LT, LTE, GT, GTE,

            READ,

            VECTOR, VECTORREF, VECTORSET
        }
        .Select(Symbol.Intern)
        .ToDictionary(x => x, x => x);
    }
}

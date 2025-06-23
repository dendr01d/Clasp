using ClaspCompiler.SchemeData;

namespace ClaspCompiler.CompilerData
{
    internal static class SpecialKeyword
    {
        public const string Apply = "apply";
        public const string Begin = "begin";
        public const string Cond = "cond";
        public const string Define = "define";
        public const string If = "if";
        public const string Lambda = "lambda";
        public const string Let = "let";
        public const string Letrec = "letrec";
        public const string LogicalAnd = "and";
        public const string LogicalOr = "or";
        public const string Quasiquote = "quasiquote";
        public const string Quote = "quote";
        public const string SetBang = "set!";
        public const string Unquote = "unquote";
        public const string UnquoteSplicing = "unquote-splicing";

        public static readonly string[] Keywords =
        [
            Apply,
            Begin,
            Cond,
            Define,
            If,
            Lambda,
            Let,
            Letrec,
            LogicalAnd,
            LogicalOr,
            Quasiquote,
            Quote,
            SetBang,
            Unquote,
            UnquoteSplicing
        ];
    }
}
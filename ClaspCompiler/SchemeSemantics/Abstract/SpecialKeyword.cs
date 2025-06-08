using ClaspCompiler.CompilerData;

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal enum SpecialKeyword
    {
        Let,
        If,
    }

    internal static class SpecialOperatorExtensions
    {
        public static string Stringify(this SpecialKeyword op)
        {
            return op switch
            {
                SpecialKeyword.Let => Keyword.LET,
                SpecialKeyword.If => Keyword.IF,

                _ => "<?>"
            };
        }
    }
}

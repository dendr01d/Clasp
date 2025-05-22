using Clasp.AST;

namespace Clasp.InterLangs.SyntacticScheme
{
    internal sealed class Character : Expr, ITerminal<Prog, char>
    {
        public char Value { get; }

        public Character(char value) : base()
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value switch
            {
                ' ' => @"#\space",
                '\t' => @"#\tab",
                '\n' => @"#\newline",
                '\r' => @"#\return",
                _ => Value.ToString()
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Lexer
{
    internal class Token
    {
        public readonly string Text;
        public readonly TokenType TType;
        public readonly int SourceIndex;
        public readonly int SourceLine;

        protected Token(string s, TokenType t, int line, int index)
        {
            Text = s;
            TType = t;
            SourceIndex = index;
            SourceLine = line;
        }

        public static Token Tokenize(TokenType tType, string s, int line, int index)
        {
            return new Token(s, tType, line, index);
        }

        private static readonly TokenType[] _staticMarkers = new TokenType[]
        {
            TokenType.OpenListParen,
            TokenType.OpenVecParen,
            TokenType.ClosingParen,
            TokenType.Quote,
            TokenType.Quasiquote,
            TokenType.Unquote,
            TokenType.UnquoteSplice,
            TokenType.Syntax,
            TokenType.QuasiSyntax,
            TokenType.Unsyntax,
            TokenType.UnsyntaxSplice,
            TokenType.DotOperator,
            TokenType.Undefined,
        };

        public override string ToString()
        {
            if (_staticMarkers.Contains(TType))
            {
                return string.Format("({0})", TType);
            }
            else
            {
                return string.Format("({0}){1}", TType, Text);
            }
        }
    }
}

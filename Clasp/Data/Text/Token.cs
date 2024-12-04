using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clasp.Data.Text;

namespace Clasp.Data.Text
{
    public class Token
    {
        public readonly string Text;
        public readonly TokenType TType;
        public readonly Blob SourceBlob;
        public readonly int LineIdx;
        public readonly int LineNum;

        public string SurroundingLine => SourceBlob[LineNum - 1]; //lines of text are 1-indexed

        protected Token(string s, TokenType t, Blob source, int line, int index)
        {
            Text = s;
            TType = t;
            SourceBlob = source;
            LineIdx = index;
            LineNum = line;
        }

        public static Token Tokenize(TokenType tType, string s, Blob source, int line, int index)
        {
            return new Token(s, tType, source, line, index);
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

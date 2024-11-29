using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Lexer;

namespace Clasp.Reader
{
    internal static class TokenStreamExtensions
    {
        public static bool Any(this IEnumerator<Token> tokens)
        {
            return tokens.MoveNext();
        }

        public static Token Pop(this IEnumerator<Token> tokens)
        {
            Token output = tokens.Current;
            if (!tokens.MoveNext())
            {
                throw new ReaderException("Token sequence ended unexpectedly.");
            }
            return output;
        }

        public static Token Peek(this IEnumerator<Token> tokens) => tokens.Current;
    }
}

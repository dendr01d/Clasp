using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Clasp
{
    internal static class Printer
    {
        public static string PrintLineErrorHelper(Lexer.Token token) => PrintLineErrorHelper(token.SurroundingLine, token.LineNum, token.LineIdx, token.Text);

        private const string INDENT = "   ";

        public static string PrintLineErrorHelper(string fullLine, int lineNumber, int index, string erroneousText)
        {
            string lineText = fullLine.Trim();

            string pointer = string.Concat(new string(' ', index - (fullLine.Length - lineText.Length)), "^");

            lineText = string.Concat(
                lineText.Substring(0, index),
                "\x1b[31;47m", //red text, white bg
                lineText.Substring(index, erroneousText.Length),
                "\x1b[0m" //reset
            );

            if (index + erroneousText.Length < fullLine.Length)
            {
                lineText = string.Concat(lineText, fullLine.Substring(index + erroneousText.Length));
            }

            return string.Concat(
                INDENT,
                lineText,
                System.Environment.NewLine,
                INDENT,
                pointer);
        }

        public static string PrintTokens(IEnumerable<Lexer.Token> tokens)
        {
            return string.Join(", ", tokens
                .Where(x => x.TType != Lexer.TokenType.Whitespace)
                .Select(x => string.Concat(
                    "\x1b[30;47m", //italic, black fg, white bg
                    x.Text,
                    "\x1b[0m" //reset
                )));
        }
    }
}

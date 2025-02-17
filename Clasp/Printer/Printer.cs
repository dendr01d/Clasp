using System.Collections.Generic;
using System.Linq;

using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp
{
    internal static class Printer
    {
        public static string PrintLineErrorHelper(ISourceTraceable ist)
        {
            return PrintLineErrorHelper(ist.Location.SourceText, ist.Location);
        }

        public static string PrintLineErrorHelper(Blob sourceText, SourceCode loc)
        {
            return PrintLineErrorHelper(sourceText.Lines[loc.NormalizedLineNumber], loc.LineNumber, loc.Column, loc.Length, loc.Source);
        }

        private const string INDENT = "   ";

        private static string PrintLineErrorHelper(string fullLine, int lineNo, int column, int length, string source)
        {
            string lineText = fullLine.Trim();

            string pointer = string.Concat(new string(' ', column - (fullLine.Length - lineText.Length)), "^");

            lineText = string.Concat(
                lineText.Substring(0, column),
                "\x1b[31;47m", //red text, white bg
                lineText.Substring(column, length),
                "\x1b[0m" //reset
            );

            if (column + length < fullLine.Length)
            {
                lineText = string.Concat(lineText, fullLine.Substring(column + length));
            }

            string reference = string.Format("{0}:{1}", source, lineNo);

            return string.Concat(
                INDENT,
                lineText,
                INDENT,
                INDENT,
                reference,
                System.Environment.NewLine,
                INDENT,
                pointer);
        }

        public static string PrintRawTokens(IEnumerable<Token> tokens)
        {
            return string.Join(", ", tokens
                .Where(x => x.TType != TokenType.Whitespace)
                .Select(x => string.Concat(
                    "\x1b[30;47m", //italic, black fg, white bg
                    x.Text,
                    "\x1b[0m" //reset
                ))) + " ";
        }
    }
}

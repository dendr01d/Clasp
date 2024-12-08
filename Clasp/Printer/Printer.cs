using Clasp.Data.Metadata;
using Clasp.Data.Text;

namespace Clasp
{
    internal static class Printer
    {
        public static string PrintLineErrorHelper(ISourceTraceable ist)
        {
            return PrintLineErrorHelper(ist.SourceText, ist.Location);
        }

        public static string PrintLineErrorHelper(Blob sourceText, SourceLocation loc)
        {
            return PrintLineErrorHelper(sourceText.Lines[loc.NormalizedLineNumber], loc.Column, loc.Length);
        }

        private const string INDENT = "   ";

        public static string PrintLineErrorHelper(string fullLine, int column, int length)
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

            return string.Concat(
                INDENT,
                lineText,
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
                )));
        }
    }
}

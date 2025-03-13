using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding.Environments;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;
using Clasp.Data.Terms.ProductValues;

namespace Clasp.Printer
{
    internal static class FormPrinter
    {
        public static string Print(Term t, bool colorize)
        {
            StringBuilder sb = new StringBuilder();
            Stack<string> indents = new Stack<string>();

            PrintFormattedTerm(t, sb, indents, colorize, RandomParenColor());

            return sb.ToString();
        }

        private static int PrintFormattedTerm(Term t, StringBuilder sb, Stack<string> indents, bool colorize, string parenColor)
        {
            if (Nil.Is(t))
            {
                return 0;
            }
            else if (t is Cons cns)
            {
                return PrintFormattedList(cns, sb, indents, colorize, parenColor);
            }
            else
            {
                return PrintFormattedValue(t, sb, colorize);
            }
        }

        private const string PAREN_LEFT = "⟦";
        private const string PAREN_RIGHT = "⟧";

        private static int PrintFormattedList(Cons cns, StringBuilder sb, Stack<string> indents, bool colorize, string parenColor)
        {
            sb.Append(ColorizeParen(PAREN_LEFT, parenColor, colorize));

            string deeperColor = _depthRotation[parenColor];

            int indentLength = PrintFormattedTerm(cns.Car, sb, indents, colorize, deeperColor);
            string newIndent = indentLength > 0
                ? new string(' ', indentLength + 2) //accounts for paren, op, and space
                : " "; //accounts only for paren

            indents.Push(newIndent);

            Term rest = cns.Cdr;

            if (rest is Cons firstLine)
            {
                sb.Append(' ');
                deeperColor = _breadthRotation[deeperColor];
                PrintFormattedTerm(firstLine.Car, sb, indents, colorize, deeperColor);
                rest = firstLine.Cdr;
            }

            while (rest is Cons anotherLine)
            {
                sb.AppendLine();
                foreach(string indent in indents)
                {
                    sb.Append(indent);
                }
                deeperColor = _breadthRotation[deeperColor];
                PrintFormattedTerm(anotherLine.Car, sb, indents, colorize, deeperColor);
                rest = anotherLine.Cdr;
            }

            if (!Nil.Is(rest))
            {
                sb.Append(" . ");
                deeperColor = _breadthRotation[deeperColor];
                PrintFormattedTerm(rest, sb, indents, colorize, deeperColor);
            }

            indents.Pop();
            sb.Append(ColorizeParen(PAREN_RIGHT, parenColor, colorize));

            return 0;
        }

        private static int PrintFormattedValue(Term t, StringBuilder sb, bool colorize)
        {
            string str = t switch
            {
                Symbol sym => sym.ToPrintedString(),
                PrimitiveProcedure pp => pp.OpSymbol.ToPrintedString(),
                _ => t.ToString()
            };

            string outStr = t switch
            {
                Symbol special when StaticEnv.CoreKeywords.Contains(special) => Colorize(str, Color.Khaki, colorize),
                PrimitiveProcedure pp => Colorize(str, Color.PeachPuff, colorize),
                CharString cStr => Colorize(str, Color.ForestGreen, colorize),
                Character c => Colorize(str, Color.SeaGreen, colorize),
                Data.Terms.Boolean b => Colorize(str, Color.Purple, colorize),
                _ => str
            };

            sb.Append(outStr);

            return str.Length;
        }

        // ------------------------

        private static string Colorize(string str, System.Drawing.Color color, bool enabled)
        {
            if (enabled)
            {
                return string.Format("\x1b[38;2;{0};{1};{2}m{3}\x1b[0m",
                    (int)color.R,
                    (int)color.G,
                    (int)color.B,
                    str);
            }
            return str;
        }

        private static string EncodeColor(Color clr) => string.Format("\x1b[38;2;{0};{1};{2}m", (int)clr.R, (int)clr.G, (int)clr.B);

        private static readonly string DARKBLUE = EncodeColor(Color.Teal);
        private static readonly string LITEBLUE = EncodeColor(Color.MediumAquamarine); 
        private static readonly string DARKORNG = EncodeColor(Color.Chocolate); 
        private static readonly string LITEORNG = EncodeColor(Color.Coral); 
        private static readonly string DARKPURP = EncodeColor(Color.Indigo);
        private static readonly string LITEPURP = EncodeColor(Color.SlateBlue); 

        private static string ColorizeParen(string paren, string code, bool enabled)
        {
            if (enabled)
            {
                return string.Format("{0}{1}\x1b[0m", code, paren);
            }
            return $"{paren}";
        }

        private static Random _rng = new Random();

        private static string RandomParenColor() => new string[]
        {
            DARKBLUE,
            LITEBLUE,
            DARKORNG,
            LITEORNG,
            DARKPURP,
            LITEPURP
        }[_rng.Next(0, 5)];

        private static readonly Dictionary<string, string> _depthRotation = new Dictionary<string, string>()
        {
            { DARKBLUE, LITEORNG },
            { DARKORNG, LITEPURP },
            { DARKPURP, LITEBLUE },
            { LITEBLUE, DARKORNG },
            { LITEORNG, DARKPURP },
            { LITEPURP, DARKBLUE },
        };

        private static readonly Dictionary<string, string> _breadthRotation = new Dictionary<string, string>()
        {
            { DARKBLUE, LITEBLUE },
            { DARKORNG, LITEORNG },
            { DARKPURP, LITEPURP },
            { LITEBLUE, DARKBLUE },
            { LITEORNG, DARKORNG },
            { LITEPURP, DARKPURP },
        };
    }
}

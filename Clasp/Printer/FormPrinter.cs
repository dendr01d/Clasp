using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using Clasp.Binding.Environments;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Procedures;
using Clasp.Data.Terms.ProductValues;

namespace Clasp.Printer
{
    internal static class FormPrinter
    {
        #region Presets

        private static readonly Color LiteRed = Color.FromArgb(0xFF, 0x88, 0x88);
        private static readonly Color LiteMgn = Color.FromArgb(0xFF, 0x88, 0xFF);
        private static readonly Color LiteBlu = Color.FromArgb(0x88, 0x88, 0xFF);
        private static readonly Color LiteCyn = Color.FromArgb(0x88, 0xFF, 0xFF);
        private static readonly Color LiteGrn = Color.FromArgb(0x88, 0xFF, 0x88);
        private static readonly Color LiteYlw = Color.FromArgb(0xFF, 0xFF, 0x88);

        private static readonly Color PureRed = Color.FromArgb(0xFF, 0x00, 0x00);
        private static readonly Color PureMgn = Color.FromArgb(0xFF, 0x00, 0xFF);
        private static readonly Color PureBlu = Color.FromArgb(0x00, 0x00, 0xFF);
        private static readonly Color PureCyn = Color.FromArgb(0x00, 0xFF, 0xFF);
        private static readonly Color PureGrn = Color.FromArgb(0x00, 0xFF, 0x00);
        private static readonly Color PureYlw = Color.FromArgb(0xFF, 0xFF, 0x00);

        private static readonly Color DarkRed = Color.FromArgb(0x88, 0x00, 0x00);
        private static readonly Color DarkMgn = Color.FromArgb(0x88, 0x00, 0x88);
        private static readonly Color DarkBlu = Color.FromArgb(0x00, 0x00, 0x88);
        private static readonly Color DarkCyn = Color.FromArgb(0x00, 0x88, 0x88);
        private static readonly Color DarkGrn = Color.FromArgb(0x00, 0x88, 0x00);
        private static readonly Color DarkYlw = Color.FromArgb(0x88, 0x88, 0x00);

        private const string PAREN_LEFT = "⌠";
        private const string PAREN_RIGHT = "⌡";
        private const string DOT_OP = "•";

        #endregion

        #region Preset Helpers

        private static readonly Random _rng = new();

        private static readonly Color[] PureColors = [PureRed, PureMgn, PureBlu, PureCyn, PureGrn, PureYlw];
        private static Color RandomParenColor() => PureColors[_rng.Next(0, 6)];

        private static Color RotateDepthwise(Color c)
        {
            return c switch
            {
                _ when c == PureRed => PureCyn,
                _ when c == PureMgn => PureGrn,
                _ when c == PureBlu => PureYlw,
                _ when c == PureCyn => PureRed,
                _ when c == PureGrn => PureMgn,
                _ when c == PureYlw => PureBlu,
                _ => Color.White
            };
        }

        private static Color RotateBreadthwise(Color c)
        {
            return c switch
            {
                _ when c == PureRed => PureGrn,
                _ when c == PureMgn => PureCyn,
                _ when c == PureBlu => PureRed,
                _ when c == PureCyn => PureYlw,
                _ when c == PureGrn => PureBlu,
                _ when c == PureYlw => PureMgn,
                _ => Color.White
            };
        }

        #endregion

        public static string Print(Term t, bool colorize)
        {
            StringBuilder sb = new();
            Stack<string> indents = new();

            PrintFormattedTerm(t, sb, indents, colorize, Color.White);

            return sb.ToString();
        }

        private static int PrintFormattedTerm(Term t, StringBuilder sb, Stack<string> indents, bool colorize, Color parenColor)
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

        private static int PrintFormattedList(Cons cns, StringBuilder sb, Stack<string> indents, bool colorize, Color parenColor)
        {
            sb.Append(Colorize(PAREN_LEFT, parenColor, colorize));

            Color deeperColor = RotateDepthwise(parenColor);

            int indentLength = PrintFormattedTerm(cns.Car, sb, indents, colorize, deeperColor);
            string newIndent = indentLength > 0
                ? new string(' ', indentLength + 2) //accounts for paren, op, and space
                : " "; //accounts only for paren

            indents.Push(newIndent);

            Term rest = cns.Cdr;

            if (rest is Cons firstLine)
            {
                sb.Append(' ');
                deeperColor = RotateBreadthwise(deeperColor);
                PrintFormattedTerm(firstLine.Car, sb, indents, colorize, deeperColor);
                rest = firstLine.Cdr;
            }

            while (rest is Cons anotherLine)
            {
                sb.AppendLine();
                foreach (string indent in indents)
                {
                    sb.Append(indent);
                }
                deeperColor = RotateBreadthwise(deeperColor);
                PrintFormattedTerm(anotherLine.Car, sb, indents, colorize, deeperColor);
                rest = anotherLine.Cdr;
            }

            if (!Nil.Is(rest))
            {
                sb.Append(' ');
                sb.Append(Colorize(DOT_OP, parenColor, colorize));
                sb.Append(' ');

                deeperColor = RotateBreadthwise(deeperColor);
                PrintFormattedTerm(rest, sb, indents, colorize, deeperColor);
            }

            indents.Pop();
            sb.Append(Colorize(PAREN_RIGHT, parenColor, colorize));

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
                Symbol special when StaticEnv.CoreKeywords.Contains(special) => Colorize(str, LiteYlw, colorize),
                PrimitiveProcedure pp => Colorize(str, LiteRed, colorize),
                RefString cStr => Colorize(str, LiteGrn, colorize),
                Character c => Colorize(str, LiteCyn, colorize),
                Data.Terms.Boolean b => Colorize(str, LiteBlu, colorize),
                _ => str
            };

            sb.Append(outStr);

            return str.Length;
        }

        // ------------------------

        private static string Colorize(string str, Color color, bool enabled)
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
    }
}

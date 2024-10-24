using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal static class PrettyPrinter
    {
        private const string BLACK = "\x1B[30m";
        private const string RED = "\x1B[31m"; //nil
        private const string GREEN = "\x1B[32m"; //literals
        private const string YELLOW = "\x1B[33m"; //parens
        private const string BLUE = "\x1B[34m"; //procedures
        private const string MAGENTA = "\x1B[35m"; //symbols
        private const string CYAN = "\x1B[36m"; //special form
        private const string WHITE = "\x1B[37m";
        private const string DEFAULT = "\x1B[39m";

        public static string PrettyPrint(this Expression expr)
        {
            return PrettyPrint(expr, 0) + DEFAULT;
        }

        private static string PrettyPrint(Expression expr, int margin)
        {
            if (expr.IsNil)
            {
                return RED + expr.Serialize();
            }
            else if (expr.IsAtom)
            {
                string color = expr switch
                {
                    Symbol => CYAN,
                    _ => GREEN
                };

                return color + expr.Serialize();
            }
            else
            {
                IEnumerable<string> elements = Pair.Enumerate(expr).Select(x => PrettyPrint(x, margin));


            }


            string raw = expr.Serialize();

            if (!expr.IsPair || raw.Length <= 20)
            {
                return raw;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append('(');
                sb.Append(expr.Car.Serialize());

                if (expr.Cdr.IsAtom)
                {
                    sb.Append(" . ");
                    sb.Append(expr.Cdr.PrettyPrint(sb.Length));
                }
                else if (!expr.Cdr.IsNil)
                {
                    foreach (Expression e in Pair.Enumerate(expr.Cdr))
                    {
                        sb.Append(System.Environment.NewLine);
                        sb.Append(' ', margin + 2);
                        sb.Append(e.PrettyPrint(margin + 2));
                    }
                }

                sb.Append(')');
                return sb.ToString();
            }
        }

    }
}

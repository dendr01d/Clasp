using System.Text;

using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler
{
    internal static class TextWriterExtensions
    {
        public static void WriteIndenting(this TextWriter writer, string indentingText, ref int indent)
        {
            writer.Write(indentingText);
            indent += indentingText.Length;
        }

        public static void WriteIndenting(this TextWriter writer, char indentingChar, ref int indent)
            => writer.WriteIndenting(indentingChar.ToString(), ref indent);

        public static void WriteIndenting(this TextWriter writer, IPrintable term, ref int indent)
        {
            StringBuilder sb = new();
            TextWriter temp = new StringWriter(sb);

            term.Print(temp, indent);

            string output = sb.ToString();
            indent += output.TakeWhile(x => !char.IsWhiteSpace(x)).Count();
            writer.Write(output);
        }

        public static void Indent(this TextWriter writer, int indentLength)
            => writer.Write(new string(' ', indentLength));

        public static void WriteLineIndent(this TextWriter writer, string line, int indentLength)
        {
            writer.WriteLine(line);
            writer.Indent(indentLength);
        }

        public static void WriteLineIndent(this TextWriter writer, IPrintable term, int indent)
        {
            Write(writer, term, indent);
            WriteLineIndent(writer, indent);
        }

        public static void WriteLineIndent(this TextWriter writer, int indentLength)
            => writer.WriteLineIndent(string.Empty, indentLength);

        public static void Write(this TextWriter writer, IPrintable term, int indent)
            => term.Print(writer, indent);

        public static void WriteLineByLine<T>(this TextWriter writer, IEnumerable<T> items, Action<TextWriter, T, int> printer, int indent)
        {
            if (!items.Any()) return;

            printer.Invoke(writer, items.First(), indent);

            foreach (T item in items.Skip(1))
            {
                writer.WriteLineIndent(indent);
                printer.Invoke(writer, item, indent);
            }
        }

        public static void WriteLineByLine(this TextWriter writer, IEnumerable<IPrintable> items, int indent)
            => WriteLineByLine(writer, items, Write, indent);

        #region Cons

        public static void WriteCons<T>(this TextWriter writer, ICons<T> cns, int indent)
            where T : ISchemeExp
        {
            writer.WriteIndenting('(', ref indent);
            writer.Write(cns.Car, indent);
            WriteCdr(writer, cns.Cdr, indent);
            writer.Write(')');
        }

        private static void WriteCdr<T>(TextWriter writer, T term, int indent)
            where T : ISchemeExp
        {
            if (!term.IsNil)
            {
                writer.WriteLineIndent(indent);

                if (term is ICons<T> cns)
                {
                    writer.Write(cns.Car, indent);
                    WriteCdr(writer, cns.Cdr, indent);
                }
                else
                {
                    writer.Write(term);
                }
            }
        }

        #endregion

        #region Application Form

        public static void WriteApplication(this TextWriter writer, string op, IEnumerable<IPrintable> args, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            writer.WriteIndenting(op, ref indent);
            WriteArgs(writer, args, op.Contains(Environment.NewLine), indent);
            writer.Write(')');
        }

        public static void WriteApplication(this TextWriter writer, IPrintable op, IEnumerable<IPrintable> args, int indent)
        {
            writer.WriteIndenting('(', ref indent);
            writer.WriteIndenting(op, ref indent);
            WriteArgs(writer, args, op.BreaksLine, indent);
            writer.Write(')');
        }

        private static void WriteArgs(TextWriter writer, IEnumerable<IPrintable> args, bool opBreaksLine, int indent)
        {
            if (args.Any())
            {
                if (args.Any(x => x.BreaksLine))
                {
                    if (opBreaksLine)
                    {
                        indent += 2;
                        writer.WriteLineIndent(indent);
                    }

                    writer.WriteIndenting(' ', ref indent);
                    writer.Write(args.First(), indent);

                    foreach (IPrintable arg in args.Skip(1))
                    {
                        writer.WriteLineIndent(indent);
                        writer.Write(arg, indent);
                    }
                }
                else
                {
                    writer.Write(' ');
                    writer.Write(string.Join(' ', args));
                }
            }
        }

        #endregion
    }
}

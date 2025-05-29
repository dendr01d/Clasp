using System.Text;

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
        {
            writer.Write(indentingChar);
            indent += 1;
        }

        public static void Indent(this TextWriter writer, int indent)
        {
            writer.Write(new string(' ', indent));
        }

        public static void WriteLineIndent(this TextWriter writer, string line, int indent)
        {
            writer.WriteLine(line);
            Indent(writer, indent);
        }

        public static void WriteLineIndent(this TextWriter writer, int indent)
            => WriteLineIndent(writer, string.Empty, indent);

        public static void Write(this TextWriter writer, IPrintable term, int indent)
        {
            term.Print(writer, indent);
        }

        public static void WriteIndenting(this TextWriter writer, IPrintable term, ref int indent)
        {
            StringBuilder sb = new StringBuilder();
            TextWriter temp = new StringWriter(sb);

            term.Print(temp, indent);

            string output = sb.ToString();
            indent += output.Split(Environment.NewLine).FirstOrDefault()?.Length ?? 0;
            writer.Write(output);
        }

        public static void WriteWithComment(this TextWriter writer, IPrintable term, int paddedWidth, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                writer.Write(term.ToString());
            }
            else
            {
                string format = $"{{0,{paddedWidth}}}{{1}}";
                writer.Write(format, term.ToString(), comment);
            }
        }

        public static void WriteApplication(this TextWriter writer, string op, IPrintable[] args, int indent)
        {
            WriteIndenting(writer, '(', ref indent);
            WriteIndenting(writer, op, ref indent);

            if (args.Length > 0)
            {
                if (args.All(x => !x.CanBreak))
                {
                    foreach (IPrintable arg in args)
                    {
                        writer.Write(' ');
                        arg.Print(writer, indent);
                    }
                }
                else
                {
                    writer.WriteIndenting(' ', ref indent);
                    args[0].Print(writer, indent);

                    foreach (IPrintable arg in args.Skip(1))
                    {
                        writer.WriteLineIndent(indent);
                        arg.Print(writer, indent);
                    }
                }
            }

            writer.Write(')');
        }

        public static void WriteApplication(this TextWriter writer, IPrintable op, IPrintable[] args, int indent)
            => WriteApplication(writer, op.ToString(), args, indent);
    }
}

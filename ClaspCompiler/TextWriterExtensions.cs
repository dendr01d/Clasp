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
    }
}

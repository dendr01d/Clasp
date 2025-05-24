
namespace ClaspCompiler.Textual
{
    internal sealed class SourceRef : IPrintable
    {
        /// <summary>Source of the text (file path or otherwise)</summary>
        public string Name { get; init; }
        /// <summary>Full text to which this source refers</summary>
        public string Text { get; init; }

        /// <summary>Line number of position within <see cref="Text"/></summary>
        public int LineNumber { get; init; }
        /// <summary>Position within line in <see cref="Text"/></summary>
        public int Column { get; init; }

        /// <summary>Character index within <see cref="Text"/></summary>
        public int StartIndex { get; init; }
        /// <summary>Character span within <see cref="Text"/></summary>
        public int Length { get; init; }

        public SourceRef(string name, string text,
            int line, int column, int index, int length)
        {
            Name = name;
            Text = text;
            LineNumber = line;
            Column = column;
            StartIndex = index;
            Length = length;
        }

        /// <summary>Extract the source snippet of the <see cref="Text"/> to which this source refers.</summary>
        public ReadOnlySpan<char> GetSnippet()
        {
            return Text.AsSpan().Slice(Column, Length);
        }

        public override string ToString() => Text;
        public void Print(TextWriter writer, int indent) => writer.Write(Text);
    }
}

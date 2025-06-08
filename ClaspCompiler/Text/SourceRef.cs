
using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.Textual
{
    internal sealed record SourceRef : IPrintable
    {
        /// <summary>Source of the text (file path or otherwise)</summary>
        public string SourceName { get; init; }

        /// <summary>Line number of position within the source</summary>
        public int Line { get; init; }
        /// <summary>Position within line in the source</summary>
        public int Column { get; init; }

        /// <summary>Character index within the source</summary>
        public int Index { get; init; }
        /// <summary>Character span within the source</summary>
        public int Length { get; init; }

        public SourceRef(string name = "?",
            int line = -1, int column = -1, int index = 0, int length = 0)
        {
            SourceName = name;
            Line = line;
            Column = column;
            Index = index;
            Length = length;
        }

        public SourceRef Merge(SourceRef other)
        {
            if (SourceName != other.SourceName)
            {
                throw new Exception("Can't merge source references to different sources.");
            }

            bool thisSourceLeads = Index >= other.Index;

            return new SourceRef(SourceName)
            {
                Line = thisSourceLeads ? Line : other.Line,
                Column = thisSourceLeads ? Column : other.Column,
                Index = thisSourceLeads ? Index : other.Index,
                Length = thisSourceLeads
                    ? other.Index + other.Length - Index
                    : Index + Length - other.Index
            };
        }

        public bool BreaksLine => false;
        public string AsString => $"({SourceName}: Line {Line}, Col {Column} (Index {Index}) Length = {Length})";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}

namespace ClaspCompiler.Text
{
    internal sealed record SourceRef(AcquisitionMethod Acquisition, int Line, int Column, int Index, int Length) : IPrintable
    {
        public static SourceRef DefaultSyntax = new(CoreDefinition.Instance, 0, 0, 0, 0);

        public readonly bool Original = Acquisition is ReadFromFile;

        public SourceRef MergeWith(SourceRef other)
        {
            if (Acquisition != other.Acquisition)
            {
                throw new Exception($"Can't merge source references from different acquisition methods.");
            }

            Tuple<SourceRef, SourceRef> range = Index <= other.Index
                ? new(this, other)
                : new(other, this);

            int length = (range.Item2.Index + range.Item2.Length) - range.Item1.Index;

            return new SourceRef(Acquisition, range.Item1.Line, range.Item1.Column, range.Item1.Index, length);
        }

        public bool BreaksLine => false;
        public string AsString => $"{Acquisition} @ line {Line}, col {Column}";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public sealed override string ToString() => AsString;
    }
}

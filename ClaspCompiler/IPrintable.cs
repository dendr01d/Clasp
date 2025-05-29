using System.Diagnostics.CodeAnalysis;

namespace ClaspCompiler
{
    internal interface IPrintable
    {
        public bool CanBreak { get; }
        [MemberNotNull]
        public string ToString();
        public void Print(TextWriter writer, int indent);
    }
}

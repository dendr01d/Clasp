using System.Diagnostics.CodeAnalysis;

namespace ClaspCompiler
{
    internal interface IPrintable
    {
        [MemberNotNull]
        public string ToString();
        public void Print(TextWriter writer, int indent);
    }
}

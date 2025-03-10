
namespace Clasp.Data.Terms
{
    internal sealed class CharString : Term
    {
        public readonly string Value;

        public CharString(string s) => Value = s;
        public override string ToString() => string.Format("\"{0}\"", System.Uri.UnescapeDataString(Value));
        public override string ToPrintedString() => Value;
        protected override string FormatType() => "String";
    }
}

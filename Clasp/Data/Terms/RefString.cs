
using System;

namespace Clasp.Data.Terms
{
    internal struct RefString : ITerm, IEquatable<RefString>
    {
        public readonly string Value;

        public RefString(string s) => Value = s;
        public bool Equals(RefString other) => Value == other.Value;
        public bool Equals(ITerm? other) => other is RefString str && Equals(str);
        public override bool Equals(object? other) => other is RefString str && Equals(str);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => string.Format("\"{0}\"", Value);
    }
}

using System;

namespace Clasp.Data.Terms
{
    internal readonly struct FixNum : ITerm, IEquatable<FixNum>
    {
        public readonly int Value;

        public FixNum(int value) => Value = value;
        public bool Equals(FixNum other) => Value == other.Value;
        public bool Equals(ITerm? other) => other is FixNum fix && Equals(fix);
        public override bool Equals(object? other) => other is FixNum fix && Equals(fix);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }
}

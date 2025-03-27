using System;

namespace Clasp.Data.Terms
{
    internal readonly struct FloNum : ITerm, IEquatable<FloNum>
    {
        public readonly double Value;

        public FloNum(double value) => Value = value;
        public bool Equals(FloNum other) => Value == other.Value;
        public bool Equals(ITerm? other) => other is FloNum flo && Equals(flo);
        public override bool Equals(object? other) => other is FloNum flo && Equals(flo);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }
}

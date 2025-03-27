using System;

namespace Clasp.Data.Terms
{
    internal readonly struct BoolTrue : ITerm, IEquatable<BoolTrue>
    {
        public BoolTrue() { }
        public bool Equals(BoolTrue other) => true;
        public bool Equals(ITerm? other) => other is BoolTrue;
        public override bool Equals(object? other) => other is BoolTrue;
        public override int GetHashCode() => 1;
        public override string ToString() => "#t";
    }
}

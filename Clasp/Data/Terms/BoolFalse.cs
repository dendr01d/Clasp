using System;

namespace Clasp.Data.Terms
{
    internal readonly struct BoolFalse : ITerm, IEquatable<BoolFalse>
    {
        public BoolFalse() { }
        public bool Equals(BoolFalse other) => true;
        public bool Equals(ITerm? other) => other is BoolFalse;
        public override bool Equals(object? other) => other is BoolFalse;
        public override int GetHashCode() => 0;
        public override string ToString() => "#f";
    }
}

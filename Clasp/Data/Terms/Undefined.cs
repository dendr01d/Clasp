using System;

namespace Clasp.Data.Terms
{
    internal readonly struct Undefined : ITerm, IEquatable<Undefined>
    {
        public Undefined() { }
        public bool Equals(Undefined other) => true;
        public bool Equals(ITerm? other) => other is Undefined;
        public override bool Equals(object? other) => other is Undefined;
        public override int GetHashCode() => 0;
        public override string ToString() => "#<undefined>";
    }
}

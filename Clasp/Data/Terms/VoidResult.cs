using System;

namespace Clasp.Data.Terms
{
    internal readonly struct VoidResult : ITerm, IEquatable<VoidResult>
    {
        public VoidResult() { }
        public bool Equals(VoidResult other) => true;
        public bool Equals(ITerm? other) => other is VoidResult;
        public override bool Equals(object? other) => other is VoidResult;
        public override int GetHashCode() => 0;
        public override string ToString() => "#<void>";
    }
}

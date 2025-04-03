using System;

namespace Clasp.Data.Terms
{
    internal readonly struct Nil : ITerm, IEquatable<Nil>
    {
        public Nil() { }
        
        public bool Equals(Nil other) => true;
        public bool Equals(ITerm? other) => other is Nil;
        public override bool Equals(object? other) => other is Nil;
        public override int GetHashCode() => 0;
        public override string ToString() => "()";
    }
}

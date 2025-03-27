using System;

namespace Clasp.Data.Terms
{
    internal readonly struct Boole : ITerm, IEquatable<Boole>
    {
        public readonly bool Value;

        public Boole(bool value) => Value = value;
        public bool Equals(Boole other) => Value == other.Value;
        public bool Equals(ITerm? other) => other is Boole b && Equals(b);
        public override bool Equals(object? other) => other is Boole b && Equals(b);
        public override int GetHashCode() => Value ? 1 : 0;
        public override string ToString() => Value ? "#t" : "#f";
    }
}

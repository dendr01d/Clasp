using System;

namespace Clasp.Data.Terms
{
    internal sealed class Box : ITerm, IEquatable<Box>
    {
        public ITerm Value { get; private set; }

        public Box(ITerm value) => Value = value;

        public void Mutate(ITerm value) => Value = value;

        public bool Equals(Box? other) => ReferenceEquals(Value, other?.Value);
        public bool Equals(ITerm? other) => other is Box box && Equals(box);
        public override bool Equals(object? other) => other is Box box && Equals(box);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"π{Value}";
    }
}

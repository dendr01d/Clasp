using System;

namespace Clasp.Data.Terms
{
    internal readonly struct Vector : ITerm, IEquatable<Vector>
    {
        private readonly ITerm[] _values;

        public Vector(params ITerm[] values) => _values = values;

        public bool Equals(Vector other) => ReferenceEquals(_values, other._values);
        public bool Equals(ITerm? other) => other is Vector vec && Equals(vec);
        public override bool Equals(object? other) => other is Vector vec && Equals(vec);
        public override int GetHashCode() => _values.GetHashCode();
        public override string ToString() => $"#({String.Join(" ", _values as object?[])})";
    }
}

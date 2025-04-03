using System;
using System.IO;

namespace Clasp.Data.Terms
{
    internal readonly struct Port : ITerm, IEquatable<Port>
    {
        public readonly string Name;
        public readonly Stream Value;

        public Port(string name, Stream value)
        {
            Name = name;
            Value = value;
        }
        
        public bool Equals(Port other) => Value == other.Value;
        public bool Equals(ITerm? other) => other is Port prt && Equals(prt);
        public override bool Equals(object? other) => other is FloNum prt && Equals(prt);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"{{{Name}}}";
    }
}

using System;

using Clasp.Data.Text;

namespace Clasp.Data.Terms
{
    internal readonly struct StxId : ISyntax
    {
        public readonly Symbol SymbolicName;
        public readonly SourceCode Location { get; }

        public StxId(Symbol symbolicName, SourceCode location)
        {
            SymbolicName = symbolicName;
            Location = location;
        }

        public bool Equals(StxId other) => SymbolicName.Equals(other.SymbolicName) && Location.Equals(other.Location);
        public bool Equals(ITerm? other) => other is StxId id && Equals(id);
        public override bool Equals(object? other) => other is StxId id && Equals(id);
        public override int GetHashCode() => HashCode.Combine(SymbolicName, Location);
        public override string ToString() => SymbolicName.ToString();
    }
}

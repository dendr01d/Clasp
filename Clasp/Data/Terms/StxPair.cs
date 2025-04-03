using System;

using Clasp.Data.Text;

namespace Clasp.Data.Terms
{
    internal readonly struct StxPair : ISyntax
    {
        public readonly Cons Pair;
        public readonly SourceCode Location { get; }

        public StxPair(ISyntax car, ISyntax cdr, SourceCode location)
        {
            Pair = new Cons(car, cdr);
            Location = location;
        }

        public bool Equals(StxPair other) => Pair.Equals(other.Pair) && Location.Equals(other.Location);
        public bool Equals(ITerm? other) => other is StxPair stp && Equals(stp);
        public override bool Equals(object? other) => other is StxPair stp && Equals(stp);
        public override int GetHashCode() => HashCode.Combine(Pair, Location);
        public override string ToString() => Pair.ToString();
    }
}

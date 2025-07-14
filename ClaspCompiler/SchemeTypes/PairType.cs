using System.Collections;
using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.SchemeTypes
{
    internal sealed record PairType(SchemeType Car, SchemeType Cdr) : SchemeType, ICons<SchemeType>
    {
        private readonly Lazy<int> _lazyHash = new(() => HashCode.Combine(Car, Cdr));

        public bool Equals(PairType? other) => other is not null && Car.Equals(other.Car) && Cdr.Equals(other.Cdr);
        public override int GetHashCode() => _lazyHash.Value;

        public override string AsString => $"(Pair {Car} {Cdr})";

        public IEnumerator<SchemeType> GetEnumerator() => this.Enumerate();
        IEnumerator IEnumerable.GetEnumerator() => this.Enumerate();
    }
}
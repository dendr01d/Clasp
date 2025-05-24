using System;

namespace Clasp.Data.Terms
{
    internal readonly struct Cons : ITerm, IEquatable<Cons>
    {
        private readonly Box _car;
        private readonly Box _cdr;

        public ITerm Car => _car.Value;
        public ITerm Cdr => _cdr.Value;

        public Cons(ITerm car, ITerm cdr)
        {
            _car = new Box(car);
            _cdr = new Box(cdr);
        }

        public void SetCar(ITerm car) => _car.Mutate(car);
        public void SetCdr(ITerm cdr) => _cdr.Mutate(cdr);

        public static ITerm List(ITerm first, params ITerm[] rest)
        {
            return (rest.Length > 0)
                ? new Cons(first, List(rest[0], rest[1..]))
                : first;
        }

        public bool Equals(Cons other) => Car.Equals(other.Car) && Cdr.Equals(other.Cdr);
        public bool Equals(ITerm? other) => other is Cons cns && Equals(cns);
        public override bool Equals(object? other) => other is Cons cns && Equals(cns);
        public override int GetHashCode() => HashCode.Combine(_car, _cdr);
        public override string ToString() => $"({Car}{PrintAsTail(Cdr)})";

        private static string PrintAsTail(ITerm term)
        {
            return term switch
            {
                Nil => string.Empty,
                Cons cns => $"{cns.Car} {PrintAsTail(cns.Cdr)}",
                StxPair stp => $"{stp.Pair.Car} {PrintAsTail(stp.Pair.Cdr)}",
                _ => $" . {term}"
            };
        }
    }
}

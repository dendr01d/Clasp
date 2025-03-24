using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine.Terms
{
    internal struct Cons : ITerm, IEquatable<Cons>
        where T1 : ITerm
        where T2 : ITerm
    {
        public T1 Car { get; private set; }
        public T2 Cdr { get; private set; }

        public Cons(T1 car, T2 cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        public void SetCar(T1 car) => Car = car;
        public void SetCdr(T2 cdr) => Cdr = cdr;

        public bool Equals(Cons<T1, T2> other) => Car.Equals(other.Car) && Cdr.Equals(other.Cdr);
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Cons<T1, T2> cns && Equals(cns);
        public override int GetHashCode() => HashCode.Combine(Car, Cdr);
    }
}

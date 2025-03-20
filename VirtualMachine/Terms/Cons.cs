using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachine.Terms
{
    internal class Cons : ITerm, IEquatable<Cons>
    {
        public ITerm Car;
        public ITerm Cdr;

        public Cons(ITerm car, ITerm cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        public void SetCar(ITerm car) => Car = car;
        public void SetCdr(ITerm cdr) => Cdr = cdr;

        public byte[] GetBytes() => [];

        public bool Equals(Cons? other)
        {
            return other is not null
                && Car.Equals(other.Car)
                && Cdr.Equals(other.Cdr);
        }

        public bool Equals(ITerm? other) => other is Cons cns && Equals(cns);

        public override bool Equals(object? obj) => obj is Cons cns && Equals(cns);

        public override int GetHashCode()
        {
            return HashCode.Combine(Car, Cdr);
        }
    }
}

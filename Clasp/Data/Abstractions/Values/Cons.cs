using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Abstractions.Values
{
    internal readonly struct Cons : IAbstractValue
    {
        public readonly Box Car;
        public readonly Box Cdr;

        public Cons(IAbstractValue car, IAbstractValue cdr)
        {
            Car = new Box(car);
            Cdr = new Box(cdr);
        }

        public void SetCar(IAbstractValue car) => Car.MutateValue(car);
        public void SetCdr(IAbstractValue cdr) => Cdr.MutateValue(cdr);
    }
}

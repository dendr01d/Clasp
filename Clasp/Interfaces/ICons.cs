using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;

namespace Clasp.Interfaces
{
    internal interface ICons<T1, T2> : IEnumerable<T1?>
        where T1 : Term
        where T2 : Term
    {
        public T1 Car { get; }
        public T2 Cdr { get; }

        public bool IsDotted { get; }

        public void SetCar(T1 newCar);
        public void SetCdr(T2 newCdr);
    }
}

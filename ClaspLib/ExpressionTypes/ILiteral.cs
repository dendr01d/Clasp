using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib
{
    public interface ILiteral<T> : IAtom
        where T : struct
    {
        public T Value { get; }
    }
}

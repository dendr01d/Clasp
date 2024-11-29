using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib
{
    public interface INumComplex : INumber
    {
        public INumReal RealPart { get; }
        public INumReal ImaginaryPart { get; }
    }
}

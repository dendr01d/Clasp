using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib.ExpressionTypes
{
    public interface INumRational : INumReal
    {
        public INumInteger Numerator { get; }
        public INumInteger Denominator { get; }
    }
}

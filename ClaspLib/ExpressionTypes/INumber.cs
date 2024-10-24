using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib.ExpressionTypes
{
    public interface INumber : IAtom
    {
        public bool IsExact { get; }

        public INumber Add(INumber num);
        public INumber Subtract(INumber num);
        public INumber Multiply(INumber num);
        public INumber Divide(INumber num);

        public INumber Quotient(INumber num);
        public INumber Modulo(INumber num);
        public INumber Remainder(INumber num);

        public INumber Exponent(INumber num);

        public bool NumEq(INumber num);
        public bool NumLt(INumber num);
        public bool NumGt(INumber num);
        public bool NumLeq(INumber num);
        public bool NumGeq(INumber num);

        public INumber Abs();
        public INumber Truncate();
        public INumber Ceiling();
        public INumber Floor();
        public INumber Round();
    }

    public static class NumberExtensions
    {

    }
}

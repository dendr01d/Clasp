using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal class Vector : Atom
    {
        private readonly Expression[] _data;

        protected Vector(params Expression[] data)
        {
            _data = data;
        }

        public static Vector MkVector(params Expression[] data)
        {
            return new Vector(data);
        }

        public static Expression Ref(Vector vec, int index)
        {
            return vec._data[index];
        }

        public static void Set(Vector vec, int index, Expression obj)
        {
            vec._data[index] = obj;
        }

        public override bool IsAtom => false;
        public override Expression Car => throw new ExpectedTypeException<Pair>(this);
        public override Expression Cdr => throw new ExpectedTypeException<Pair>(this);
        public override Expression SetCar(Expression expr) => throw new ExpectedTypeException<Pair>(this);
        public override Expression SetCdr(Expression expr) => throw new ExpectedTypeException<Pair>(this);

        public override string ToPrinted() => ToSerialized();
        public override string ToSerialized()
        {
            return $"#({string.Join<Expression>(' ', _data)})";
        }
    }
}

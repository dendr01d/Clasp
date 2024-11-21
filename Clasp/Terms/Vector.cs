using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal class Vector : Atom
    {
        private readonly List<Expression> _data;

        public Vector(params Expression[] data)
        {
            _data = data.ToList();
        }

        public static Expression Ref(Vector vec, int index)
        {
            return vec._data[index];
        }

        public static void Set(Vector vec, int index, Expression obj)
        {
            vec._data[index] = obj;
        }

        public static void Add(Vector vec, Expression obj)
        {
            vec._data.Add(obj);
        }

        public bool VecEquals(Vector other)
        {
            if (_data.Count != other._data.Count)
            {
                return false;
            }

            for (int i = 0; i < _data.Count; ++i)
            {
                if (_data[i].Pred_Equal(other._data[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool IsAtom => false;

        public override string Display() => "[" + string.Join(", ", _data.Select(x => x.Display())) + "]";
        public override string Write()
        {
            return "#(" + string.Join(' ', _data.Select(x => x.Write())) + ")";
        }
    }
}

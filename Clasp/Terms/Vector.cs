using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    //internal class Vector : Atom
    //{
    //    private readonly Expression[] _data;

    //    public Expression[] Data => _data;
    //    public IEnumerable<Expression> EnumerableData => _data.AsEnumerable();

    //    protected Vector(params Expression[] data)
    //    {
    //        _data = data;
    //    }

    //    public static Vector MkVector(params Expression[] data)
    //    {
    //        return new Vector(data);
    //    }

    //    public static Expression Ref(Vector vec, int index)
    //    {
    //        return vec._data[index];
    //    }

    //    public static void Set(Vector vec, int index, Expression obj)
    //    {
    //        vec._data[index] = obj;
    //    }

    //    public override bool IsAtom => false;

    //    public override Expression Deconstruct() => this;

    //    public override string Print() => Serialize();
    //    public override string Serialize()
    //    {
    //        return $"#({string.Join<Expression>(' ', _data)})";
    //    }
    //}
}

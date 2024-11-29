using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib
{
    public interface IProcPrimitive : IProcedure, IAtom
    {
        public int Arity { get; }
        public IExpression Apply(IEnumerable<IExpression> args);
    }
}

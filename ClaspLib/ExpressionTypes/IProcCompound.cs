using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib.ExpressionTypes
{
    public interface IProcCompound : IProcedure, IComposite
    {
        public IEnvironment Closure { get; }
        public IExpression Parameters { get; }
        public IExpression Body { get; }
    }
}

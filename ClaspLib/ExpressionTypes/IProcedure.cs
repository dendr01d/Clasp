using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib.ExpressionTypes
{
    public interface IProcedure : IExpression
    {
        public bool ApplicativeOrder { get; }
    }
}

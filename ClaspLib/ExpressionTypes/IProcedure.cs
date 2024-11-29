using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib
{
    public interface IProcedure : IExpression
    {
        public bool ApplicativeOrder { get; }
    }
}

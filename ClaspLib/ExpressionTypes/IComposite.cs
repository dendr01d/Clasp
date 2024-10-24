using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib.ExpressionTypes
{
    public interface IComposite : IExpression
    {
        /// <summary>
        /// Breaks down compiled expression types into their equivalent expression as a composite object.
        /// </summary>
        public IComposite Decompose();
    }
}

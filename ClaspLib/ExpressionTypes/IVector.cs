using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib.ExpressionTypes
{
    public interface IVector : IComposite
    {
        public int Length { get; }

        public IExpression AtIndex(int i);

        public void SetIndex(int i, IExpression value);
    }
}

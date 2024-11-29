using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib
{
    public interface IPair : IComposite
    {
        public IExpression Car { get; }
        public IExpression Cdr { get; }

        public void SetCar(IExpression value);
        public void SetCdr(IExpression value);
    }
}

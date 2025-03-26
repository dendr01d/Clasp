using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Abstractions.Values
{
    internal sealed class Box: IAbstractValue
    {
        public IAbstractValue Value { get; private set; }

        public Box(IAbstractValue value)
        {
            if (value is Box b)
            {
                Value = b.Value;
            }
            else
            {
                Value = value;
            }
        }

        public void MutateValue(IAbstractValue value)
        {
            Value = value;
        }
    }
}

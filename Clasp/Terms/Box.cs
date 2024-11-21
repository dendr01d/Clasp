using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal abstract class Box<T> : Expression
    {
        public readonly T BoxedValue;

        public Box(T obj) => BoxedValue = obj;

        public override bool IsAtom => true;
        public override string Display() => string.Format("[{0}: {1}]", typeof(T).Name, BoxedValue?.ToString() ?? "NULL");
        public override string Write() => Display();
    }
}

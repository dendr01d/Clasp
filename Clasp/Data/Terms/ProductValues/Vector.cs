using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Terms.ProductValues
{
    internal sealed class Vector : Term
    {
        public readonly Term[] Values;
        public Vector(params Term[] values) => Values = values;

        public override string ToString() => string.Format(
            "#({0})",
            string.Format(", ", Values.ToArray<object>()));

        protected override string FormatType() => string.Format("Vec<{0}>", string.Join(", ", Values.Select(x => x.TypeName)));
    }
}

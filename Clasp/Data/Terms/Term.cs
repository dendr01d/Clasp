using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.AbstractSyntax;

namespace Clasp.Data.Terms
{
    internal abstract class Term
    {
        private Lazy<string> _typeName;
        public string TypeName => _typeName.Value;

        protected Term()
        {
            _typeName = new Lazy<string>(FormatType);
        }

        public abstract override string ToString();

        protected abstract string FormatType();
    }
}

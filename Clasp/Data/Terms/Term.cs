using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public virtual string ToTermString() => ToString();
        protected abstract string FormatType();

        #region Implicit Booleans

        public static implicit operator bool(Term t) => t != Boolean.False;
        public static implicit operator Term(bool b) => b ? Boolean.True : Boolean.False;

        #endregion
    }
}

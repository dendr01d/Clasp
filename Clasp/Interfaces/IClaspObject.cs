using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Interfaces
{
    internal interface IClaspObject
    {
        public string ToString();
        public Data.Terms.Term ToTerm();
        public Data.Terms.Syntax ToSyntax();
        public IEnumerable<Data.Text.Token> ToTokens();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Binding
{
    internal class ScopeTokenGenerator
    {
        public const uint NullToken = 0;
        private const uint FirstToken = NullToken + 1;

        private uint _tokenCounter;

        public ScopeTokenGenerator()
        {
            _tokenCounter = FirstToken;
        }

        public uint FreshToken() => _tokenCounter++;
    }
}

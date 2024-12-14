using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Binding
{
    internal class ScopeTokenGenerator
    {
        public const uint TopToken = 0;
        private const uint FirstToken = TopToken + 1;

        private uint _tokenCounter;

        public ScopeTokenGenerator()
        {
            _tokenCounter = FirstToken;
        }

        public uint FreshToken() => _tokenCounter++;
    }
}

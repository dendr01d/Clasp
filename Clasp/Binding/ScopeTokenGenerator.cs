using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Binding
{
    internal class ScopeTokenGenerator
    {
        public const ScopeToken NullToken = 0;
        private const ScopeToken FirstToken = NullToken + 1;

        private ScopeToken _tokenCounter;

        public ScopeTokenGenerator()
        {
            _tokenCounter = FirstToken;
        }

        public ScopeToken FreshToken() => _tokenCounter++;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Binding.Scopes
{
    internal class ScopeFactory
    {
        private readonly ScopeTokenGenerator _tokenGen;

        public ScopeFactory(ScopeTokenGenerator gen)
        {
            _tokenGen = gen;
        }

        public Scope NewScope() => new Scope(_tokenGen.FreshToken());

        public Scope NewScopeInside(Scope s) => new Scope(_tokenGen.FreshToken(), s);
    }
}

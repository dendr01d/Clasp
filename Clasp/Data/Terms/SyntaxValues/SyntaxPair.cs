using System;
using System.Collections.Generic;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Text;

namespace Clasp.Data.Terms.SyntaxValues
{
    /// <summary>
    /// A syntactic pair of Clasp objects implicitly expected to be syntactic objects themselves
    /// </summary>
    /// <remarks>
    /// Primarily exists to enable lazy scope manipulation over lists.
    /// </remarks>
    internal sealed class SyntaxPair : Syntax
    {
        private readonly ScopeSet _scopes;
        List<Action<Syntax>> _lazyOps;

        private Syntax _car;
        private Syntax _cdr;
        private Cons _cons;

        public SyntaxPair(Term car, Term cdr, SourceCode loc) : base(loc)
        {
            _scopes = new ScopeSet();
            _lazyOps = new List<Action<Syntax>>();

            _car = WrapRaw(car);
            _cdr = WrapRaw(cdr);
            _cons = Cons.Truct(_car, _cdr);
        }

        public override Cons Expose()
        {
            LazilyOperate();
            return _cons;
        }

        private void LazilyOperate()
        {
            // If there are any pending scope operations, apply them all and reset the operator
            if (_lazyOps.Count > 0)
            {
                foreach (var op in _lazyOps)
                {
                    op(_car);
                    op(_cdr);
                }
                _lazyOps = new List<Action<Syntax>>();
            }
        }

        #region Scope-Adjustment

        public override void AddScope(int phase, params Scope[] scopes)
        {
            _lazyOps.Add(x => x.AddScope(phase, scopes));
            _scopes.AddScope(phase, scopes);
        }
        public override void FlipScope(int phase, params Scope[] scopes)
        {
            _lazyOps.Add(x => x.FlipScope(phase, scopes));
            _scopes.FlipScope(phase, scopes);
        }
        public override void RemoveScope(int phase, params Scope[] scopes)
        {
            _lazyOps.Add(x => x.RemoveScope(phase, scopes));
            _scopes.RemoveScope(phase, scopes);
        }
        public override Syntax StripScopes(int inclusivePhaseThreshold)
        {
            LazilyOperate();
            Syntax strippedCar = _car.StripScopes(inclusivePhaseThreshold);
            Syntax strippedCdr = _cdr.StripScopes(inclusivePhaseThreshold);
            SyntaxPair strippedCopy = new SyntaxPair(strippedCar, strippedCdr, Location);
            strippedCopy._scopes.RestrictPhaseUpTo(inclusivePhaseThreshold);
            return strippedCopy;
        }
        public override ScopeSet GetScopeSet() => new ScopeSet(_scopes);

        #endregion

        public override SyntaxPair ListPrepend(Syntax stx) => new SyntaxPair(stx, this, Location);

        protected override string FormatType() => string.Format("StxPair<{0}>", _cons.TypeName);

    }
}

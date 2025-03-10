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
        //List<Action<Syntax>> _lazyOps;

        private Syntax _car;
        private Syntax _cdr;
        //private Cons _cons;

        private SyntaxPair(Term car, Term cdr, SourceCode loc, ScopeSet scopes) : base(loc)
        {
            _scopes = new ScopeSet(scopes);

            _car = WrapRaw(car, loc);
            _cdr = WrapRaw(cdr, loc);
        }

        public SyntaxPair(Term car, Term cdr, SourceCode loc) : this(car, cdr, loc, new ScopeSet())
        {
            //_scopes = new ScopeSet();
            //_lazyOps = new List<Action<Syntax>>();

            //_car = WrapRaw(car, loc);
            //_cdr = WrapRaw(cdr, loc);
            //_cons = Cons.Truct(_car, _cdr);
        }

        public SyntaxPair(Cons cns, SourceCode loc) : this(cns.Car, cns.Cdr, loc) { }

        //public override Cons Expose()
        //{
        //    LazilyOperate();
        //    return _cons;
        //}
        //public override Term ExposeAll()
        //{
        //    LazilyOperate();
        //    return Cons.Truct(_car.ExposeAll(), _cdr.ExposeAll());
        //}
        public override Cons Expose()
        {
            return Cons.Truct(_car, _cdr);
        }
        public override Cons ExposeAll()
        {
            return Cons.Truct(_car.ExposeAll(), _cdr.ExposeAll());
        }

        //private void LazilyOperate()
        //{
        //    // If there are any pending scope operations, apply them all and reset the operator
        //    if (_lazyOps.Count > 0)
        //    {
        //        foreach (var op in _lazyOps)
        //        {
        //            op(_car);
        //            op(_cdr);
        //        }
        //        _lazyOps = new List<Action<Syntax>>();
        //    }
        //}

        #region Scope-Adjustment

        //public override void AddScope(int phase, params Scope[] scopes)
        //{
        //    _lazyOps.Add(x => x.AddScope(phase, scopes));
        //    _scopes.AddScope(phase, scopes);
        //}
        //public override void FlipScope(int phase, params Scope[] scopes)
        //{
        //    _lazyOps.Add(x => x.FlipScope(phase, scopes));
        //    _scopes.FlipScope(phase, scopes);
        //}
        //public override void RemoveScope(int phase, params Scope[] scopes)
        //{
        //    _lazyOps.Add(x => x.RemoveScope(phase, scopes));
        //    _scopes.RemoveScope(phase, scopes);
        //}
        //public override Syntax StripScopes(int inclusivePhaseThreshold)
        //{
        //    LazilyOperate();
        //    Syntax strippedCar = _car.StripScopes(inclusivePhaseThreshold);
        //    Syntax strippedCdr = _cdr.StripScopes(inclusivePhaseThreshold);
        //    SyntaxPair strippedCopy = new SyntaxPair(strippedCar, strippedCdr, Location);
        //    strippedCopy._scopes.RestrictPhaseUpTo(inclusivePhaseThreshold);
        //    return strippedCopy;
        //}
        public override void AddScope(int phase, params Scope[] scopes)
        {
            _scopes.AddScope(phase, scopes);
            _car.AddScope(phase, scopes);
            _cdr.AddScope(phase, scopes);
        }
        public override void FlipScope(int phase, params Scope[] scopes)
        {
            _scopes.FlipScope(phase, scopes);
            _car.FlipScope(phase, scopes);
            _cdr.FlipScope(phase, scopes);
        }
        public override void RemoveScope(int phase, params Scope[] scopes)
        {
            _scopes.RemoveScope(phase, scopes);
            _car.RemoveScope(phase, scopes);
            _cdr.RemoveScope(phase, scopes);
        }
        public override Syntax StripScopes(int inclusivePhaseThreshold)
        {
            SyntaxPair result = new SyntaxPair(_car.StripScopes(inclusivePhaseThreshold), _cdr.StripScopes(inclusivePhaseThreshold), Location, _scopes);
            result._scopes.RestrictPhaseUpTo(inclusivePhaseThreshold);
            return result;
        }


        public override ScopeSet GetScopeSet() => new ScopeSet(_scopes);

        #endregion

        protected override string FormatType() => "StxPair";

    }
}

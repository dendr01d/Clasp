using System.Collections.Generic;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Text;

namespace Clasp.Data.Terms.SyntaxValues
{
    internal sealed class SyntaxPair : Syntax
    {
        private readonly ScopeSet _scopes;
        private readonly Cons _pair;
        public override Cons Expose() => _pair;

        /*
        
        
        HEY

        first of all, if you're going to have syntaxpairs maintaining their own scope sets, it needs to be for an actual purpose
        when you build new syntax pairs, they shouldn't actually process their constituent elements
        leave them as normal terms

        you only need to syntaxify them (and in so doing imbue them with the syntaxpair's scope set)
            when the syntaxpair itself is being deconstructed into car/cdr values
        because that's the point when the terms in question stop automatically having their scope adjusted the same way
        
        
        
        
        
        
        */



        public SyntaxPair(Term car, Term cdr, SourceCode loc, ScopeSet scopes) : base(loc)
        {
            Syntax stxCar = FromTerm(car, loc, scopes);
            Syntax stxCdr = FromTerm(cdr, loc, scopes);

            _pair = stxCdr is SyntaxPair stp
                ? Cons.Truct(stxCar, stp._pair)
                : Cons.Truct(stxCar, stxCdr);

            _scopes = scopes;
        }
        public SyntaxPair(Cons cns, SourceCode loc, ScopeSet scopes) : this(cns.Car, cns.Cdr, loc, scopes) { }

        public static Syntax ProperList(IEnumerable<Syntax> elements, ScopeSet? scope = null)
        {
            Syntax output = Datum.NilDatum;

            foreach (Syntax stx in elements)
            {
                output = new SyntaxPair(stx, output, stx.Location, stx.GetScopes());
            }

            return output;
        }

        public static Syntax ImproperList(IEnumerable<Syntax> elements, ScopeSet? scope = null)
        {
            Syntax output = Datum.NilDatum;

            foreach (Syntax stx in elements)
            {
                output = new SyntaxPair(stx, output, stx.Location, stx.GetScopes());
            }

            return output;
        }

        #region Scope-Adjustment

        public override void AddScope(int phase, params Scope[] scopes) => _scopes.AddScope(phase, scopes);
        public override void FlipScope(int phase, params Scope[] scopes) => _scopes.FlipScope(phase, scopes);
        public override void RemoveScope(int phase, params Scope[] scopes) => _scopes.RemoveScope(phase, scopes);
        public override Syntax StripScopes(int inclusivePhaseThreshold)
            => new SyntaxPair(_pair, Location, _scopes.RestrictPhaseUpTo(inclusivePhaseThreshold));
        public override ScopeSet GetScopes() => _scopes;

        #endregion

        public override SyntaxPair ListPrepend(Syntax stx) => new SyntaxPair(stx, _pair, Location, _scopes);

        protected override string FormatType() => string.Format("StxPair<{0}>", _pair.TypeName);

    }
}

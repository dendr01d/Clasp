using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;
using Clasp.Data.Terms.Product;
using Clasp.Data.Terms.Syntax;

namespace Clasp.Binding.Scopes
{
    internal static class ScopeAdjuster
    {
        private static void SetUnion(HashSet<uint> a, uint[] b) => a.UnionWith(b);
        private static void SetFlip(HashSet<uint> a, uint[] b) => a.SymmetricExceptWith(b);
        private static void SetSubtract(HashSet<uint> a, uint[] b) => a.ExceptWith(b);

        // Maybe someday I'll make a proper lazy version
        private static Term EagerlyAdjust(Term term, int phase, uint[] scopeIds, Action<HashSet<uint>, uint[]> adjustment)
        {
            if (term is Syntax stx)
            {
                // recur on the wrapped value
                // then create a NEW syntax with the result of the recurrence and copies of the original's scope sets
                // then modify the NEW syntax's scope sets instead of the old

                Term inner = EagerlyAdjust(stx.Expose(), phase, scopeIds, adjustment);
                Syntax adjustedStx = Syntax.FromDatum(inner, stx);
                adjustment(stx.GetScopeSet(phase), scopeIds);
                return adjustedStx;
            }
            else if (term is ConsList cl)
            {
                Term car = EagerlyAdjust(cl.Car, phase, scopeIds, adjustment);
                Term cdr = EagerlyAdjust(cl.Cdr, phase, scopeIds, adjustment);
                return ConsList.Cons(car, cdr);
            }
            else
            {
                return term;
            }
        }

        public static Term Paint(Term term, int phase, params uint[] scopeIds)
            => EagerlyAdjust(term, phase, scopeIds, SetUnion);

        public static Term Flip(Term term, int phase, params uint[] scopeIds)
            => EagerlyAdjust(term, phase, scopeIds, SetFlip);

        public static Term Remove(Term term, int phase, params uint[] scopeIds)
            => EagerlyAdjust(term, phase, scopeIds, SetSubtract);

        ///// <summary>
        ///// Recursively adjust the scopeset of the <paramref name="term"/> (assuming it's a <see cref="Syntax"/>
        ///// or a <see cref="ConsList"/> containing <see cref="Syntax"/> objects), making the same
        ///// <paramref name="adjustment"/> in ALL PHASES.
        ///// </summary>
        //private static Term EagerlyAdjustAll(Term term, uint[] scopeIds, Action<HashSet<uint>, uint[]> adjustment)
        //{
        //    if (term is Syntax stx)
        //    {
        //        Term inner = EagerlyAdjustAll(stx.Expose(), scopeIds, adjustment);
        //        Syntax adjustedStx = Syntax.Wrap(inner, stx);

        //        foreach (int phase in stx.GetLivePhases())
        //        {
        //            adjustment(adjustedStx.GetScopeSet(phase), scopeIds);
        //        }

        //        return adjustedStx;
        //    }
        //    else if (term is ConsList cl)
        //    {
        //        Term car = EagerlyAdjustAll(cl.Car, scopeIds, adjustment);
        //        Term cdr = EagerlyAdjustAll(cl.Cdr, scopeIds, adjustment);
        //        return ConsList.Cons(car, cdr);
        //    }
        //    else
        //    {
        //        return term;
        //    }
        //}

        //public static Term PaintAll(Term term, params uint[] scopeIds)
        //    => EagerlyAdjustAll(term, scopeIds, SetUnion);

        //public static Term FlipInAll(Term term, params uint[] scopeIds)
        //    => EagerlyAdjustAll(term, scopeIds, SetFlip);

        //public static Term RemoveFromAll(Term term, params uint[] scopeIds)
        //    => EagerlyAdjustAll(term, scopeIds, SetSubtract);
    }
}

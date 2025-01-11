using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Data.Metadata;
using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp.Data.Terms
{
    internal class SyntaxWrapper : Term
    {
        public readonly SourceLocation Source;
        public readonly PhasedLexicalInfo Context;

        public readonly Term WrappedTerm;

        private SyntaxWrapper(Term term, SourceLocation source, PhasedLexicalInfo? lexInfo)
        {
            WrappedTerm = term;

            Source = source;
            Context = lexInfo ?? new PhasedLexicalInfo();
        }

        public static SyntaxWrapper Wrap(Term term, SourceLocation source, PhasedLexicalInfo? lexInfo)
        {
            if (term is SyntaxWrapper sw)
            {
                throw new InvalidOperationException("Can't wrap existing syntax wrapper.");
            }
            else
            {
                return new SyntaxWrapper(term, source, lexInfo);
            }
        }

        public static SyntaxWrapper Wrap(Term term, SyntaxWrapper extantWrapper)
        {
            if (term is SyntaxWrapper sw)
            {
                throw new InvalidOperationException("Can't wrap existing syntax wrapper.");
            }
            else
            {
                return new SyntaxWrapper(term, extantWrapper.Source, extantWrapper.Context);
            }
        }

        public static SyntaxWrapper Wrap(Term term, Token token) => Wrap(term, token.Location, null);

        // ---

        public void Paint(int phase, params uint[] scopeTokens)
        {
            Context[phase].Add(scopeTokens);
        }

        public void FlipScope(int phase, params uint[] scopeTokens)
        {
            Context[phase].Flip(scopeTokens);
        }

        public Term Strip()
        {
            if (WrappedTerm is ConsList cl)
            {
                Term outCar = cl.Car is SyntaxWrapper stxCar
                    ? stxCar.Strip()
                    : cl.Car;

                Term outCdr = cl.Cdr is SyntaxWrapper stxCdr
                    ? stxCdr.Strip()
                    : cl.Cdr;

                if (cl.Car is SyntaxWrapper || cl.Cdr is SyntaxWrapper)
                {
                    return ConsList.Cons(
                        outCar == cl.Car ? cl.Car : outCar,
                        outCdr == cl.Cdr ? cl.Cdr : outCdr);
                }
                else
                {
                    return cl;
                }
            }
            //else if (WrappedTerm is Vector vec)
            //{

            //}
            else
            {
                return WrappedTerm;
            }
        }

        public Term Expose()
        {
            if (WrappedTerm is ConsList cl)
            {
                if (cl.Car is not SyntaxWrapper)
                {
                    cl.SetCar(Wrap(cl.Car, this));
                }
                if (cl.Cdr is not SyntaxWrapper)
                {
                    cl.SetCdr(Wrap(cl.Cdr, this));
                }
                return cl;
            }
            //else if (WrappedTerm is Vector vec)
            //{

            //}
            else
            {
                return WrappedTerm;
            }
        }

        public bool TryExposeList(
            [NotNullWhen(true)] out SyntaxWrapper? car,
            [NotNullWhen(true)] out SyntaxWrapper? cdr)
        {
            if (Expose() is ConsList cl
                && cl.Car is SyntaxWrapper stxCar
                && cl.Cdr is SyntaxWrapper stxCdr)
            {
                car = stxCar;
                cdr = stxCdr;
                return true;
            }
            else
            {
                car = null;
                cdr = null;
                return false;
            }
        }

        public bool TryExposeIdentifier(
            [NotNullWhen(true)] out Symbol? sym,
            [NotNullWhen(true)] out string? name)
        {
            if (WrappedTerm is Symbol s)
            {
                sym = s;
                name = s.Name;
                return true;
            }
            else
            {
                sym = null;
                name = null;
                return false;
            }
        }

        public override string ToString()
        {
            return string.Format("#'{0}", WrappedTerm);
        }
    }

    //internal abstract class Syntax : Term
    //{
    //    public readonly SourceLocation Source;
    //    public readonly PhasedLexicalInfo Context;

    //    protected Syntax(SourceLocation source, PhasedLexicalInfo? lexInfo = null)
    //    {
    //        Source = source;
    //        Context = lexInfo ?? new PhasedLexicalInfo();
    //    }

    //    public virtual void Paint(int phase, params uint[] tokens)
    //    {
    //        Context[phase].Add(tokens);
    //    }

    //    public virtual void Flip(int phase, params uint[] tokens)
    //    {
    //        Context[phase].Flip(tokens);
    //    }

    //    public abstract Term Strip();
    //    public abstract Term Expose();



    //    // ---------

    //    public static Syntax Wrap(Term value, Token token) => Wrap(value, token.Location, null);

    //    public static Syntax Wrap(Term value, SourceLocation loc, PhasedLexicalInfo? lexInfo = null)
    //    {
    //        return value switch
    //        {
    //            Syntax stx => stx,
    //            Symbol s => new Identifier(s, loc, lexInfo),
    //            ConsList cl => new SyntaxList(cl, loc, lexInfo),
    //            Atom a => new SyntaxAtom(a, loc, lexInfo),
    //            _ => throw new ClaspException.Uncategorized(string.Format("Impossible syntax type: {0}.", value))
    //        };
    //    }

    //    protected static Term StripSyntax(Term t) => t is Syntax s ? s.Strip() : t;

    //    public static bool TryExposeSyntaxList(Syntax input,
    //        [NotNullWhen(true)] out SyntaxList? cons,
    //        [NotNullWhen(true)] out Syntax? car,
    //        [NotNullWhen(true)] out Syntax? cdr)
    //    {
    //        cons = null;
    //        car = null;
    //        cdr = null;

    //        if (input is SyntaxList stxList
    //            && stxList.Expose() is ConsList stxCons
    //            && stxCons.Car is Syntax stxCar
    //            && stxCons.Cdr is Syntax stxCdr)
    //        {
    //            cons = stxList;
    //            car = stxCar;
    //            cdr = stxCdr;
    //        }

    //        return false;
    //    }
    //}

    //// -------------------------------------------

    //internal sealed class SyntaxAtom : Syntax
    //{
    //    public readonly Atom WrappedValue;
    //    public SyntaxAtom(Atom value, SourceLocation source, PhasedLexicalInfo? lexInfo = null) : base(source, lexInfo) => WrappedValue = value;
    //    public SyntaxAtom(Atom value, Syntax extant) : this(value, extant.Source, extant.Context) { }
    //    public override Term Strip() => WrappedValue;
    //    public override Term Expose() => WrappedValue;
    //    public override string ToString() => string.Format("STX({0})", WrappedValue);
    //}

    //// -------------------------------------------

    //internal sealed class SyntaxList : Syntax
    //{
    //    public readonly ConsList WrappedValue;
    //    private readonly List<Tuple<int, uint>> _pendingPaint = new List<Tuple<int, uint>>();
    //    public SyntaxList(ConsList value, SourceLocation source, PhasedLexicalInfo? lexInfo = null) : base(source, lexInfo) => WrappedValue = value;
    //    public SyntaxList(ConsList value, Syntax extant) : this(value, extant.Source, extant.Context) { }
    //    public SyntaxList(Syntax car, Syntax cdr, SourceLocation source, PhasedLexicalInfo? lexInfo = null) : base(source, lexInfo)
    //    {
    //        WrappedValue = ConsList.Cons(car, cdr);
    //    }
    //    public SyntaxList(Syntax car, Syntax cdr, Syntax extant) : this(car, cdr, extant.Source, extant.Context) { }
    //    public override void Paint(int phase, params uint[] tokens)
    //    {
    //        _pendingPaint.AddRange(tokens.Select(x => new Tuple<int, uint>(phase, x)));
    //        base.Paint(phase, tokens);
    //    }
    //    public override Term Strip()
    //    {
    //        Term car = StripSyntax(WrappedValue.Car);
    //        Term cdr = StripSyntax(WrappedValue.Cdr);

    //        return (car == WrappedValue.Car && cdr == WrappedValue.Cdr)
    //            ? WrappedValue
    //            : ConsList.Cons(car, cdr);
    //    }
    //    public override Term Expose()
    //    {
    //        foreach(var pendingScope in _pendingPaint)
    //        {
    //            if (WrappedValue.Car is Syntax stxCar) stxCar.Paint(pendingScope.Item1, pendingScope.Item2);
    //            if (WrappedValue.Cdr is Syntax stxCdr) stxCdr.Paint(pendingScope.Item1, pendingScope.Item2);
    //        }
    //        _pendingPaint.Clear();
    //        return WrappedValue;
    //    }

    //    public override string ToString() => string.Format("STX({0})", WrappedValue);
    //}

    //// -------------------------------------------

    //internal sealed class Identifier : Syntax, IBindable
    //{
    //    public string Name { get => WrappedValue.Name; }

    //    public readonly Symbol WrappedValue;
    //    public Identifier(Symbol value, SourceLocation source, PhasedLexicalInfo? lexInfo = null) : base(source, lexInfo) => WrappedValue = value;
    //    public Identifier(Symbol value, Syntax extant) : this(value, extant.Source, extant.Context) { }
    //    public override Term Strip() => WrappedValue;
    //    public override Term Expose() => WrappedValue;
    //    public override string ToString() => string.Format("STX({0})", WrappedValue);
    //}
}

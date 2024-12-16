using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Clasp.Data.Metadata;
using Clasp.Data.Text;
using Clasp.Interfaces;

namespace Clasp.Data.Terms
{
    internal abstract class Syntax : Term
    {
        public readonly SourceLocation Source;
        public readonly PhasedLexicalInfo Context;

        protected Syntax(SourceLocation source, PhasedLexicalInfo? lexInfo = null)
        {
            Source = source;
            Context = lexInfo ?? new PhasedLexicalInfo();
        }

        public virtual void Paint(int phase, params uint[] tokens)
        {
            Context[phase].Add(tokens);
        }

        public virtual void Flip(int phase, params uint[] tokens)
        {
            Context[phase].Flip(tokens);
        }

        public abstract Term Strip();
        public abstract Term Expose();



        // ---------

        public static Syntax Wrap(Term value, Token token) => Wrap(value, token.Location, null);

        public static Syntax Wrap(Term value, SourceLocation loc, PhasedLexicalInfo? lexInfo = null)
        {
            return value switch
            {
                Syntax stx => stx,
                Symbol s => new Identifier(s, loc, lexInfo),
                ConsList cl => new SyntaxList(cl, loc, lexInfo),
                Atom a => new SyntaxAtom(a, loc, lexInfo),
                _ => throw new ClaspException.Uncategorized(string.Format("Impossible syntax type: {0}.", value))
            };
        }

        protected static Term StripSyntax(Term t) => t is Syntax s ? s.Strip() : t;

        public static bool TryExposeSyntaxList(Syntax input,
            [NotNullWhen(true)] out SyntaxList? cons,
            [NotNullWhen(true)] out Syntax? car,
            [NotNullWhen(true)] out Syntax? cdr)
        {
            cons = null;
            car = null;
            cdr = null;

            if (input is SyntaxList stxList
                && stxList.Expose() is ConsList stxCons
                && stxCons.Car is Syntax stxCar
                && stxCons.Cdr is Syntax stxCdr)
            {
                cons = stxList;
                car = stxCar;
                cdr = stxCdr;
            }

            return false;
        }
    }

    // -------------------------------------------

    internal sealed class SyntaxAtom : Syntax
    {
        public readonly Atom WrappedValue;
        public SyntaxAtom(Atom value, SourceLocation source, PhasedLexicalInfo? lexInfo = null) : base(source, lexInfo) => WrappedValue = value;
        public SyntaxAtom(Atom value, Syntax extant) : this(value, extant.Source, extant.Context) { }
        public override Term Strip() => WrappedValue;
        public override Term Expose() => WrappedValue;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }

    // -------------------------------------------

    internal sealed class SyntaxList : Syntax
    {
        public readonly ConsList WrappedValue;
        private readonly List<Tuple<int, uint>> _pendingPaint = new List<Tuple<int, uint>>();
        public SyntaxList(ConsList value, SourceLocation source, PhasedLexicalInfo? lexInfo = null) : base(source, lexInfo) => WrappedValue = value;
        public SyntaxList(ConsList value, Syntax extant) : this(value, extant.Source, extant.Context) { }
        public SyntaxList(Syntax car, Syntax cdr, SourceLocation source, PhasedLexicalInfo? lexInfo = null) : base(source, lexInfo)
        {
            WrappedValue = ConsList.Cons(car, cdr);
        }
        public SyntaxList(Syntax car, Syntax cdr, Syntax extant) : this(car, cdr, extant.Source, extant.Context) { }
        public override void Paint(int phase, params uint[] tokens)
        {
            _pendingPaint.AddRange(tokens.Select(x => new Tuple<int, uint>(phase, x)));
            base.Paint(phase, tokens);
        }
        public override Term Strip()
        {
            Term car = StripSyntax(WrappedValue.Car);
            Term cdr = StripSyntax(WrappedValue.Cdr);

            return (car == WrappedValue.Car && cdr == WrappedValue.Cdr)
                ? WrappedValue
                : ConsList.Cons(car, cdr);
        }
        public override Term Expose()
        {
            foreach(var pendingScope in _pendingPaint)
            {
                if (WrappedValue.Car is Syntax stxCar) stxCar.Paint(pendingScope.Item1, pendingScope.Item2);
                if (WrappedValue.Cdr is Syntax stxCdr) stxCdr.Paint(pendingScope.Item1, pendingScope.Item2);
            }
            _pendingPaint.Clear();
            return WrappedValue;
        }

        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }

    // -------------------------------------------

    internal sealed class Identifier : Syntax, IBindable
    {
        public string Name { get => WrappedValue.Name; }

        public readonly Symbol WrappedValue;
        public Identifier(Symbol value, SourceLocation source, PhasedLexicalInfo? lexInfo = null) : base(source, lexInfo) => WrappedValue = value;
        public Identifier(Symbol value, Syntax extant) : this(value, extant.Source, extant.Context) { }
        public override Term Strip() => WrappedValue;
        public override Term Expose() => WrappedValue;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }
}

using System;
using System.Collections;
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

        protected Syntax(SourceLocation source)
        {
            Source = source;
            Context = new PhasedLexicalInfo();
        }

        protected Syntax(PhasedLexicalInfo lexInfo, SourceLocation source) : this(source)
        {
            Context = lexInfo;
        }

        public virtual void Paint(int phase, params uint[] tokens)
        {
            Context[phase].Add(tokens);
        }
        public abstract Term Strip();
        public abstract Term Expose(int phase);



        // ---------

        public static Syntax Wrap(Term value, Token token)
        {
            return value switch
            {
                Syntax stx => stx,
                Symbol s => new Identifier(s, token.Location),
                Product p => new SyntaxList(p, token.Location),
                Atom a => new SyntaxAtom(a, token.Location),
                _ => throw new Exception(string.Format("Impossible syntax type: {0}.", value))
            };
        }

        //public static Syntax Wrap(Term value, PhasedLexicalInfo lexInfo, SourceLocation source)
        //{
        //    return value switch
        //    {
        //        Symbol s => new Identifier(s, lexInfo, source),
        //        Product p => new SyntaxList(p, lexInfo, source),
        //        Atom a => new SyntaxAtom(a, lexInfo, source),
        //        _ => throw new Exception(string.Format("Impossible syntax type: {0}.", value))
        //    };
        //}

        protected static Term StripSyntax(Term t) => t is Syntax s ? s.Strip() : t;
        
    }

    internal sealed class SyntaxAtom : Syntax
    {
        public readonly Atom WrappedValue;
        public SyntaxAtom(Atom value, SourceLocation source) : base(source) => WrappedValue = value;
        public SyntaxAtom(Atom value, PhasedLexicalInfo lexInfo, SourceLocation source) : base(lexInfo, source) => WrappedValue = value;
        public override Term Strip() => WrappedValue;
        public override Term Expose(int phase) => WrappedValue;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }

    internal sealed class SyntaxList : Syntax
    {
        public readonly ConsList WrappedValue;
        private readonly List<uint> _pendingPaint = new List<uint>();

        public SyntaxList(ConsList value, SourceLocation source) : base(source) => WrappedValue = value;
        public SyntaxList(ConsList value, PhasedLexicalInfo lexInfo, SourceLocation source) : base(lexInfo, source) => WrappedValue = value;

        public override void Paint(int phase, params uint[] tokens)
        {
            _pendingPaint.AddRange(tokens);
            base.Paint(phase, tokens);
        }
        public override Term Strip() => ConsList.Cons(StripSyntax(WrappedValue.Car), StripSyntax(WrappedValue.Cdr));
        public override Term Expose(int phase)
        {
            if (WrappedValue.Car is Syntax stxCar) stxCar.Paint(phase, _pendingPaint.ToArray());
            if (WrappedValue.Cdr is Syntax stxCdr) stxCdr.Paint(phase, _pendingPaint.ToArray());
            _pendingPaint.Clear();
            return WrappedValue;
        }

        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }

    internal sealed class Identifier : Syntax, IBindable
    {
        public string Name { get => WrappedValue.Name; }

        public readonly Symbol WrappedValue;
        public Identifier(Symbol value, SourceLocation source) : base(source) => WrappedValue = value;
        public Identifier(Symbol value, PhasedLexicalInfo lexInfo, SourceLocation source) : base(lexInfo, source) => WrappedValue = value;
        public override Term Strip() => WrappedValue;
        public override Term Expose(int phase) => WrappedValue;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }
}

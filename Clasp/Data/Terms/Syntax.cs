using Clasp.Data.Metadata;
using Clasp.Data.Text;

namespace Clasp.Data.Terms
{
    internal abstract class Syntax : Term
    {
        public readonly SourceLocation Source;
        public readonly bool Original;
        public readonly LexicalInfo Context;

        protected Syntax(SourceLocation source)
        {
            Source = source;
            Context = new LexicalInfo();
            Original = false;
        }

        protected Syntax(Token source) : this(source.Location)
        {
            Original = true;
        }

        protected Syntax(Syntax source) : this(source.Source)
        {
            Context = new LexicalInfo(source.Context);
        }

        public abstract Term Strip();

        public static Syntax Wrap(Term value, Token source)
        {
            return value switch
            {
                Symbol s => new SyntaxId(s, source),
                Product p => new SyntaxProduct(p, source),
                Atom a => new SyntaxAtom(a, source),
                _ => throw new Exception(string.Format("Impossible syntax type: {0}.", value))
            };
        }

        public static Syntax Wrap(Term value, Syntax source)
        {
            return value switch
            {
                Symbol s => new SyntaxId(s, source),
                Product p => new SyntaxProduct(p, source),
                Atom a => new SyntaxAtom(a, source),
                _ => throw new Exception(string.Format("Impossible syntax type: {0}.", value))
            };
        }

        protected static Term StripSyntax(Term t) => t is Syntax s ? s.Strip() : t;
    }

    internal sealed class SyntaxAtom : Syntax
    {
        public readonly Atom WrappedValue;
        public SyntaxAtom(Atom value, Token source) : base(source) => WrappedValue = value;
        public SyntaxAtom(Atom value, Syntax source) : base(source) => WrappedValue = value;
        public override Term Strip() => WrappedValue;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }

    internal sealed class SyntaxProduct : Syntax
    {
        public readonly Product WrappedValue;
        public SyntaxProduct(Product value, Token source) : base(source) => WrappedValue = value;
        public SyntaxProduct(Product value, Syntax source) : base(source) => WrappedValue = value;
        public override Term Strip()
        {
            return WrappedValue switch
            {
                ConsList cl => ConsList.ConstructDirect(cl.Select(StripSyntax)),
                Vector vec => new Vector(vec.Values.Select(StripSyntax).ToArray()),
                _ => WrappedValue
            };
        }
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }

    internal sealed class SyntaxId : Syntax
    {
        public readonly Symbol WrappedValue;
        public SyntaxId(Symbol value, Token source) : base(source) => WrappedValue = value;
        public SyntaxId(Symbol value, Syntax source) : base(source) => WrappedValue = value;
        public override Term Strip() => WrappedValue;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }
}

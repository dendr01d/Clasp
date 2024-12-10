using System.Diagnostics.CodeAnalysis;

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

        public abstract Term Strip();

        public static Syntax Wrap(Term value, Token token)
        {
            return value switch
            {
                Symbol s => new Identifier(s, token.Location),
                Product p => new SyntaxProduct(p, token.Location),
                Atom a => new SyntaxAtom(a, token.Location),
                _ => throw new Exception(string.Format("Impossible syntax type: {0}.", value))
            };
        }

        public static Syntax Wrap(Term value, PhasedLexicalInfo lexInfo, SourceLocation source)
        {
            return value switch
            {
                Symbol s => new Identifier(s, lexInfo, source),
                Product p => new SyntaxProduct(p, lexInfo, source),
                Atom a => new SyntaxAtom(a, lexInfo, source),
                _ => throw new Exception(string.Format("Impossible syntax type: {0}.", value))
            };
        }

        protected static Term StripSyntax(Term t) => t is Syntax s ? s.Strip() : t;
    }

    internal sealed class SyntaxAtom : Syntax
    {
        public readonly Atom WrappedValue;
        public SyntaxAtom(Atom value, SourceLocation source) : base(source) => WrappedValue = value;
        public SyntaxAtom(Atom value, PhasedLexicalInfo lexInfo, SourceLocation source) : base(lexInfo, source) => WrappedValue = value;
        public override Term Strip() => WrappedValue;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }

    internal sealed class SyntaxProduct : Syntax
    {
        public readonly Product WrappedValue;
        public SyntaxProduct(Product value, SourceLocation source) : base(source) => WrappedValue = value;
        public SyntaxProduct(Product value, PhasedLexicalInfo lexInfo, SourceLocation source) : base(lexInfo, source) => WrappedValue = value;
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

    internal sealed class Identifier : Syntax, IBindable
    {
        public string Name { get => WrappedValue.Name; }

        public readonly Symbol WrappedValue;
        public Identifier(Symbol value, SourceLocation source) : base(source) => WrappedValue = value;
        public Identifier(Symbol value, PhasedLexicalInfo lexInfo, SourceLocation source) : base(lexInfo, source) => WrappedValue = value;
        public override Term Strip() => WrappedValue;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }
}

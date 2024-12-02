namespace Clasp.AST
{
    internal abstract class Syntax : Fixed, ISourceTraceable
    {
        public readonly Lexer.Token? SourceToken;
        public readonly Binding.ScopeSet Context;
        public Lexer.Token? SourceTrace => SourceToken;

        protected Syntax()
        {
            Context = new Binding.ScopeSet();
        }
        protected Syntax(Lexer.Token source) : this()
        {
            SourceToken = source;
        }
        protected Syntax(Syntax source) : this()
        {
            SourceToken = source.SourceToken;
            Context = new Binding.ScopeSet(source.Context);
        }

        public abstract Fixed Strip();

        public static Syntax Wrap(Fixed value, Lexer.Token source)
        {
            return value switch
            {
                Symbol s => new SyntaxId(s, source),
                Product p => new SyntaxProduct(p, source),
                Atom a => new SyntaxAtom(a, source),
                _ => throw new Exception(string.Format("Impossible syntax type: {0}.", value))
            };
        }

        public static Syntax Wrap(Fixed value, Syntax source)
        {
            return value switch
            {
                Symbol s => new SyntaxId(s, source),
                Product p => new SyntaxProduct(p, source),
                Atom a => new SyntaxAtom(a, source),
                _ => throw new Exception(string.Format("Impossible syntax type: {0}.", value))
            };
        }
    }

    internal sealed class SyntaxAtom : Syntax
    {
        public readonly Atom WrappedValue;
        public SyntaxAtom(Atom value, Lexer.Token source) : base(source) => WrappedValue = value;
        public SyntaxAtom(Atom value, Syntax source) : base(source) => WrappedValue = value;
        public override Fixed Strip() => WrappedValue;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }

    internal sealed class SyntaxProduct : Syntax
    {
        public readonly Product WrappedValue;
        public SyntaxProduct(Product value, Lexer.Token source) : base(source) => WrappedValue = value;
        public SyntaxProduct(Product value, Syntax source) : base(source) => WrappedValue = value;
        public override Fixed Strip()
        {
            if (WrappedValue is ConsCell cell)
            {
                if (cell.Car is Syntax stxCar)
                {
                    cell.SetCar(stxCar.Strip());
                }

                if (cell.Cdr is Syntax stxCdr)
                {
                    cell.SetCdr(stxCdr.Strip());
                }

                return cell;
            }
            else if (WrappedValue is Vector vec)
            {
                for (int i = 0; i < vec.Values.Length; ++i)
                {
                    if (vec.Values[i] is Syntax stx)
                    {
                        vec.Values[i] = stx.Strip();
                    }
                }

                return vec;
            }
            else
            {
                return WrappedValue;
            }
        }
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }

    internal sealed class SyntaxId : Syntax
    {
        public readonly Symbol WrappedValue;
        public SyntaxId(Symbol value, Lexer.Token source) : base(source) => WrappedValue = value;
        public SyntaxId(Symbol value, Syntax source) : base(source) => WrappedValue = value;
        public override Fixed Strip() => WrappedValue;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }
}

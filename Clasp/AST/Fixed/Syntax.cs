using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.AST
{
    internal abstract class Syntax : Fixed
    {
        public readonly int SourceLine;
        public readonly int SourceIndex;
        public readonly HashSet<string> Context;
        protected Syntax(int line, int index)
        {
            SourceLine = line;
            SourceIndex = index;
            Context = new HashSet<string>();
        }
        protected Syntax(Syntax source) : this(source.SourceLine, source.SourceIndex)
        {
            Context = new HashSet<string>(source.Context);
        }

        public static Syntax Wrap(Fixed value, int line, int index)
        {
            return value switch
            {
                Symbol s => new SyntaxId(s, line, index),
                Product p => new SyntaxProduct(p, line, index),
                Atom a => new SyntaxAtom(a, line, index),
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
        public SyntaxAtom(Atom value, int line, int index) : base(line, index) => WrappedValue = value;
        public SyntaxAtom(Atom value, Syntax source) : base(source) => WrappedValue = value;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }

    internal sealed class SyntaxProduct : Syntax
    {
        public readonly Product WrappedValue;
        public SyntaxProduct(Product value, int line, int index) : base(line, index) => WrappedValue = value;
        public SyntaxProduct(Product value, Syntax source) : base(source) => WrappedValue = value;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }

    internal sealed class SyntaxId : Syntax
    {
        public readonly Symbol WrappedValue;
        public SyntaxId(Symbol value, int line, int index) : base(line, index) => WrappedValue = value;
        public SyntaxId(Symbol value, Syntax source) : base(source) => WrappedValue = value;
        public override string ToString() => string.Format("STX({0})", WrappedValue);
    }
}

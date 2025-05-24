using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

using ClaspCompiler.Data;
using ClaspCompiler.Textual;

namespace ClaspCompiler.Syntax
{
    internal sealed class StxPair : SyntaxBase, ICons<ISyntax>
    {
        public ISyntax Car { get; private set; }
        public ISyntax Cdr { get; private set; }

        public override bool IsAtom => false;
        public override bool IsNil => false;

        public StxPair(ISyntax car, ISyntax cdr, SourceRef? source = null) : base(source)
        {
            Car = car;
            Cdr = cdr;
        }

        public override string ToString() => IConsExtensions.ToString(this);
        public override void Print(TextWriter writer, int indent) => IConsExtensions.Print(this, writer, indent);

        public IEnumerator<ISyntax> GetEnumerator() => this.Enumerate();
        IEnumerator IEnumerable.GetEnumerator() => this.Enumerate();
    }
}

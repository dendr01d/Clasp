using System.Collections;
using System.Diagnostics.CodeAnalysis;

using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.Textual;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed class StxPair : SyntaxBase, ICons<ISyntax>
    {
        public required ISyntax Car { get; init; }
        public required ISyntax Cdr { get; init; }

        public override bool IsAtom => false;
        public override bool IsNil => false;

        public StxPair(SourceRef src, IEnumerable<uint>? scopeSet = null)
            : base(src, scopeSet ?? [])
        { }

        [SetsRequiredMembers]
        public StxPair(ISyntax car, ISyntax cdr, SourceRef src, IEnumerable<uint>? scopeSet = null) 
            : this(src, scopeSet)
        {
            Car = car;
            Cdr = cdr;
        }

        public override StxPair AddScopes(params uint[] ids) => new(Source, ScopeSet.Union(ids))
        {
            Car = Car.AddScopes(ids),
            Cdr = Cdr.AddScopes(ids)
        };
        public override StxPair RemoveScopes(params uint[] ids) => new(Source, ScopeSet.Except(ids))
        {
            Car = Car.RemoveScopes(ids),
            Cdr = Cdr.RemoveScopes(ids)
        };
        public override StxPair FlipScopes(params uint[] ids) => new(Source, ScopeSet.SymmetricExcept(ids))
        {
            Car = Car.FlipScopes(ids),
            Cdr = Cdr.FlipScopes(ids)
        };
        public override StxPair ClearScopes() => new(Car, Cdr, Source)
        {
            Car = Car.ClearScopes(),
            Cdr = Cdr.ClearScopes()
        };

        public override bool BreaksLine => Car.BreaksLine || Cdr is ICons<ISyntax>;
        public override string AsString => this.Stringify();
        public override void Print(TextWriter writer, int indent) => writer.WriteCons(this, indent);

        public IEnumerator<ISyntax> GetEnumerator() => this.Enumerate();
        IEnumerator IEnumerable.GetEnumerator() => this.Enumerate();
    }
}

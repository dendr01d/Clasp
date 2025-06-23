using System.Diagnostics.CodeAnalysis;

using ClaspCompiler.SchemeData;
using ClaspCompiler.SchemeSyntax.Abstract;
using ClaspCompiler.Textual;

namespace ClaspCompiler.SchemeSyntax
{
    internal sealed class Identifier : SyntaxBase
    {
        public required Symbol FreeSymbol { get; init; }
        public required Symbol BindingSymbol { get; init; }

        public override bool IsAtom => true;
        public override bool IsNil => false;

        public Identifier(SourceRef src, IEnumerable<uint>? scopeSet = null)
            : base(src, scopeSet ?? [])
        { }

        [SetsRequiredMembers]
        public Identifier(Symbol freeSym, Symbol? bindingSym, SourceRef src, IEnumerable<uint>? scopeSet = null) 
            : this(src, scopeSet)
        {
            FreeSymbol = freeSym;
            BindingSymbol = bindingSym ?? freeSym;
        }

        public override Identifier AddScopes(params uint[] ids) => new(FreeSymbol, BindingSymbol, Source, ScopeSet.Union(ids));
        public override Identifier RemoveScopes(params uint[] ids) => new(FreeSymbol, BindingSymbol, Source, ScopeSet.Except(ids));
        public override Identifier FlipScopes(params uint[] ids) => new(FreeSymbol, BindingSymbol, Source, ScopeSet.SymmetricExcept(ids));
        public override Identifier ClearScopes() => new(FreeSymbol, BindingSymbol, Source);

        public override bool BreaksLine => false;
        public override string AsString => BindingSymbol.ToString();
        public override void Print(TextWriter writer, int indent) => writer.Write(AsString);
    }
}

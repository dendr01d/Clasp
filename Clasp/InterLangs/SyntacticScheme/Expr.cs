namespace Clasp.InterLangs.SyntacticScheme
{
    internal abstract class Expr : InterLang<Expr>
    {
        protected Expr() : base(nameof(SyntacticScheme)) { }
    }
}

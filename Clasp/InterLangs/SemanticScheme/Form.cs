namespace Clasp.InterLangs.SemanticScheme
{
    internal abstract class Form : InterLang<Form>
    {
        protected Form() : base(nameof(SemanticScheme)) { }
    }
}

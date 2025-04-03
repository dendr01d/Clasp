using Clasp.Data.Text;

namespace Clasp.Data.Terms
{
    internal interface ISyntax : ITerm
    {
        SourceCode Location { get; }
    }
}

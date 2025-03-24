using Clasp.Data.Abstractions.Variables;

namespace Clasp.Data.Abstractions.References
{
    /// <summary>
    /// Represents a reference to a mutable <see cref="AbstractObject"/> defined at the "top level",
    /// i.e. outside the context of ANY <see cref="AbstractProgram"/>.
    /// </summary>
    internal sealed class GlobalReference : AbstractReference
    {
        private readonly GlobalVariable _var;
        public override GlobalVariable Variable => _var;

        public GlobalReference(GlobalVariable var)
        {
            _var = var;
        }
    }
}

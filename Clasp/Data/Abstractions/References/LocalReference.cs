using Clasp.Data.Abstractions.Variables;

namespace Clasp.Data.Abstractions.References
{
    /// <summary>
    /// Represents a reference to a mutable <see cref="AbstractObject"/> defined within the context
    /// of the enclosing <see cref="AbstractProgram"/>.
    /// </summary>
    internal sealed class LocalReference : AbstractReference
    {
        private readonly LocalVariable _var;
        public override LocalVariable Variable => _var;

        public LocalReference(LocalVariable var)
        {
            _var = var;
        }
    }
}

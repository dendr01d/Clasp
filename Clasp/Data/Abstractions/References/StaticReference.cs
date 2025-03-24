using Clasp.Data.Abstractions.Variables;

namespace Clasp.Data.Abstractions.References
{
    /// <summary>
    /// Represents a reference to an immutable <see cref="AbstractObject"/> that's statically defined.
    /// </summary>
    internal sealed class StaticReference : AbstractReference
    {
        private readonly StaticVariable _var;
        public override StaticVariable Variable => _var;

        public StaticReference(StaticVariable var)
        {
            _var = var;
        }
    }
}

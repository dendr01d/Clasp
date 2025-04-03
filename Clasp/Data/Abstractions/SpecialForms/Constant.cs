using Clasp.Data.Static;
using Clasp.Data.Terms;

namespace Clasp.Data.Abstractions.SpecialForms
{
    /// <summary>
    /// Represents a process that simply returns a statically-known <see cref="IAbstractForm"/>.
    /// </summary>
    internal sealed class Constant : AbstractSpecialForm
    {
        public readonly ITerm Value;

        public Constant(ITerm value) => Value = value;

        public override string ToString() => $"const: {Value}";
        public override ITerm Express() => Cons.List(Symbols.S_Const, Value, new Nil());
    }
}

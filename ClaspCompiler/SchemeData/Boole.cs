using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Boole : ValueBase<bool>, IValue
    {
        public static readonly Boole True = new(true);
        public static readonly Boole False = new(false);

        private Boole(bool b) : base(b, AtomicType.Boole) { }

        public override string AsString => Value ? "#t" : "#f";
    }
}

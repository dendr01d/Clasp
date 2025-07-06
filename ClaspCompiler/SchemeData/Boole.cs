using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Boole : ValueBase<bool>
    {
        public static readonly Boole True = new(true);
        public static readonly Boole False = new(false);

        private Boole(bool value) : base(value, UnionType.Boole) { }

        public override string AsString => Value ? "#t" : "#f";
    }
}

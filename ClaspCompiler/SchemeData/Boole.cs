using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Boole : ValueBase<bool>, IValue
    {
        public static readonly Boole True = new(true);
        public static readonly Boole False = new(false);

        private Boole(bool b) : base(b) { }
    }
}

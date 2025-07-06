using ClaspCompiler.SchemeData.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeData
{
    internal sealed record Boolean : ValueBase<bool>
    {
        public static readonly Boolean True = new(true);
        public static readonly Boolean False = new(false);

        private Boolean(bool value) : base(value, SchemeType.Boolean) { }

        public override string AsString => Value ? "#t" : "#f";
    }
}

namespace ClaspCompiler.SchemeTypes
{
    internal static class TypeList
    {
        public static SchemeType List(params SchemeType[] types)
        {
            return types.Length == 0
                ? AtomicType.Nil
                : new PairType(types[0], List(types[1..]));
        }

        public static SchemeType DottedList(params SchemeType[] types)
        {
            return types.Length switch
            {
                0 => AtomicType.Nil,
                1 => types[0],
                _ => new PairType(types[0], DottedList(types[1..]))
            };
        }

        public static SchemeType ListOf(SchemeType elementType, VarType bindingType)
        {
            return new UnionType(AtomicType.Nil, new PairType(elementType, bindingType));
        }
    }
}

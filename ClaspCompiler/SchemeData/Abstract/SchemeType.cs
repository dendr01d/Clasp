namespace ClaspCompiler.SchemeData.Abstract
{
    internal enum SchemeType
    {
        Integer,
        Boolean,
        Symbol,
        Cons
    }

    internal static class SchemeTypeExtensions
    {
        public static string GetName(this SchemeType sType)
        {
            return sType.ToString().ToLower();
        }
    }
}

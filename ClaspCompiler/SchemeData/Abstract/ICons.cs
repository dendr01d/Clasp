namespace ClaspCompiler.SchemeData.Abstract
{
    /// <summary>
    /// A structurally recursive scheme object containing two members of a certain expression type.
    /// </summary>
    /// <typeparam name="T">
    /// The expression type held by the cons cell.
    /// Practically speaking this must encapsulate a type that itself implements <see cref="ICons{T}"/>
    /// </typeparam>
    internal interface ICons<T> : IPrintable, IEnumerable<T>
    {
        public T Car { get; }
        public T Cdr { get; }
    }

    internal static class IConsExtensions
    {
        public static IEnumerator<T> Enumerate<T>(this ICons<T> cns)
        {
            ICons<T> current = cns;
            yield return current.Car;

            while (current.Cdr is ICons<T> next)
            {
                current = next;
                yield return current.Car;
            }

            yield return current.Cdr;
        }

        public static string Stringify<T>(this ICons<T> cns, Predicate<T> isNil)
        {
            return string.Format("({0}{1})", cns.Car, StringifyCdr(cns.Cdr, isNil));
            //return $"({cns.Car} . {cns.Cdr})";
        }

        private static string StringifyCdr<T>(T exp, Predicate<T> isNil)
        {
            if (isNil(exp)) return string.Empty;
            if (exp is ICons<T> cns) return string.Format(" {0}{1}", cns.Car, StringifyCdr(cns.Cdr, isNil));
            return string.Format(" . {0}", exp);
        }
    }
}

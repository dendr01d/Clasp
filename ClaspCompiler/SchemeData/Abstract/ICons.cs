namespace ClaspCompiler.SchemeData.Abstract
{
    /// <summary>
    /// A structurally recursive scheme object containing two members of a certain expression type.
    /// </summary>
    /// <typeparam name="T">
    /// The expression type held by the cons cell.
    /// Practically speaking this must encapsulate a type that itself implements <see cref="ICons{T}"/>
    /// </typeparam>
    internal interface ICons<T> : ISchemeExp, IEnumerable<T>
        where T : ISchemeExp
    {
        T Car { get; }
        T Cdr { get; }
    }

    internal static class IConsExtensions
    {
        public static IEnumerator<T> Enumerate<T>(this ICons<T> cns)
            where T : ISchemeExp
        {
            ICons<T> current = cns;
            yield return cns.Car;

            while (current.Cdr is ICons<T> next)
            {
                current = next;
                yield return current.Car;
            }

            yield return current.Cdr;
        }

        public static string Stringify<T>(this ICons<T> cns)
            where T : ISchemeExp
        {
            return string.Format("({0}{1})", cns.Car, StringifyCdr(cns.Cdr));
        }

        private static string StringifyCdr<T>(T term)
            where T : ISchemeExp
        {
            if (term.IsNil)
            {
                return string.Empty;
            }
            else if (term is ICons<T> cns)
            {
                return string.Format(" {0}{1}", cns.Car, StringifyCdr(cns.Cdr));
            }
            else
            {
                return string.Format(" . {0}", term);
            }
        }

        public static void Print<T>(this ICons<T> cns, TextWriter writer, int indent)
            where T : ISchemeExp
        {
            writer.Write('(');
            writer.Write(cns.Car, indent);
            PrintCdr(writer, cns.Cdr, indent + 2);
            writer.Write(')');
        }

        private static void PrintCdr<T>(TextWriter writer, T term, int indent)
            where T : ISchemeExp
        {
            if (!term.IsNil)
            {
                writer.WriteLineIndent(indent);

                if (term is ICons<T> cns)
                {
                    writer.Write(cns.Car, indent);
                    PrintCdr(writer, cns.Cdr, indent);
                }
                else
                {
                    writer.Write(term);
                }
            }
        }
    }
}

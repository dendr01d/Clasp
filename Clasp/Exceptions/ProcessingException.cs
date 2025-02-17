using System.Linq;

using Clasp.Data.Terms;

namespace Clasp.Exceptions
{
    public class ProcessingException : ClaspException
    {
        private ProcessingException(string format, params object?[] args) : base(format, args) { }

        public class InvalidPrimitiveArgumentsException : ProcessingException
        {
            internal InvalidPrimitiveArgumentsException(Term arg) : base(
                "Could not process the primitive operation with this argument: {0}",
                string.Format("{0} ({1})", arg, arg.TypeName))
            { }

            internal InvalidPrimitiveArgumentsException(params Term[] args) : base(
                "Could not process the primitive operation with these argument{0}: {1}",
                args.Length == 1 ? string.Empty : "s",
                FormatListItems(args.Select(x => string.Format("{0} ({1})", x, x.TypeName))))
            { }

            internal InvalidPrimitiveArgumentsException() : base(
                "Could not process the primitive operation without any arguments.")
            { }
        }

        public class UnknownNumericType : ProcessingException
        {
            internal UnknownNumericType(params Number[] unknownNumbers) : base(
                "Number arguments to mathematical function are of unknown type(s):\n\t{0}",
                string.Join(", ", unknownNumbers.AsEnumerable()))
            { }
        }
    }
}

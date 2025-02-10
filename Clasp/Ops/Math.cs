using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal class Math
    {
        #region Addition

        public static Term Add(MachineState mx, params Term[] args)
        {
            return args.Length switch
            {
                0 => Integer.Zero,
                1 => args[0],
                2 => Add(mx, args[0], args[1]),
                _ => Helpers.FoldLeft(mx, Add, args)
            };
        }

        public static Term Add(MachineState mx, Term a, Term b)
        {
            if (a is Integer i1 && b is Integer i2) return new Integer(i1.Value + i2.Value);
            else if (a is INumber n1 && b is INumber n2) return new Real(n1.AsDouble + n2.AsDouble);
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(a, b);
        }

        #endregion

        #region Subtraction

        public static Term Subtract(MachineState mx, params Term[] args)
        {
            return args.Length switch
            {
                0 => Integer.Zero,
                1 => Subtract(mx, args[0]),
                2 => Subtract(mx, args[0], args[1]),
                _ => throw new ProcessingException.InvalidPrimitiveArgumentsException(args)
            };
        }

        public static Term Subtract(MachineState mx, Term a)
        {
            if (a is Integer i1) return new Integer(-1 * i1.Value);
            else if (a is INumber n1) return new Real(-1 * n1.AsDouble);
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(a);
        }

        public static Term Subtract(MachineState mx, Term a, Term b)
        {
            if (a is Integer i1 && b is Integer i2) return new Integer(i1.Value - i2.Value);
            else if (a is INumber n1 && b is INumber n2) return new Real(n1.AsDouble - n2.AsDouble);
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(a, b);
        }

        #endregion

        #region Multiplication

        public static Term Multiply(MachineState mx, params Term[] args)
        {
            return args.Length switch
            {
                0 => Integer.One,
                1 => args[0],
                2 => Multiply(mx, args[0], args[1]),
                _ => Helpers.FoldLeft(mx, Multiply, args)
            };
        }

        public static Term Multiply(MachineState mx, Term a, Term b)
        {
            if (a is Integer i1 && b is Integer i2) return new Integer(i1.Value * i2.Value);
            else if (a is INumber n1 && b is INumber n2) return new Real(n1.AsDouble * n2.AsDouble);
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(a, b);
        }

        #endregion

        #region Division
        public static Term Divide(MachineState mx, params Term[] args)
        {
            return args.Length switch
            {
                0 => Integer.One,
                1 => Divide(mx, Integer.One, args[0]),
                2 => Divide(mx, args[0], args[1]),
                _ => throw new ProcessingException.InvalidPrimitiveArgumentsException(args)
            };
        }

        public static Term Divide(MachineState mx, Term a, Term b)
        {
            if (a is INumber n1 && b is INumber n2) return new Real(n1.AsDouble / n2.AsDouble);
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(a, b);
        }

        #endregion
    }
}

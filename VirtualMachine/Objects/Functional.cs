namespace VirtualMachine.Objects
{
    /// <summary>
    /// Represents a first-class function.
    /// </summary>
    internal class Functional
    {
        /// <summary>
        /// The required number of arguments that need to be supplied to invoke the functional.
        /// </summary>
        public readonly int Arity;
        /// <summary>
        /// Whether or not the functional can accept additional, optional arguments during invocation.
        /// </summary>
        public readonly bool Variadic;
        /// <summary>
        /// The instruction pointer corresponding to the body of the functional.
        /// </summary>
        public readonly uint Address;

        /// <summary>
        /// A collection of terms that were implicitly captured by the functional from the surrounding
        /// lexical context at the point of its construction.
        /// </summary>
        public readonly Box[] Closure;

        public Functional(int arity, bool variadic, uint instructionAddress, params Box[] capturedTerms)
        {
            Arity = arity;
            Variadic = variadic;
            Address = instructionAddress;
            Closure = capturedTerms;
        }
    }
}

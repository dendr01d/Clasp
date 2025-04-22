namespace VirtualMachine.Objects
{
    /// <summary>
    /// Represents a contiguous array of <see cref="Term"/>s.
    /// </summary>
    internal class Vector
    {
        public readonly Term[] Elements;

        public Vector(int length)
        {
            Elements = new Term[length];
            Array.Fill(Elements, Term.Undefined);
        }
    }
}

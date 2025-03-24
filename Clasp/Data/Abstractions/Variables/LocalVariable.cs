namespace Clasp.Data.Abstractions.Variables
{
    internal abstract class LocalVariable : AbstractVariable
    {
        public readonly bool IsMutable;
        public readonly bool IsDotted;

        public LocalVariable(string symbolicName, bool mutable, bool dotted) : base(symbolicName)
        {
            IsMutable = mutable;
            IsDotted = dotted;
        }
    }
}

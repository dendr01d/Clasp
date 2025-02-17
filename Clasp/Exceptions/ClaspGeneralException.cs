namespace Clasp.Exceptions
{
    public sealed class ClaspGeneralException : ClaspException
    {
        internal ClaspGeneralException(string format, params object?[] args) : base(format, args) { }
    }
}

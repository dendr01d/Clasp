using System.Diagnostics.CodeAnalysis;

namespace ClaspCompiler
{
    internal interface IPrintable
    {
        /// <summary>
        /// Standard override of <see cref="object.ToString"/> that formats this object as text on a single line.
        /// </summary>
        [MemberNotNull]
        public string ToString();

        /// <summary>
        /// Prints this object to <paramref name="writer"/> with line breaks and indentation.
        /// </summary>
        /// <param name="writer">The mode through which the object will be printed.</param>
        /// <param name="indent">The indentation level (# of spaces) of <paramref name="writer"/> at the point this method is called.</param>
        /// 
        public void Print(TextWriter writer, int indent);
        /// <summary>
        /// Indicates whether or not the printed form of this object may contain line breaks.
        /// </summary>
        public bool CanBreak { get; }
    }
}

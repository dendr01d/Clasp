using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Text;

namespace Clasp.Data.Metadata
{
    public struct SourceLocation
    {
        /// <summary>
        /// The name of the location to which this <see cref="SourceLocation"/> refers.
        /// May be a file path or something else.
        /// </summary>
        public readonly string Source;

        /// <summary>
        /// The line number (1-indexed) within the source text to which this
        /// <see cref="SourceLocation"/> refers.
        /// </summary>
        public readonly int LineNumber;

        /// <summary>
        /// The column number (0-indexed by character) within the source text to which this
        /// <see cref="SourceLocation"/> refers.
        /// </summary>
        public readonly int Column;

        /// <summary>
        /// The first character (1-indexed) of the source text (as an array of characters) to which
        /// this <see cref="SourceLocation"/> refers.
        /// </summary>
        public readonly int StartingPosition;

        /// <summary>
        /// The length of the span of characters within the source text (as an array of characters)
        /// to which this <see cref="SourceLocation"/> refers.
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// Indicates if this is the location of genuine code, or if it only indicates the location from
        /// which generated code was derived.
        /// </summary>
        public readonly bool Original;

        /// <summary>
        /// The full text of <see cref="Source"/>, within which this location exists, if available.
        /// </summary>
        public readonly Blob SourceText;

        /// <summary>
        /// The <see cref="LineNumber"/> normalized to a 0-indexed system.
        /// </summary>
        public int NormalizedLineNumber => LineNumber - 1;

        /// <summary>
        /// The <see cref="StartingPosition"/> normalized to a 0-indexed system.
        /// </summary>
        public int NormalizedStartingPosition => StartingPosition - 1;


        public SourceLocation(string source,
            int lineNumber, int column, int startingPosition, int length,
            Blob text, bool original = true)
        {
            Source = source;
            LineNumber = lineNumber;
            Column = column;
            StartingPosition = startingPosition;
            Length = length;

            SourceText = text;
            Original = original;
        }

        public SourceLocation Derivation() => new SourceLocation(Source, LineNumber, Column, StartingPosition, 0, SourceText, false);
    }
}

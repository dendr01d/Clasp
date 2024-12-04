using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Text
{
    /// <summary>
    /// A collection of lines of text used as input, to be parsed into tokens.
    /// Maintaining a reference back to the input text aids with error-reporting.
    /// </summary>
    public class Blob : IEnumerable<string>
    {
        public readonly string[] Lines;

        public Blob(IEnumerable<string> lines)
        {
            Lines = lines.ToArray();
        }

        public string this[int i] => Lines[i];

        public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)Lines).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Lines.GetEnumerator();
    }
}

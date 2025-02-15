﻿using System;
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
        public readonly string Source;
        public readonly string[] Lines;

        public Blob(string source, IEnumerable<string> lines)
        {
            Source = source;
            Lines = lines.ToArray();
        }

        public string this[int i] => Lines[i];

        public string Extract(int line, int column, int length)
        {
            return Lines[line].Substring(column, length);
        }

        public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)Lines).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Lines.GetEnumerator();
    }
}

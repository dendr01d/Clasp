using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Exceptions
{
    public class PiperException : ClaspException
    {
        private PiperException(string format, params object?[] args) : base(format, args) { }
        private PiperException(Exception innerException, string format, params object?[] args) : base(innerException, format, args) { }

        public class MissingFile : PiperException
        {
            internal MissingFile(string missingFilePath) : base(
                "Couldn't find file:\n\t{0}",
                missingFilePath
                )
            { }
        }
    }
}

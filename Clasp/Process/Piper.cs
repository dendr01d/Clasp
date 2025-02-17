using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Clasp.Exceptions;

namespace Clasp.Process
{
    internal static class Piper
    {
        public static IEnumerable<string> PipeInFileContents(string relativePath)
        {
            string fullPath = Path.GetFullPath(relativePath);
            if (!File.Exists(fullPath))
            {
                throw new PiperException.MissingFile(fullPath);
            }
            return File.ReadAllLines(fullPath);
        }
    }
}

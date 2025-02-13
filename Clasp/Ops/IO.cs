using System;
using System.IO;
using System.Linq;

using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class IO
    {
        public static void Display(params Term[] terms)
        {
            System.Console.WriteLine(string.Join(' ', terms.Select(x => x.ToString())));
        }

        // this is getting out of hand...

        public static CoreForm Import(MachineState mx, CharString cs)
        {
            string path = Path.GetFullPath(cs.Value);

            if (File.Exists(path))
            {
                string fullText = string.Format("(begin {0})", File.ReadAllText(path));

                mx.ParentProcess.Interpret()
            }
        }




    }
}

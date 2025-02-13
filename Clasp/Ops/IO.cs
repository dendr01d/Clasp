using System;
using System.IO;
using System.Linq;

using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Process;

namespace Clasp.Ops
{
    internal static class IO
    {
        public static VoidTerm Display(MachineState mx, params Term[] terms)
        {
            Console.WriteLine(string.Join(' ', terms.Select(x => x.ToString())));
            return VoidTerm.Value;
        }

        public static VoidTerm Import(MachineState mx, CharString cs)
        {
            if (mx.CurrentEnv != mx.CurrentEnv.TopLevel)
            {
                throw new ProcessingException.SemanticError("Files may only be imported at the top level of execution.");
            }

            string path = Path.GetFullPath(cs.Value);

            if (!File.Exists(path))
            {
                throw new ProcessingException.SemanticError("Could not find the file: {0}", path);
            }

            string fullText = string.Format("(begin {0} )", File.ReadAllText(path));
            CoreForm output = Processor.ParseText(path, fullText, mx.CurrentEnv);

            if (output is not SequentialForm sf)
            {
                throw new ClaspGeneralException("Somehow parsed imported 'begin' form to a different core form?");
            }

            mx.Continuation.Push(output);

            return VoidTerm.Value;
        }




    }
}

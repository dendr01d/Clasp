using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;
using Clasp.Process;

namespace Clasp.Ops
{
    internal static class PortOps
    {
        public static Port OpenConsoleOut() => new PortWriter("Console-Out", Console.OpenStandardOutput());
        public static Port OpenConsoleIn() => new PortReader("Console-In", Console.OpenStandardInput());

        public static Port OpenFileWriter(CharString filePath)
        {
            return new PortWriter(filePath.Value, File.Open(filePath.Value, FileMode.Create));
        }

        public static Port OpenFileReader(CharString filePath)
        {
            return new PortReader(filePath.Value, File.Open(filePath.Value, FileMode.OpenOrCreate));
        }

        public static Term Write(PortWriter port, Term t)
        {
            port.Stream.Write(t.ToPrintedString());
            return VoidTerm.Value;
        }

        //TODO this should probably return a multi-value, aye?
        public static Term Read(PortReader port)
        {
            return Reader.ReadTokenText(Lexer.LexText(port.Name, port.Stream.ReadToEnd()));
        }
    }
}

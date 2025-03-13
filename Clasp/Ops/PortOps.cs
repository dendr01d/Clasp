using System;
using System.Text;

using Clasp.Data.Terms;
using Clasp.Process;

namespace Clasp.Ops
{
    internal static class PortOps
    {
        public static Port OpenConsoleOut()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            return new PortWriter("Console-Out", Console.Out);
        }
        public static Port OpenConsoleIn()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            return new PortReader("Console-In", Console.In);
        }

        //public static Port OpenFileWriter(CharString filePath)
        //{
        //    return new PortWriter(filePath.Value, File.Open(filePath.Value, FileMode.Create));
        //}

        //public static Port OpenFileReader(CharString filePath)
        //{
        //    return new PortReader(filePath.Value, File.Open(filePath.Value, FileMode.OpenOrCreate));
        //}

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

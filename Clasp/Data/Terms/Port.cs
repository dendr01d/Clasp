using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Terms
{
    internal abstract class Port : Atom
    {
        public readonly string Name;
        protected Port(string name) => Name = name;
        public override string ToString() => $"#{{{Name}}}";
    }

    internal sealed class PortReader : Port
    {
        public readonly StreamReader Stream;
        public PortReader(string name, Stream stream) : base(name)
        {
            Stream = new StreamReader(stream);
        }
        protected override string FormatType() => nameof(PortReader);
    }

    internal sealed class PortWriter : Port
    {
        public readonly StreamWriter Stream;
        public PortWriter(string name, Stream stream) : base(name)
        {
            Stream = new StreamWriter(stream);
        }
        protected override string FormatType() => nameof(PortWriter);
    }
}

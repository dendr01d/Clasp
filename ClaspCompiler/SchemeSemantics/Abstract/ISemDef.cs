using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemDef : ISemTop
    {
        public ISemVar Variable { get; }
        public ISemExp Value { get; }
    }
}

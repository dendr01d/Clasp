using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.IntermediateCps.Abstract
{
    internal interface IControlStructure : IPrintable
    {
        public string ControlCode { get; }
        public Dictionary<Var, int> FreeVariables { get; }
    }
}

using ClaspCompiler.CompilerData;

namespace ClaspCompiler.IntermediateCps.Abstract
{
    internal interface IControlStructure : IPrintable
    {
        public string ControlCode { get; }
        public Dictionary<VarBase, int> FreeVariables { get; }
    }
}

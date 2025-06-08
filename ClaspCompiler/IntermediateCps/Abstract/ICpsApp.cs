namespace ClaspCompiler.IntermediateCps.Abstract
{
    internal interface ICpsApp : ICpsExp
    {
        public bool IOBound { get; }
        public ICpsExp[] Arguments { get; }
    }
}

namespace ClaspCompiler.IntermediateCLang.Abstract
{
    internal interface INormApp : INormExp
    {
        public string Operator { get; }
        public INormArg[] Arguments { get; }
    }
}

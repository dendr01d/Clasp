namespace ClaspCompiler.SchemeSemantics.Abstract
{
    internal interface ISemFormals : IPrintable
    {
        public ISemVar[] Parameters { get; }
        public ISemVar? VarParam { get; }
    }
}

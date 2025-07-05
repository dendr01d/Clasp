namespace ClaspCompiler.Text
{
    internal abstract record AcquisitionMethod
    {
    }

    internal sealed record CoreDefinition : AcquisitionMethod
    {
        public static readonly CoreDefinition Instance = new();
        private CoreDefinition() { }
    }

    internal sealed record ReadFromFile(string FileName) : AcquisitionMethod { }

    internal sealed record ImportedFromModule(string ModuleName, SourceRef ModuleSource) { }

    internal sealed record TransformedByMacro(string MacroName, SourceRef PreTransformationForm) { }
}

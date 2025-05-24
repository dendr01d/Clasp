using ClaspCompiler.Common;

namespace ClaspCompiler.Semantics
{
    internal sealed class Application : ApplicationBase<ISemExp>, ISemExp
    {
        public Application(ISemExp op, params ISemExp[] args) : base(op, args) { }
    }
}

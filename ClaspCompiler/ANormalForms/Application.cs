using ClaspCompiler.Common;

namespace ClaspCompiler.ANormalForms
{
    internal sealed class Application : ApplicationBase<INormExp>, INormExp
    {
        public Application(INormExp op, IEnumerable<INormExp> args) : base(op, args.ToArray()) { }
    }
}

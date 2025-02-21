using Clasp.Binding;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Modules
{
    internal abstract class Module
    {
        public readonly string Name;

        protected Module(string name) => Name = name;
    }

    internal abstract class InstantiatedModule : Module
    {
        public readonly Scope OutsideEdge;
        public readonly Identifier[] ExportedNames;

        protected InstantiatedModule(string name, Scope outside, Identifier[] ids) : base(name)
        {
            OutsideEdge = outside;
            ExportedNames = ids;
        }
    }
}

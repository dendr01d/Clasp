using System.IO;

using Clasp.Binding;
using Clasp.Binding.Environments;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Exceptions;

namespace Clasp.Modules
{
    internal abstract class Module
    {
        public readonly string Name;

        protected Module(string name) => Name = name;

        private static Module LoadFromFile(string name, string path)
        {
            if (StaticEnv.TryUncacheModule(name, out Module? mdl))
            {
                return mdl;
            }
            else if (File.Exists(path))
            {
                return FreshModule.Read(name, path);
            }
            else
            {
                throw new ClaspGeneralException("Module file not found: {0}", path);
            }
        }

        public static Module LoadModule(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);

            return LoadFromFile(name, path);
        }

        const string ROOT = @"%USERPROFILE%\Source\Repos\ClaspSource\";

        public static Module ImportModule(string moduleName)
        {
            string fileName = Path.ChangeExtension(moduleName, "clsp");
            //TODO this should be parameterized somehow
            string path = Path.Combine(ROOT, fileName);

            return LoadFromFile(moduleName, path);
        }

        public static Module ExpandModule(string moduleName)
        {
            if (!StaticEnv.TryUncacheModule(moduleName, out Module? mdl))
            {
                mdl = ImportModule(moduleName);
            }

            if (mdl is FreshModule fm)
            {
                return ExpandedModule.Expand(fm);
            }
            else
            {
                return mdl;
            }
        }

        public static Module ParseModule(string moduleName)
        {
            if (!StaticEnv.TryUncacheModule(moduleName, out Module? mdl)
                || mdl is not ExpandedModule)
            {
                mdl = ExpandModule(moduleName);
            }

            if (mdl is ExpandedModule em)
            {
                return ParsedModule.Parse(em);
            }
            else
            {
                return mdl;
            }
        }

        public static InterpretedModule InterpretModule(string moduleName)
        {
            if (!StaticEnv.TryUncacheModule(moduleName, out Module? mdl))
            {
                mdl = ParseModule(moduleName);
            }

            if (mdl is ParsedModule pm)
            {
                return InterpretedModule.Interpret(pm);
            }
            else
            {
                return (InterpretedModule)mdl; // TODO handle this more elegantly
            }
        }
    }

    internal abstract class ProcessedModule : Module
    {
        public readonly Identifier[] ExportedIds;

        protected ProcessedModule(string name, Identifier[] ids) : base(name)
        {
            ExportedIds = ids;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Exceptions;
using Clasp.Process;

namespace Clasp.Binding.Modules
{
    internal abstract class Module
    {
        public readonly string Name;
        public abstract bool Visited { get; }
        public abstract bool Instantiated { get; }

        protected Module(string name)
        {
            Name = name;
        }

        public static bool CircularReference(string moduleName)
        {
            return ModuleCache.TryGet(moduleName, out Module? mdl)
                && !mdl.Visited;
        }

        /// <summary>
        /// Declare that a module must exist, loading it into the cache without visiting or instantiating it.
        /// </summary>
        public static void Declare(string moduleName)
        {
            if (!ModuleCache.Contains(moduleName))
            {
                string fileName = Path.ChangeExtension(moduleName, RuntimeParams.FILE_EXT);
                string filePath = Path.GetFullPath(Path.Combine(RuntimeParams.LIBRARY_REPO_DIR, fileName));

                Syntax moduleStx = Reader.ReadModuleForm(Lexer.LexFile(filePath));

                DeclaredModule mdl = new DeclaredModule(moduleName, moduleStx);
                ModuleCache.Update(mdl);
            }
        }

        /// <summary>
        /// Update a cached module by "visiting" it and expanding/parsing its syntax as needed.
        /// This will <see cref="Declare(string)"/> the module if it is undeclared.
        /// </summary>
        public static void Visit(string moduleName)
        {
            if (CircularReference(moduleName))
            {
                throw new ClaspGeneralException("Tried to visit module '{0}', which is already in the process of being visited.", moduleName);
            }

            Declare(moduleName);
            Module mdl = ModuleCache.Get(moduleName);

            if (mdl is DeclaredModule dMdl)
            {
                CompilationContext ctx = CompilationContext.ForModule();

                Syntax expandedStx = Expander.ExpandAnticipatedForm(Keywords.MODULE, dMdl.FreshSyntax, ctx);
                CoreForm parsedStx = Parser.ParseSyntax(expandedStx, 1);

                VisitedModule vMdl = new VisitedModule(moduleName, parsedStx, ctx.CollectedIdentifiers.ToArray());
                ModuleCache.Update(vMdl);
            }
        }

        /// <summary>
        /// Instantiate a module by evaluating its contents and ensuring all exported names have corresponding definitions.
        /// This will <see cref="Visit(string)"/> the module if it is unvisited, but it's an error to instantiate an undeclared module.
        public static void Instantiate(string moduleName)
        {
            if (!ModuleCache.Contains(moduleName))
            {
                throw new ClaspGeneralException("Tried to instantiate module '{0}' before it has been declared.", moduleName);
            }

            Visit(moduleName);
            Module mdl = ModuleCache.Get(moduleName);

            if (mdl is VisitedModule vMdl)
            {
                RootEnv env = new RootEnv();
                Interpreter.InterpretProgram(vMdl.ParsedForm, env);

                IEnumerable<string> missingNames = vMdl.ExportedSymbols
                    .Select(x => x.Name)
                    .Where(x => !env.ContainsKey(x));

                if (missingNames.Any())
                {
                    throw new ClaspGeneralException("Instantiating module '{0}' failed to produce definitions for these exported names: {0}",
                        string.Join(", ", missingNames.Select(x => $"'{x}'")));
                }

                InstantiatedModule iMdl = new InstantiatedModule(moduleName, env);
                ModuleCache.Update(iMdl);
            }
        }
    }
}

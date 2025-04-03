using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Clasp.AbstractMachine;
using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
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

        public static string NameFromPath(string moduleFilePath)
        {
            string relative = Path.GetRelativePath(RuntimeParams.LIBRARY_REPO_DIR, moduleFilePath);
            return Path.ChangeExtension(relative, null);
        }

        public static bool DetectCircularReference(string moduleName)
        {
            return ModuleCache.TryGet(moduleName, out Module? mdl) && !mdl.Visited;
        }

        /// <summary>
        /// Declare that a module must exist, loading it into the cache without visiting or instantiating it.
        /// </summary>
        private static void Declare(string moduleName)
        {
            if (!ModuleCache.Contains(moduleName))
            {
                string fileName = Path.ChangeExtension(moduleName, RuntimeParams.FILE_EXT);
                string filePath = Path.Combine(RuntimeParams.LIBRARY_REPO_DIR, fileName);

                Syntax moduleBody = Reader.ReadTokenSyntax(Lexer.LexFile(filePath));
                DeclaredModule mdl = new DeclaredModule(moduleName, moduleBody);
                ModuleCache.Update(mdl);
            }
        }

        public static ParsedModule Visit(string moduleName, Syntax moduleStx)
        {
            moduleStx = moduleStx.StripScopes(0);
            
            if (ModuleCache.Contains(moduleName))
            {
                throw new ClaspGeneralException("Tried to declare syntactic module '{0}' that already exists in cache.", moduleName);
            }
            else
            {
                DeclaredModule mdl = new DeclaredModule(moduleName, moduleStx);
                ModuleCache.Update(mdl);
                return Visit(moduleName);
            }
        }

        /// <summary>
        /// Update a cached module by "visiting" it and expanding/parsing its syntax as needed.
        /// This will <see cref="Declare(string)"/> the module if it is undeclared.
        /// </summary>
        public static ParsedModule Visit(string moduleName)
        {
            Declare(moduleName);
            Module mdl = ModuleCache.Get(moduleName);

            if (mdl is ParsedModule pMdl)
            {
                return pMdl;
            }
            else if (mdl is DeclaredModule dMdl)
            {
                Syntax stx = dMdl.FreshSyntax;

                Scope outsideEdge = new Scope($"{Keywords.MODULE} '{moduleName}' Outside-Edge", stx.Location);
                Scope insideEdge = new Scope($"{Keywords.MODULE} '{moduleName}' Inside-Edge", stx.Location);

                CompilationContext bodyContext = CompilationContext.ForModule(insideEdge);

                stx.AddScope(bodyContext.Phase, outsideEdge, insideEdge);

                Syntax visitationStx = Syntax.WrapWithRef(Cons.Truct(Symbols.S_VisitModule, stx), stx);
                Syntax expandedStx = Expander.Expand(visitationStx, bodyContext);
                CoreForm parsedStx = Parser.ParseSyntax(expandedStx, 1);

                VisitedModule vMdl = new VisitedModule(moduleName, parsedStx, bodyContext.CollectedIdentifiers.ToArray(), insideEdge);
                ModuleCache.Update(vMdl);

                return vMdl;
            }

            throw new ClaspGeneralException("Unknown module type???");
        }

        /// <summary>
        /// Instantiate a module by evaluating its contents and ensuring all exported names have corresponding definitions.
        /// This will <see cref="Visit(string)"/> the module if it is unvisited, but it's an error to instantiate an undeclared module.
        public static Term Instantiate(string moduleName, Action<int, MachineState>? postStepHook = null)
        {
            //if (!ModuleCache.Contains(moduleName))
            //{
            //    throw new ClaspGeneralException("Tried to instantiate module '{0}' before it has been declared.", moduleName);
            //}

            Visit(moduleName);
            Module mdl = ModuleCache.Get(moduleName);

            if (mdl is VisitedModule vMdl)
            {
                RootEnv env = new RootEnv();
                Term output = Interpreter.InterpretProgram(vMdl.ParsedForm, env, postStepHook);

                List<Symbol> missingSymbols = new List<Symbol>();

                foreach (Identifier exportedId in vMdl.ExportedIds)
                {
                    if (!exportedId.TryResolveBinding(1, out RenameBinding? binding))
                    {
                        missingSymbols.Add(exportedId.Expose());
                    }
                    else if (!env.ContainsKey(binding.Name))
                    {
                        missingSymbols.Add(binding.BindingSymbol);
                    }
                }

                if (missingSymbols.Any())
                {
                    throw new ClaspGeneralException("Instantiating module '{0}' failed to produce definitions for these exported names: {1}",
                        moduleName,
                        string.Join(", ", missingSymbols.Select(x => $"'{x.Name}'")));
                }

                InstantiatedModule iMdl = new InstantiatedModule(moduleName, vMdl.ExportedIds, vMdl.ExportedScope, env);
                ModuleCache.Update(iMdl);

                return output;
            }

            return VoidTerm.Value;
        }
    }
}

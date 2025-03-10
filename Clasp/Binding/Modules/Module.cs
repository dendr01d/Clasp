﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Static;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;
using Clasp.Data.Terms.SyntaxValues;
using Clasp.Exceptions;
using Clasp.Process;

using static System.Net.WebRequestMethods;

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
            return Path.GetFileNameWithoutExtension(moduleFilePath);
        }

        public static bool DetectCircularReference(string moduleName)
        {
            return ModuleCache.TryGet(moduleName, out Module? mdl) && !mdl.Visited;
        }

        /// <summary>
        /// Declare that a module must exist, loading it into the cache without visiting or instantiating it.
        /// </summary>
        public static void Declare(string moduleFilePath)
        {
            string moduleName = NameFromPath(moduleFilePath);

            if (!ModuleCache.Contains(moduleName))
            {
                string fileName = Path.ChangeExtension(moduleName, RuntimeParams.FILE_EXT);
                string filePath = Path.Combine(RuntimeParams.LIBRARY_REPO_DIR, fileName);

                Syntax moduleBody = Reader.ReadTokens(Lexer.LexFile(filePath));
                Declare(moduleName, moduleBody);
            }
        }

        public static void Declare(string moduleName, Syntax moduleStx)
        {
            DeclaredModule mdl = new DeclaredModule(moduleName, moduleStx);
            ModuleCache.Update(mdl);
        }

        /// <summary>
        /// Update a cached module by "visiting" it and expanding/parsing its syntax as needed.
        /// This will <see cref="Declare(string)"/> the module if it is undeclared.
        /// </summary>
        public static Scope? Visit(string moduleName)
        {
            Declare(moduleName);
            Module mdl = ModuleCache.Get(moduleName);

            if (mdl is DeclaredModule dMdl)
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

                return insideEdge;
            }

            return null;
        }

        /// <summary>
        /// Instantiate a module by evaluating its contents and ensuring all exported names have corresponding definitions.
        /// This will <see cref="Visit(string)"/> the module if it is unvisited, but it's an error to instantiate an undeclared module.
        public static Term Instantiate(string moduleName)
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
                Term output = Interpreter.InterpretProgram(vMdl.ParsedForm, env);

                List<Symbol> missingSymbols = new List<Symbol>();

                foreach(Identifier exportedId in vMdl.ExportedIds)
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

                InstantiatedModule iMdl = new InstantiatedModule(moduleName, env);
                ModuleCache.Update(iMdl);

                return output;
            }

            return VoidTerm.Value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData;
using ClaspCompiler.LexicalScopes;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeSyntax;
using ClaspCompiler.SchemeSyntax.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class PaintScopeInfo
    {
        public static ISyntax Execute(ISyntax stx)
        {
            uint scopeCounter = 0;
            ScopeSet scopes = new ScopeSet(0, scopeCounter++);
            ScopeMap map = new ScopeMap();

            map[scopes] = CreateDefaultBindings();

            ISyntax result = PaintSyntax(stx, map, scopes, ref scopeCounter);

            return result;
        }

        private static readonly string[] _defaultNames = new string[]
        {
            "read",
            "+", "-",
        };

        private static readonly string[] _specialNames = new string[]
        {
            "let"
        };

        private static RenameRecord CreateDefaultBindings()
        {
            RenameRecord rec = new RenameRecord();

            foreach(string name in _defaultNames.Concat(_specialNames))
            {
                Symbol sym = new Symbol(name);
                rec.Add(sym, sym);
            }

            return rec;
        }

        private static ISyntax PaintSyntax(ISyntax stx, ScopeMap map, ScopeSet scopes, ref uint scopeCounter)
        {
            return stx switch
            {
                StxPair stp => PaintPair(stp, map, scopes, ref scopeCounter),
                StxDatum std => PaintDatum(std, map, scopes, ref scopeCounter),
                Identifier id => PaintIdentifier(id, map, scopes, ref scopeCounter),
                _ => throw new Exception($"Can't paint scopes of: {stx}")
            };
        }

        private static ISyntax PaintPair(StxPair stp, ScopeMap map, ScopeSet scopes, ref uint scopeCounter)
        {
            ISyntax op = PaintSyntax(stp.Car, map, scopes, ref scopeCounter);

            if (op is Identifier id && _specialNames.Contains(id.BindingName.Name))
            {
                return PaintSpecialForm(id, stp.Cdr, map, scopes, ref scopeCounter);
            }

            return PaintGenericApplication(op, stp.Cdr, map, scopes, ref scopeCounter);
        }

        private static Identifier PaintIdentifier(Identifier id, ScopeMap map, ScopeSet scopes, ref uint scopeCounter)
        {
            if (map.TryGetValue(scopes, id.SymbolicName, out Symbol? bindingName))
            {
                return new Identifier(id, bindingName);
            }
            return id;
        }

        private static ISyntax PaintDatum(StxDatum std, ScopeMap map, ScopeSet scopes, ref uint scopeCounter)
        {
            return std;
        }

        private static ISyntax PaintGenericApplication(ISyntax op, ISyntax args, ScopeMap map, ScopeSet scopes, ref uint scopeCounter)
        {
            return new StxPair(op, PaintGenericArgs(args, map, scopes, ref scopeCounter));
        }

        private static ISyntax PaintGenericArgs(ISyntax args, ScopeMap map, ScopeSet scopes, ref uint scopeCounter)
        {
            return args switch
            {
                StxPair stp => new StxPair(
                    PaintSyntax(stp.Car, map, scopes, ref scopeCounter),
                    PaintGenericArgs(stp.Cdr, map, scopes, ref scopeCounter)),
                StxDatum std => PaintDatum(std, map, scopes, ref scopeCounter),
                Identifier id => PaintIdentifier(id, map, scopes, ref scopeCounter),
                _ => throw new Exception($"Can't paint scopes of args: {args}")
            };
        }

        private static ISyntax PaintSpecialForm(Identifier op, ISyntax args, ScopeMap map, ScopeSet scopes, ref uint scopeCounter)
        {
            return op.BindingName.Name switch
            {
                "let" => PaintLet(op, args, map, scopes, ref scopeCounter),
                _ => throw new Exception($"Can't paint unrecognized special form: {op}")
            };
        }

        private static ISyntax PaintLet(ISyntax op, ISyntax args, ScopeMap map, ScopeSet scopes, ref uint scopeCounter)
        {
            throw new NotImplementedException();
        }
    }
}

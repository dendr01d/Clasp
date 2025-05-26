using ClaspCompiler.Common;
using ClaspCompiler.Semantics;

namespace ClaspCompiler.CompilerPasses
{
    internal static class Uniquify
    {
        public static ProgR1 Execute(ProgR1 program)
        {
            Dictionary<Var, Var> map = new();

            return new ProgR1(program.Info, MapThroughExpression(program.Body, map));
        }

        private static ISemExp MapThroughExpression(ISemExp exp, Dictionary<Var, Var> map)
        {
            return exp switch
            {
                IAtom lit => MapLiteral(lit, map),
                Let let => MapThroughLet(let, map),
                Application app => MapThroughApplication(app, map),
                _ => throw new Exception($"Can't map variables in expression: {exp}")
            };
        }

        private static IAtom MapLiteral(IAtom lit, Dictionary<Var, Var> map)
        {
            if (lit is Var var && map.TryGetValue(var, out Var? newVar))
            {
                return newVar;
            }

            return lit;
        }

        private static Let MapThroughLet(Let let, Dictionary<Var, Var> map)
        {
            ISemExp newArg = MapThroughExpression(let.Argument, map);

            Dictionary<Var, Var> newMap = new(map);

            Var newVar = Var.GenVar(let.Variable);
            newMap[let.Variable] = newVar;

            ISemExp newBody = MapThroughExpression(let.Body, newMap);

            return new Let(
                newVar,
                newArg,
                newBody);
        }

        private static Application MapThroughApplication(Application app, Dictionary<Var, Var> map)
        {
            return new Application(
                MapThroughExpression(app.Operator, map),
                app.Arguments.Select(x => MapThroughExpression(x, map)).ToArray());
        }
    }
}

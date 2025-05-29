using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class Uniquify
    {
        public static ProgR1 Execute(ProgR1 program)
        {
            Dictionary<Var, Var> map = [];

            return new ProgR1(program.Info, MapThroughExpression(program.Body, map));
        }

        private static ISemanticExp MapThroughExpression(ISemanticExp exp, Dictionary<Var, Var> map)
        {
            return exp switch
            {
                Var var => MapVariable(var, map),
                Let let => MapThroughLet(let, map),
                Application app => MapThroughApplication(app, map),
                _ => exp
            };
        }

        private static Var MapVariable(Var variable, Dictionary<Var, Var> map)
        {
            return map.TryGetValue(variable, out Var? result)
                ? result
                : variable;
        }

        private static Let MapThroughLet(Let let, Dictionary<Var, Var> map)
        {
            ISemanticExp newArg = MapThroughExpression(let.Argument, map);

            Dictionary<Var, Var> newMap = new(map);

            Var newVar = Var.Gen(let.Variable.Name);
            newMap[let.Variable] = newVar;

            ISemanticExp newBody = MapThroughExpression(let.Body, newMap);

            return new Let(
                newVar,
                newArg,
                newBody);
        }

        private static Application MapThroughApplication(Application app, Dictionary<Var, Var> map)
        {
            return new Application(
                app.Operator,
                app.Arguments.Select(x => MapThroughExpression(x, map)).ToArray());
        }
    }
}

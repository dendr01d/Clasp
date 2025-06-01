using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateCLang;
using ClaspCompiler.IntermediateCLang.Abstract;
using ClaspCompiler.SchemeData;

namespace ClaspCompiler.CompilerPasses
{
    internal static class TypeCheckVars
    {
        public static ProgC0 Execute(ProgC0 program)
        {
            Dictionary<Var, Type> types = [];

            foreach (ITail tail in program.LabeledTails.Values)
            {
                TypeCheckTail(tail, types);
            }

            return new ProgC0(types, program.LabeledTails);
        }

        private static Type TypeCheckTail(ITail tail, Dictionary<Var, Type> types)
        {
            if (tail is Sequence seq)
            {
                TypeCheckStatement(seq.Statement, types);
                return TypeCheckTail(seq.Tail, types);
            }
            else if (tail is Return ret)
            {
                return TypeCheckExpression(ret.Value, types);
            }

            throw new Exception($"Can't check types in tail: {tail}");
        }

        private static Type TypeCheckStatement(IStatement stmt, Dictionary<Var, Type> types)
        {
            if (stmt is Assignment asmt)
            {
                Type assignedType = TypeCheckExpression(asmt.Value, types);

                if (types.TryGetValue(asmt.Variable, out Type? type))
                {
                    if (type != assignedType)
                    {
                        throw new Exception(string.Format("Type mismatch assigning to ({0}) {1}: {2}",
                            type.ToString().ToLower(),
                            asmt.Variable,
                            asmt.Value));
                    }
                }
                else
                {
                    types.Add(asmt.Variable, assignedType);
                }

                return assignedType;
            }

            throw new Exception($"Can't check types of statement: {stmt}");
        }

        private static Type TypeCheckArgument(INormArg arg, Dictionary<Var, Type> types)
        {
            if (arg is Var var && types.TryGetValue(var, out Type? type))
            {
                return type;
            }

            return arg switch
            {
                Integer => typeof(int),
                Var => typeof(Var),
                _ => throw new Exception($"Can't check type of arg: {arg}")
            };
        }

        private static Type TypeCheckExpression(INormExp exp, Dictionary<Var, Type> types)
        {
            if (exp is INormArg atm)
            {
                return TypeCheckArgument(atm, types);
            }
            else if (exp is Application app)
            {
                switch (app.Operator)
                {
                    case "+":
                        app.Arguments.ToList().ForEach(x => AssertType(x, typeof(int), types));
                        return typeof(int);

                    case "-":
                        app.Arguments.ToList().ForEach(x => AssertType(x, typeof(int), types));
                        return typeof(int);

                    case "read":
                        return typeof(int);

                    default:
                        throw new Exception($"Can't check type of unknown application: {app}");
                }
            }

            throw new Exception($"Can't check type of expression: {exp}");
        }

        private static void AssertType(INormExp arg, Type expectedType, Dictionary<Var, Type> types)
        {
            if (arg is Var v && !types.ContainsKey(v))
            {
                types[v] = expectedType;
            }
            else if (expectedType != TypeCheckExpression(arg, types))
            {
                throw new Exception($"Expected type of {expectedType}: {arg}");
            }
        }
    }
}

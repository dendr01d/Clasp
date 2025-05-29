using ClaspCompiler.IntermediateAnfLang.Abstract;
using ClaspCompiler.IntermediateAnfLang;
using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeData.Abstract;

namespace ClaspCompiler.CompilerPasses
{
    internal static class TypeCheckVars
    {
        public static ProgC0 Execute(ProgC0 program)
        {
            Dictionary<Var, SchemeType> types = [];

            foreach (ITail tail in program.LabeledTails.Values)
            {
                TypeCheckTail(tail, types);
            }

            return new ProgC0(types, program.LabeledTails);
        }

        private static SchemeType TypeCheckTail(ITail tail, Dictionary<Var, SchemeType> types)
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

        private static SchemeType TypeCheckStatement(IStatement stmt, Dictionary<Var, SchemeType> types)
        {
            if (stmt is Assignment asmt)
            {
                SchemeType assignedType = TypeCheckExpression(asmt.Value, types);

                if (types.TryGetValue(asmt.Variable, out SchemeType type))
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

        private static SchemeType TypeCheckArgument(INormArg arg, Dictionary<Var, SchemeType> types)
        {
            if (arg is Var var && types.TryGetValue(var, out SchemeType type))
            {
                return type;
            }
            else if (arg is IAtom atm)
            {
                return atm.TypeName;
            }

            throw new Exception($"Can't check type of arg: {arg}");
        }

        private static SchemeType TypeCheckExpression(INormExp exp, Dictionary<Var, SchemeType> types)
        {
            if (exp is INormArg atm)
            {
                return TypeCheckArgument(atm, types);
            }
            else if (exp is Application app
                && app.Operator is Var var)
            {
                switch (var.Name.Name)
                {
                    case "+":
                        app.Arguments.ToList().ForEach(x => AssertType(x, SchemeType.Integer, types));
                        return SchemeType.Integer;

                    case "-":
                        app.Arguments.ToList().ForEach(x => AssertType(x, SchemeType.Integer, types));
                        return SchemeType.Integer;

                    case "read":
                        return SchemeType.Integer;

                    default:
                        throw new Exception($"Can't check type of unknown application: {app}");
                }
            }

            throw new Exception($"Can't check type of expression: {exp}");
        }

        private static void AssertType(INormExp arg, SchemeType expectedType, Dictionary<Var, SchemeType> types)
        {
            if (expectedType != TypeCheckExpression(arg, types))
            {
                throw new Exception($"Expected type of {expectedType.ToString().ToLower()}: {arg}");
            }
        }
    }
}

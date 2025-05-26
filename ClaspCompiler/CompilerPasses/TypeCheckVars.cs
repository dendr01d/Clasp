using ClaspCompiler.ANormalForms;
using ClaspCompiler.Common;
using ClaspCompiler.Data;

namespace ClaspCompiler.CompilerPasses
{
    internal static class TypeCheckVars
    {
        public static ProgC0 Execute(ProgC0 program)
        {
            Dictionary<Var, TypeName> types = [];

            foreach (ITail tail in program.LabeledTails.Values)
            {
                TypeCheckTail(tail, types);
            }

            return new ProgC0(types, program.LabeledTails);
        }

        private static TypeName TypeCheckTail(ITail tail, Dictionary<Var, TypeName> types)
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

        private static TypeName TypeCheckStatement(IStatement stmt, Dictionary<Var, TypeName> types)
        {
            if (stmt is Assign asmt)
            {
                TypeName assignedType = TypeCheckExpression(asmt.Value, types);

                if (types.TryGetValue(asmt.Variable, out TypeName type))
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

        private static TypeName TypeCheckArgument(INormArg arg, Dictionary<Var, TypeName> types)
        {
            if (arg is Var var && types.TryGetValue(var, out TypeName type))
            {
                return type;
            }
            else if (arg is IAtom atm)
            {
                return atm.TypeName;
            }

            throw new Exception($"Can't check type of arg: {arg}");
        }

        private static TypeName TypeCheckExpression(INormExp exp, Dictionary<Var, TypeName> types)
        {
            if (exp is INormArg atm)
            {
                return TypeCheckArgument(atm, types);
            }
            else if (exp is Application app
                && app.Operator is Var var)
            {
                switch (var.Data.Name)
                {
                    case "+":
                        app.Arguments.ToList().ForEach(x => AssertType(x, TypeName.Int, types));
                        return TypeName.Int;

                    case "-":
                        app.Arguments.ToList().ForEach(x => AssertType(x, TypeName.Int, types));
                        return TypeName.Int;

                    case "read":
                        return TypeName.Int;

                    default:
                        throw new Exception($"Can't check type of unknown application: {app}");
                }
            }

            throw new Exception($"Can't check type of expression: {exp}");
        }

        private static void AssertType(INormExp arg, TypeName expectedType, Dictionary<Var, TypeName> types)
        {
            if (expectedType != TypeCheckExpression(arg, types))
            {
                throw new Exception($"Expected type of {expectedType.ToString().ToLower()}: {arg}");
            }
        }
    }
}

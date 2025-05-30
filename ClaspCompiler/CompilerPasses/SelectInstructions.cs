﻿using ClaspCompiler.ANormalForms;
using ClaspCompiler.Common;
using ClaspCompiler.PseudoIl;

namespace ClaspCompiler.CompilerPasses
{
    internal sealed class SelectInstructions
    {
        public static ProgIl0 Execute(ProgC0 program)
        {
            Dictionary<Label, Block> labeledBlocks = [];

            foreach (var pair in program.LabeledTails)
            {
                Label label = new(pair.Key);
                Block block = new([],
                    SelectTail(pair.Value).ToArray());

                labeledBlocks.Add(label, block);
            }

            var localVars = program.LocalVariables.ToDictionary(x => (IMem)x.Key, x => x.Value);

            return new ProgIl0(localVars, labeledBlocks);
        }

        private static IEnumerable<IInstruction> SelectTail(ITail tail)
        {
            if (tail is Sequence seq)
            {
                return SelectStatement(seq.Statement)
                    .Concat(SelectTail(seq.Tail));
            }
            else if (tail is Return ret)
            {
                return SelectExpression(ret.Value)
                    .Append(new Instruction(PseudoOp.Return));
            }

            throw new Exception($"Can't select instructions from tail: {tail}");
        }

        private static IEnumerable<IInstruction> SelectStatement(IStatement stmt)
        {
            if (stmt is Assign asmt)
            {
                return SelectExpression(asmt.Value)
                    .Append(new Instruction(PseudoOp.Store, asmt.Variable));
            }

            throw new Exception($"Can't select instructions from statement: {stmt}");
        }

        private static IInstruction SelectArgument(INormArg arg)
        {
            if (arg is Var var)
            {
                return new Instruction(PseudoOp.Load, var);
            }
            else if (arg is IAtom lit)
            {
                return new Instruction(PseudoOp.Load, lit);
            }

            throw new Exception($"Can't select instruction for unknown arg type: {arg}");
        }

        private static IEnumerable<IInstruction> SelectExpression(INormExp exp)
        {
            if (exp is INormArg arg)
            {
                return [SelectArgument(arg)];
            }
            else if (exp is Application app
                && app.Operator is Var op)
            {
                IEnumerable<IInstruction> loadArgs = app.Arguments.SelectMany(SelectExpression);

                return loadArgs.Concat(op.Data.Name switch
                {
                    "+" when app.Adicity == 2 => [new Instruction(PseudoOp.Add)],
                    "-" when app.Adicity == 1 => [new Instruction(PseudoOp.Neg)],
                    "-" when app.Adicity == 2 => [new Instruction(PseudoOp.Sub)],
                    "read" => ConstructRead(),
                    _ => throw new Exception($"Can't select instructions for application: {app}")
                });
            }

            throw new Exception($"Can't select instructions for expression: {exp}");
        }

        private static IEnumerable<IInstruction> ConstructRead()
        {
            return [
                new Instruction(PseudoOp.Call, new Label("string [System.Console]System.Console::ReadLine()")),
                new Instruction(PseudoOp.Call, new Label("int32 [System.Runtime]System.Int32::Parse(string)")),
                ];
        }
    }
}

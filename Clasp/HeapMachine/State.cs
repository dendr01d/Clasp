namespace Clasp.HeapMachine
{
    /// <summary>
    /// A machine that can be driven by external commands. The idea is that the machine can be driven by
    /// an evaluator or compiled code.
    /// </summary>
    internal class State
    {
        public Expression Accumulator { get; private set; }
        public Instruction Next { get; private set; }
        public Environment Env { get; private set; }
        public List<Expression> ValueRib { get; private set; }
        public Stack<State> CallStack { get; private set; }

        public State()
        {

        }

        private State(State original)
        {
            Accumulator = original.Accumulator;
            Next = original.Next;
            Env = original.Env;
            ValueRib = original.ValueRib.ToList();
            CallStack = new Stack<State>(original.CallStack.Reverse());
        }

        private State Export() => new State(this);
        private void Restore(State s)
        {
            Accumulator = s.Accumulator;
            Next = s.Next;
            Env = s.Env;
            ValueRib = s.ValueRib.ToList();
            CallStack = new Stack<State>(s.CallStack.Reverse());
        }

        private void ExecuteNext()
        {
            Instruction instr = Next;

            try
            {
                switch (instr)
                {
                    case Halt:
                        break;

                    case PushConst pushConst:
                        Accumulator = pushConst.ConstantValue;
                        Next = pushConst.Next;
                        break;

                    case LexGet lexGet:
                        Accumulator = Env.LookUp(lexGet.VarName);
                        Next = lexGet.Next;
                        break;

                    case LexSet lexSet:
                        Env.Bind(lexSet.VarName, Accumulator);
                        Next = lexSet.Next;
                        break;

                    case Branch branch:
                        Next = Accumulator.IsTrue ? branch.Then : branch.Else;
                        break;

                    case Conti conti:
                        Accumulator = new Continuation(Export());
                        Next = conti.Next;
                        break;

                    case Nuate nuate:
                        Restore(nuate.Continuation.Expect<Box<State>>().BoxedValue);
                        Next = new Return(nuate.DebugInfo);
                        break;

                    case CallFrame callFrame:
                        State newFrame = new State(this)
                        {
                            Accumulator = Expression.Nil,
                            Next = callFrame.Return
                        };
                        CallStack.Push(newFrame);
                        Next = callFrame.Next;
                        break;

                    case AccArg accArg:
                        ValueRib.Add(Accumulator);
                        Next = accArg.Next;
                        break;

                    case Apply apply:
                        {
                            CompoundProcedure proc = Accumulator.Expect<CompoundProcedure>();
                            proc.EnvClosure.BindArgs(proc.Parameters, ValueRib);
                            Env = proc.EnvClosure;
                            ValueRib.Clear();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Accumulator = new Error(ex);
                Next = new Halt(ex.Message);
            }
        }

        internal class Continuation : Box<State>
        {
            public Continuation(State s) : base(s) { }
        }
    }

    //internal class LexicalScope
    //{
    //    private LexicalScope? _staticChainLink;
    //    private readonly Expression[] _rib;

    //    public LexicalScope(params Expression[] values)
    //    {
    //        _staticChainLink = null;
    //        _rib = values;
    //    }

    //    public LexicalScope(LexicalScope chainLink, params Expression[] values) : this(values)
    //    {
    //        _staticChainLink = chainLink;
    //    }

    //    private LexicalScope GetChainLinkByIndex(int i)
    //    {
    //        LexicalScope target = this;

    //        while (i > 0)
    //        {
    //            if (target._staticChainLink is null)
    //            {
    //                throw new Exception("Tried to index past beginning of static chain");
    //            }
    //            else
    //            {
    //                target = target._staticChainLink;
    //                --i;
    //            }
    //        }

    //        return target;
    //    }

    //    private Expression GetRibValue(int i) => _rib[i];
    //    private void SetRibValue(int i, Expression value) => _rib[i] = value;

    //    public Expression Lookup(int chainIndex, int ribIndex)
    //        => GetChainLinkByIndex(chainIndex).GetRibValue(ribIndex);
    //    public void SetValue(int chainIndex, int ribIndex, Expression value)
    //        => GetChainLinkByIndex(chainIndex).SetRibValue(ribIndex, value);
    //}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    internal class Machine
    {
        private Machine()
        {
            _expStack = new Stack<Expression>();
            _envStack = new Stack<Environment>();
            _ptrStack = new Stack<Action<Machine>?>();

            _exp = null;
            _val = null;
            _unev = null;
            _argl = null;
            _proc = null;

            _env = GlobalEnvironment.Empty();

            _goto = null;
            _continue = null;
        }

        public Machine(Expression exp, Environment env, Action<Machine> ptr) : this()
        {
            Assign_Exp(exp);
            ReplaceScope(env);
            Assign_GoTo(ptr);
        }


        private Stack<Expression> _expStack;
        private Stack<Environment> _envStack;
        private Stack<Action<Machine>?> _ptrStack;

        #region Registers

        private Expression? _exp;
        private Expression? _val;
        private Expression? _unev;
        private Expression? _argl;
        private Procedure? _proc;

        private Environment _env;

        private Action<Machine>? _goto;
        private Action<Machine>? _continue;

        #endregion

        #region Register Access

        public Expression Exp => _exp ?? throw new UninitializedRegisterException("Exp");
        public Expression Val => _val ?? throw new UninitializedRegisterException("Val");
        public Expression Unev => _unev ?? throw new UninitializedRegisterException("Unev");
        public Expression Argl => _argl ?? throw new UninitializedRegisterException("Argl");
        public Procedure Proc => _proc ?? throw new UninitializedRegisterException("Proc");

        public Environment Env => _env;

        public Action<Machine>? GoTo => _goto;
        public Action<Machine>? Continue => _continue;

        #endregion

        #region Register Assignment

        public void Assign_Exp(Expression x) => _exp = x;
        public void Assign_Val(Expression x) => _val = x;
        public void Assign_Unev(Expression x) => _unev = x;
        public void Assign_Argl(Expression x) => _argl = x;
        public void Assign_Proc(Procedure p) => _proc = p;

        public void Assign_GoTo(Action<Machine>? ptr) => _goto = ptr;
        public void Assign_Continue(Action<Machine> ptr) => _continue = ptr;

        #endregion

        #region Stack Modification

        private void SaveTerm(Expression x) => _expStack.Push(x);
        private void RestoreTerm(ref Expression? x)
        {
            if (_expStack.TryPop(out Expression? m) && m is not null)
            {
                x = m;
            }
            else
            {
                throw new StackUnderflowException<Expression>(_expStack);
            }
        }

        public void Save_Exp() => SaveTerm(Exp);
        public void Restore_Exp() => RestoreTerm(ref _exp);

        public void Save_Val() => SaveTerm(Val);
        public void Restore_Val() => RestoreTerm(ref _val);

        public void Save_Unev() => SaveTerm(Unev);
        public void Restore_Unev() => RestoreTerm(ref _unev);

        public void Save_Argl() => SaveTerm(Argl);
        public void Restore_Argl() => RestoreTerm(ref _argl);

        public void Save_Proc() => SaveTerm(Proc);
        public void Restore_Proc() => _proc = _expStack.TryPop(out Expression? m) && m is not null && m is Procedure p
            ? p
            : throw new StackUnderflowException<Expression>(_expStack);

        public void Save_GoTo() => _ptrStack.Push(GoTo);
        public void Restore_GoTo() => _goto = _ptrStack.TryPop(out var ptr)
            ? ptr
            : throw new StackUnderflowException<Action<Machine>?>(_ptrStack);

        public void Save_Continue() => _ptrStack.Push(Continue);
        public void Restore_Continue() => _continue = _ptrStack.TryPop(out var ptr)
            ? ptr
            : throw new StackUnderflowException<Action<Machine>?>(_ptrStack);

        #endregion

        #region Environment Modification

        public void EnterNewScope() => _envStack.Push(_env);

        public void LeaveScope()
        {
            if (_envStack.TryPop(out Environment? e) && e is not null)
            {
                _env = e;
            }
            else
            {
                throw new StackUnderflowException<Environment>(_envStack);
            }
        }

        public void ReplaceScope(Environment newEnv)
        {
            _env = newEnv;
        }

        #endregion

        #region Printing

        private const string STACK_SEP = "≤";
        private const string EMPTY_PTR = "ε";

        private string FormatRegister(Expression? x) => x?.ToString() ?? EMPTY_PTR;
        private string FormatFunctor(Action<Machine>? ptr) => ptr?.Method.Name ?? EMPTY_PTR;

        public string GoingTo => FormatFunctor(_goto);
        public string ContinueTo => FormatFunctor(_continue);

        public void Print(TextWriter tw)
        {
            tw.WriteLine($" Exp: {FormatRegister(_exp)}");
            tw.WriteLine($" Val: {FormatRegister(_val)}");
            tw.WriteLine($"Proc: {FormatRegister(_proc)}");
            tw.WriteLine($"Unev: {FormatRegister(_unev)}");
            tw.WriteLine($"Argl: {FormatRegister(_argl)}");
            if (_expStack.Any())
            {
                tw.WriteLine();
                tw.WriteLine("Term Stack:");
                foreach(Expression expr in _expStack)
                {
                    tw.WriteLine($"{STACK_SEP} {expr}");
                }
            }
            tw.WriteLine();
            tw.WriteLine($"Env w/ {_env.CountBindings()} defs");
            tw.WriteLine($"{_envStack.Count} stacked environments");
            tw.WriteLine();
            tw.WriteLine($"GoTo -> {FormatFunctor(_goto)}");
            tw.WriteLine($"Cont -> {FormatFunctor(_continue)}");
            if (_ptrStack.Any())
            {
                tw.WriteLine();
                tw.WriteLine("Ptr Stack:");
                foreach (Action<Machine>? ptr in _ptrStack)
                {
                    tw.WriteLine($"{STACK_SEP} {FormatFunctor(ptr)}");
                }
            }
        }

        #endregion

        #region Helpers

        public void GoTo_Continue() => Assign_GoTo(Continue);

        public void AppendArgl(Expression newArg)
        {
            Assign_Argl(Pair.Append(Argl, Pair.MakeList(newArg)));
        }

        public void NextArgl() => Assign_Argl(Argl.Cdr);
        public void NextUnev() => Assign_Unev(Unev.Cdr);

        #endregion


    }
}

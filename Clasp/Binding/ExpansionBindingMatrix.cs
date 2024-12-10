using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.AbstractSyntax;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Binding
{
    internal class ExpansionBindingMatrix
    {
        private readonly Environment _env;
        private readonly BindingStore _store;
        private readonly int _phase;

        private ScopeSet _currentScope => _env.CurrentScope;

        public ExpansionBindingMatrix(Environment baseEnv)
        {
            _env = baseEnv.ExtractCompileTimeEnv();
            _store = new BindingStore();
            _phase = 0;
        }

        private ExpansionBindingMatrix(ExpansionBindingMatrix ancestor, int phaseLevel)
        {
            _env = new Environment(ancestor._env);
            _store = ancestor._store;
        }

        // -------------------------------

        public ExpansionBindingMatrix ExtendScope() => new ExpansionBindingMatrix(this, _phase);

        public ExpansionBindingMatrix ExtendPhase() => new ExpansionBindingMatrix(this, _phase + 1);

        private PhasedLexicalInfo GetModifiedLexicalInfo(Syntax original)
        {
            PhasedLexicalInfo newLexInfo = new PhasedLexicalInfo(original.Context);
            newLexInfo[_phase] = _currentScope;
            return newLexInfo;
        }

        public Syntax RebuildSyntax(Syntax original, Term newWrappedValue)
        {
            PhasedLexicalInfo newLexInfo = new PhasedLexicalInfo(original.Context);
            newLexInfo[_phase] = _currentScope;

            return Syntax.Wrap(newWrappedValue, newLexInfo, original.Source);
        }

        public Identifier RebuildIdentifier(Identifier original, Symbol newWrappedValue)
        {
            return new Identifier(newWrappedValue, GetModifiedLexicalInfo(original), original.Source);
        }

        public SyntaxProduct RebuildSyntaxPair(Syntax original, Syntax car, Syntax cdr)
        {
            return new SyntaxProduct(
                ConsList.Cons(car, cdr),
                GetModifiedLexicalInfo(original),
                original.Source);
        }

        public void RebindAsFresh(Identifier id)
        {
            string newName = GenerateFreshName(id.Name);
            _store.BindName(id.Name, _currentScope, newName);
        }

        private string GenerateFreshName(string initialName)
        {
            ICollection<string> visibleNames = _env.Keys;

            int counter = 1;
            string newName = initialName;

            while (visibleNames.Contains(newName))
            {
                newName = string.Format("{0}_{1}", initialName, counter);
                counter++;
            }

            return newName;
        }

        public string ResolveBindingName(Identifier id)
        {
            if (id.Context.TryGetValue(_phase, out ScopeSet? ss))
            {
                return _store.ResolveBindingName(id.Name, ss);
            }
            return id.Name;
        }

        public Identifier ResolveIdentifier(Identifier id)
        {
            string bindingName = ResolveBindingName(id);
            Symbol newSym = Symbol.Intern(bindingName);
            return new Identifier(newSym, GetModifiedLexicalInfo(id), id.Source);
        }

        public Term? ResolveBoundForm(Identifier id)
        {
            if (_env.TryGetValue(ResolveBindingName(id), out AstNode? result)
                && result is Fixed f
                && f.Value is Term t)
            {
                return t;
            }
            return null;
        }
    }
}

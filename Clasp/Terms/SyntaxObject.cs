using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Terms
{
    public enum BindingContext
    {
        Lexical, PatternVar, Transformer,
        Special_Lambda, Special_Quote,
        Special_Define, Special_DefineSyntax
    }

    internal class SyntaxObject : Atom
    {
        public readonly Expression Body;
        public readonly HashSet<int> Marks;
        private readonly List<Tuple<Expression, Expression>> _subs;
        public readonly Environment Scope;

        #region Constructors
        private SyntaxObject(Expression body, Environment scope)
        {
            Body = body;
            Scope = scope;
            Marks = new HashSet<int>();
            _subs = new List<Tuple<Expression, Expression>>();
        }

        public static SyntaxObject Wrap(Expression body, Environment scope)
        {
            return new SyntaxObject(body, scope);
        }

        public static SyntaxObject FromDatum(Expression body, SyntaxObject mimic)
        {
            SyntaxObject output = new SyntaxObject(body, mimic.Scope);
            output.Substitute
        }
        #endregion

        public override void Mark(params int[] newMarks) => Marks.SymmetricExceptWith(newMarks);

        public void Substitute(Symbol id, Symbol sub) => _subs.Add(new(id, sub));
        public void Substitute(SyntaxObject id, Symbol sub)
        {
            if (id.Body is not Symbol) throw new UncategorizedException("Can't substitute non-identifier in syntax object: " + id)
            id.Body is Symbol
            ? _subs.Add(new(id, sub))
            : throw new Exception();
        }
    }

    internal class MarkHelper
    {
        public static readonly int TopMark = 0;
        private int _freshMark;

        public MarkHelper()
        {
            _freshMark = TopMark + 1;
        }

        public int FreshMark() => _freshMark++;
    }
}

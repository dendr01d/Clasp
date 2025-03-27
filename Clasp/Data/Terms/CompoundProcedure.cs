using System;

using Clasp.Binding.Environments;
using Clasp.Data.AbstractSyntax;

namespace Clasp.Data.Terms
{
    //internal readonly struct CompoundProcedure : ITerm, IEquatable<CompoundProcedure>
    //{
    //    //instead of a full environment, capture references to the individual parts
    //    //wait, won't that include everything that isn't literal data?
    //    private readonly Box _closure; //box pointing to vector

    //    public readonly Box Body; //box pointing to vector

    //    public readonly FixNum Arity;
    //    public readonly Boole Variadic;

    //    public CompoundProcedure(FixNum arity, Boole variadic, Vector closure, Vector body)
    //    {
    //        Arity = arity;
    //        Variadic = variadic;

    //        _closure = new Box(closure);
    //        Body = new Box(body);
    //    }

    //    public bool Equals(CompoundProcedure other)
    //    {
    //        return Arity.Equals(other.Arity)
    //            && Variadic.Equals(other.Variadic)
    //            && Body.Equals(other.Body)
    //            && _closure.Equals(other._closure);
    //    }
    //    public bool Equals(ITerm? other) => other is CompoundProcedure cp && Equals(cp);
    //    public override bool Equals(object? other) => other is CompoundProcedure cp && Equals(cp);
    //    public override int GetHashCode() => HashCode.Combine(_closure, Body, Arity, Variadic);
    //    public override string ToString() => string.Format("ƒ({0}{1})",
    //        Arity.Value > 0 ? Arity.Value : string.Empty,
    //        Variadic.Value ? "+" : string.Empty);
    //}

    internal class CompoundProcedure : ITerm
    {
        public readonly Symbol[] FormalParameters;
        public readonly Symbol? VariadicParameter;
        public readonly MutableEnv CapturedEnv;
        public readonly Sequential Body;

        public int Arity => FormalParameters.Length;
        public bool IsVariadic => VariadicParameter is not null;

        public CompoundProcedure(Symbol[] parameters, Symbol? finalParameter, MutableEnv enclosing, Sequential body)
        {
            FormalParameters = parameters;
            VariadicParameter = finalParameter;

            CapturedEnv = enclosing;
            Body = body;
        }

        public Closure GetClosure() => CapturedEnv.Enclose();

        public bool Equals(CompoundProcedure other)
        {
            return ReferenceEquals(this, other);
        }
        public bool Equals(ITerm? other) => other is CompoundProcedure cp && Equals(cp);
        public override bool Equals(object? other) => other is CompoundProcedure cp && Equals(cp);
        public override int GetHashCode() => HashCode.Combine(FormalParameters, VariadicParameter, CapturedEnv, Body);
        public override string ToString() => string.Format("ƒ({0}{1})",
            Arity > 0 ? Arity : string.Empty,
            IsVariadic ? "+" : string.Empty);
    }
}

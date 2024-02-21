using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp
{
    public class LexingException : Exception
    {
        public LexingException(string msg) : base($"Lexing error: {msg}") { }
    }

    public class ParsingException : Exception
    {
        public ParsingException(string msg) : base($"Parsing error: {msg}") { }
    }

    public class ArgumentArityException : Exception
    {
        public ArgumentArityException(string procName, int expectedArity, int providedArity)
            : base($"Procedure '{procName}' expects {expectedArity} arguments but was given {providedArity}") { }
    }

    public class EnvironmentLookupException : Exception
    {
        public EnvironmentLookupException(string key) : base($"Tried to reference un-bound symbol {key}") { }
    }

    public class EnvironmentBindingException : Exception
    {
        public EnvironmentBindingException(string key, string extantDef)
            : base($"Tried to re-bind symbol {key}, already bound to {extantDef}") { }
    }

    public class ExpectedTypeException : Exception
    {
        public ExpectedTypeException(string expectedType, string providedObject)
            : base($"Expected expression of type {expectedType}: {providedObject}") { }
    }

    public class SpecialFormKeywordNotFoundException : Exception
    {
        public SpecialFormKeywordNotFoundException(string keyword)
            : base($"Tried to construct special form with non-existent keyword '{keyword}'") { }
    }

    public class ControlFalloutException : Exception
    {
        public ControlFalloutException(string formName)
            : base($"Fell out of control structure in '{formName}' evaluation") { }
    }
}

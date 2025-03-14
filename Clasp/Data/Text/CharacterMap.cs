using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Text
{
    internal static class CharacterMap
    {


        public static readonly Dictionary<string, char> NameToChar = new()
        {
            { "space",   ' ' },
            { "tab",     '\t' },
            { "newline", '\n' },
            { "return",  '\r' },

            { "Alpha",   'Α' }, { "alpha",   'α' },
            { "Beta",    'Β' }, { "beta",    'β' },
            { "Gamma",   'Γ' }, { "gamma",   'γ' },
            { "Delta",   'Δ' }, { "delta",   'δ' },
            { "Epsilon", 'Ε' }, { "epsilon", 'ε' },
            { "Zeta",    'Ζ' }, { "zeta",    'ζ' },
            { "Eta",     'Η' }, { "eta",     'η' },
            { "Theta",   'Θ' }, { "theta",   'θ' },
            { "Iota",    'Ι' }, { "iota",    'ι' },
            { "Kappa",   'Κ' }, { "kappa",   'κ' },
            { "Lambda",  'Λ' }, { "lambda",  'λ' },
            { "Mu",      'Μ' }, { "mu",      'μ' },
            { "Nu",      'Ν' }, { "nu",      'ν' },
            { "Xi",      'Ξ' }, { "xi",      'ξ' },
            { "Omicron", 'Ο' }, { "omicron", 'ο' },
            { "Pi",      'Π' }, { "pi",      'π' },
            { "Rho",     'Ρ' }, { "rho",     'ρ' },
            { "Sigma",   'Σ' }, { "sigma",   'σ' },
            { "Tau",     'Τ' }, { "tau",     'τ' },
            { "Upsilon", 'Υ' }, { "upsilon", 'υ' },
            { "Phi",     'Φ' }, { "phi",     'φ' },
            { "Chi",     'Χ' }, { "chi",     'χ' },
            { "Psi",     'Ψ' }, { "psi",     'ψ' },
            { "Omega",   'Ω' }, { "omega",   'ω' },

            { "<<", '«' }, { ">>", '»'},
            { "<=", '≤' }, { ">=", '≥'},
            { "==", '≡' }, { "!=", '≠'},
            { "~=", '≈' },
            { "+-", '±' },

            { "div", '÷' }, { "mult", '×'},

            { "join", '∪' }, { "disjoin", '∩' },

            { "mem", 'ϵ' }, { "nmem", 'Ɇ' },

            { "subset", '⊆' }, { "psubset", '⊂' },
            { "supset", '⊇' }, { "psupset", '⊃' },

            { "forall", 'Ɐ' }, { "exists", 'Ǝ' },

            { "and", '˄' }, { "or", '˅' }, { "not", '¬' },

            { "<-", '←' }, { "->", '→' }, { "<->", '↔' },

            { "true", '┬' }, { "false", '┴' },

            { "empty", 'Ø' },
            { "degree", '°' },
            { "dot", '∙' },
            { "function", 'ƒ' },
            { "infinity", '∞' },
            { "prop", '∝' },

            { "floorL", '⌊' }, { "floorR", '⌋' },
            { "ceilL", '⌈' }, { "ceilR", '⌉' },
            { "<[", '‹' }, { "]>", '›' },

            { "\"[", '“' }, { "]\"", '”'},
            { "\'[", '‘' }, { "]\'", '’'}
        };

        public static readonly Dictionary<char, string> CharToName =
            NameToChar.ToDictionary(x => x.Value, x => x.Key);

    }
}

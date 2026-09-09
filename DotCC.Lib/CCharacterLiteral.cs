using System;
using DotCC.Ir;

namespace DotCC;

internal static class CCharacterLiteral
{
    /// <summary>Decode the body of a C character constant (the chars between the
    /// quotes) to its integer value: a single char, a named escape, a
    /// <c>\xHH</c> hex escape, or a <c>\NNN</c> octal escape.</summary>
    internal static int Decode(string inner)
    {
        if (inner.Length == 0) { return 0; }
        if (inner[0] != '\\') { return inner[0]; }
        var esc = inner[1];
        switch (esc)
        {
            case 'n': return 10;
            case 't': return 9;
            case 'r': return 13;
            case 'a': return 7;
            case 'b': return 8;
            case 'f': return 12;
            case 'v': return 11;
            case '0' when inner.Length == 2: return 0;
            case '\\': return 92;
            case '\'': return 39;
            case '"': return 34;
            case '?': return 63;
            case 'x':
                return Convert.ToInt32(inner[2..], 16);
            case >= '0' and <= '7':
                return Convert.ToInt32(inner[1..], 8);
            default:
                throw new IrUnsupportedException("char literal '" + inner + "'");
        }
    }
}

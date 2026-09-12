using System;
using DotCC.Ir;

namespace DotCC;

internal static class CCharacterLiteral
{
    /// <summary>Decode characters and C escapes. Ordinary multicharacter
    /// constants use GCC-compatible ASCII/byte packing: shift left eight bits
    /// per character, retaining the low 32 bits as an int. Single-character
    /// constants retain their existing numeric value.</summary>
    internal static int Decode(string inner)
    {
        if (inner.Length == 0) { return 0; }
        int index = 0, count = 0, value = 0;
        bool byteOnly = true;
        while (index < inner.Length)
        {
            int character;
            bool escaped = inner[index] == '\\';
            if (!escaped) character = inner[index++];
            else
            {
                ++index;
                if (index == inner.Length) throw Unsupported(inner);
                var escape = inner[index++];
                switch (escape)
                {
                    case 'n': character = 10; break;
                    case 't': character = 9; break;
                    case 'r': character = 13; break;
                    case 'a': character = 7; break;
                    case 'b': character = 8; break;
                    case 'e': character = 27; break;
                    case 'f': character = 12; break;
                    case 'v': character = 11; break;
                    case '\\': character = 92; break;
                    case '\'': character = 39; break;
                    case '"': character = 34; break;
                    case '?': character = 63; break;
                    case 'x':
                        int start = index;
                        while (index < inner.Length && Uri.IsHexDigit(inner[index])) ++index;
                        if (start == index) throw Unsupported(inner);
                        character = Convert.ToInt32(inner[start..index], 16);
                        break;
                    case >= '0' and <= '7':
                        character = escape - '0';
                        for (int digits = 1; digits < 3 && index < inner.Length
                            && inner[index] is >= '0' and <= '7'; ++digits)
                            character = character * 8 + inner[index++] - '0';
                        break;
                    default: throw Unsupported(inner);
                }
            }
            // Do not silently interpret a multibyte source character as a byte.
            byteOnly &= character >= 0 && character <= (escaped ? 255 : 127);
            if (++count > 1 && !byteOnly) throw Unsupported(inner);
            value = unchecked((value << 8) | character);
        }
        return value;
    }

    private static IrUnsupportedException Unsupported(string inner) =>
        new("char literal '" + inner + "'");
}

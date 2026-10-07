#nullable enable
using System;

namespace Edg.Runtime;

/// <summary>Strict POSIX presentation-to-network conversion, independent of the
/// host socket ABI. Parsing uses stack spans; no DNS, OS call, or managed heap
/// allocation is involved. Unlike IPAddress.TryParse, IPv4 legacy abbreviated,
/// octal and hexadecimal spellings are not accepted.</summary>
public static unsafe class LibcInet
{
    public const int AfInet = 2, AfInet6 = 10, Eafnosupport = 97;

    public static int InetPton(int family, sbyte* source, void* destination)
    {
        if (family != AfInet && family != AfInet6) return -1;
        // The longest accepted spelling is 45 ASCII bytes. A bound also avoids
        // scanning an arbitrarily long rejected presentation string.
        int length = 0;
        while (length < 46 && source[length] != 0) ++length;
        if (length == 46) return 0;
        ReadOnlySpan<byte> text = new(source, length);
        Span<byte> address = stackalloc byte[16];
        bool valid = family == AfInet ? ParseV4(text, address) : ParseV6(text, address);
        if (!valid) return 0;
        int count = family == AfInet ? 4 : 16;
        address[..count].CopyTo(new Span<byte>(destination, count));
        return 1;
    }

    private static bool ParseV4(ReadOnlySpan<byte> text, Span<byte> address)
    {
        int cursor = 0;
        for (int component = 0; component != 4; ++component)
        {
            int start = cursor, value = 0;
            while (cursor < text.Length && text[cursor] >= '0' && text[cursor] <= '9')
            {
                value = value * 10 + text[cursor++] - '0';
                if (value > 255 || cursor - start > 3) return false;
            }
            if (cursor == start || (cursor - start > 1 && text[start] == '0')) return false;
            address[component] = (byte)value;
            if (component != 3 && (cursor == text.Length || text[cursor++] != '.')) return false;
        }
        return cursor == text.Length;
    }

    private static bool ParseV6(ReadOnlySpan<byte> text, Span<byte> address)
    {
        if (text.IsEmpty) return false;
        int cursor = 0, count = 0, compressed = -1;
        if (text[0] == ':')
        {
            if (text.Length < 2 || text[1] != ':') return false;
            cursor = 2; compressed = 0;
        }
        while (cursor < text.Length)
        {
            if (count == 16) return false;
            int start = cursor, value = 0, digits = 0;
            while (cursor < text.Length)
            {
                int digit = Hex(text[cursor]);
                if (digit < 0) break;
                if (++digits > 4) return false;
                value = (value << 4) | digit;
                ++cursor;
            }
            if (cursor < text.Length && text[cursor] == '.')
            {
                if (count > 12 || !ParseV4(text[start..], address[count..])) return false;
                count += 4; cursor = text.Length; break;
            }
            if (digits == 0) return false;
            address[count++] = (byte)(value >> 8);
            address[count++] = (byte)value;
            if (cursor == text.Length) break;
            if (text[cursor++] != ':') return false;
            if (cursor < text.Length && text[cursor] == ':')
            {
                if (compressed >= 0) return false;
                compressed = count; ++cursor;
            }
            else if (cursor == text.Length) return false;
        }
        if (compressed < 0) return count == 16;
        int missing = 16 - count;
        if (missing == 0) return false; // :: must abbreviate at least one group.
        address.Slice(compressed, count - compressed).CopyTo(address[(compressed + missing)..]);
        address.Slice(compressed, missing).Clear();
        return true;
    }

    private static int Hex(byte value) => value >= '0' && value <= '9' ? value - '0' :
        value >= 'a' && value <= 'f' ? value - 'a' + 10 :
        value >= 'A' && value <= 'F' ? value - 'A' + 10 : -1;
}

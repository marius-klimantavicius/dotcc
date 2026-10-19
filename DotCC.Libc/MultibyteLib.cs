#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    private static bool Utf8MultibyteLocale => Volatile.Read(ref RuntimeState.Utf8MultibyteLocale);
    // Immutable, process-lifetime borrowed strings, independent of worker TLS.
    private static readonly nint utf8LocaleStorage = Marshal.StringToCoTaskMemUTF8("C.UTF-8");
    private static readonly nint utf8CodesetStorage = Marshal.StringToCoTaskMemUTF8("UTF-8");
    private static readonly nint asciiCodesetStorage = Marshal.StringToCoTaskMemUTF8("ANSI_X3.4-1968");
    private static byte* Utf8LocaleName => (byte*)utf8LocaleStorage;
    private static readonly Lazy<global::System.Collections.Generic.Dictionary<int, nint>> cLanginfo = new(CreateCLanginfo);

    private static global::System.Collections.Generic.Dictionary<int, nint> CreateCLanginfo()
    {
        // Immutable borrowed C-locale strings, like localeconv's process-lifetime
        // conventions. Numeric IDs are the explicit Linux nl_item ABI in the
        // header, checked against the native oracle with all bytes retained.
        var result = new global::System.Collections.Generic.Dictionary<int, nint>();
        void Add(int item, string value) => result.Add(item, Marshal.StringToCoTaskMemUTF8(value));
        for (int i = 0; i < 7; i++) { Add(131072 + i, _wdayAbbr[i]); Add(131079 + i, _wdayFull[i]); }
        for (int i = 0; i < 12; i++) { Add(131086 + i, _monAbbr[i]); Add(131098 + i, _monFull[i]); }
        Add(131110, "AM"); Add(131111, "PM");
        Add(131112, "%a %b %e %H:%M:%S %Y"); Add(131113, "%m/%d/%y");
        Add(131114, "%H:%M:%S"); Add(131115, "%I:%M:%S %p");
        foreach (int item in new[] { 131116, 131118, 131119, 131120, 131121, 65537 }) Add(item, "");
        Add(262159, "-"); // C CRNCYSTR: symbol precedes the value; symbol itself is empty.
        Add(65536, "."); Add(327680, "^[yY]"); Add(327681, "^[nN]");
        return result;
    }

    public static byte* nl_langinfo(int item)
    {
        if (item == 14) return (byte*)(Utf8MultibyteLocale ? utf8CodesetStorage : asciiCodesetStorage);
        if (cLanginfo.Value.TryGetValue(item, out nint value)) return (byte*)value;
        errno = ENOTSUP;
        return LiteralPool.Pointer + LiteralPool.Empty;
    }

    /// <summary>Stateless locale conversion into unsigned UTF-16 wchar units.
    /// Does not call the host's 32-bit-wchar functions. A surrogate pair is
    /// written atomically; a short destination never gets a lone high half.</summary>
    public static ulong mbstowcs(char* destination, byte* source, ulong length)
    {
        if (source == null) { errno = EFAULT; return ulong.MaxValue; }
        bool utf8 = Utf8MultibyteLocale;
        ulong written = 0;
        while (destination == null || written < length)
        {
            if (*source == 0)
            {
                if (destination != null) destination[written] = '\0';
                return written;
            }
            int scalar = *source, consumed = 1;
            if (scalar >= 128 && (!utf8 || !DecodeMultibyteScalar(source, out scalar, out consumed)))
            { errno = EILSEQ; return ulong.MaxValue; }
            int units = scalar > 0xffff ? 2 : 1;
            if (destination != null && (ulong)units > length - written) return written;
            if (destination != null)
            {
                if (units == 1) destination[written] = (char)scalar;
                else
                {
                    int value = scalar - 0x10000;
                    destination[written] = (char)(0xd800 + (value >> 10));
                    destination[written + 1] = (char)(0xdc00 + (value & 1023));
                }
            }
            written += (ulong)units;
            source += consumed;
        }
        return written;
    }

    private static bool DecodeMultibyteScalar(byte* source, out int scalar, out int consumed)
    {
        int lead = *source;
        consumed = lead is >= 0xc2 and <= 0xdf ? 2 : lead is >= 0xe0 and <= 0xef ? 3 : lead is >= 0xf0 and <= 0xf4 ? 4 : 0;
        scalar = consumed == 2 ? lead & 31 : consumed == 3 ? lead & 15 : lead & 7;
        if (consumed == 0) return false;
        for (int i = 1; i < consumed; i++)
        {
            int next = source[i];
            // NUL fails here, before any later byte can be read.
            if (next < 0x80 || next > 0xbf) return false;
            scalar = (scalar << 6) | (next & 63);
        }
        return scalar <= 0x10ffff && !(scalar is >= 0xd800 and <= 0xdfff)
            && (consumed != 3 || scalar >= 0x800) && (consumed != 4 || scalar >= 0x10000);
    }

    /// <summary>Stateless UTF-16-to-ASCII/UTF-8 conversion. Length bounds bytes,
    /// excluding the optional NUL; malformed surrogate sequences fail without
    /// replacement. Measurement ignores length and includes no terminating NUL.</summary>
    public static ulong wcstombs(byte* destination, char* source, ulong length)
    {
        if (source == null) { errno = EFAULT; return ulong.MaxValue; }
        bool utf8 = Utf8MultibyteLocale;
        ulong written = 0;
        while (destination == null || written < length)
        {
            int scalar = *source;
            if (scalar == 0)
            {
                if (destination != null) destination[written] = 0;
                return written;
            }
            int units = 1;
            if (scalar is >= 0xd800 and <= 0xdbff)
            {
                int low = source[1];
                if (low < 0xdc00 || low > 0xdfff) { errno = EILSEQ; return ulong.MaxValue; }
                scalar = 0x10000 + ((scalar - 0xd800) << 10) + low - 0xdc00;
                units = 2;
            }
            else if (scalar is >= 0xdc00 and <= 0xdfff) { errno = EILSEQ; return ulong.MaxValue; }
            if (!utf8 && scalar >= 128) { errno = EILSEQ; return ulong.MaxValue; }
            int count = scalar < 128 ? 1 : scalar < 0x800 ? 2 : scalar < 0x10000 ? 3 : 4;
            if (destination != null)
            {
                if ((ulong)count > length - written) return written;
                if (count == 1) destination[written] = (byte)scalar;
                else
                {
                    destination[written] = (byte)((count == 2 ? 0xc0 : count == 3 ? 0xe0 : 0xf0) | (scalar >> (6 * (count - 1))));
                    for (int i = 1; i < count; i++)
                        destination[written + (ulong)i] = (byte)(0x80 | ((scalar >> (6 * (count - 1 - i))) & 63));
                }
            }
            written += (ulong)count;
            source += units;
        }
        return written;
    }
}

#nullable enable

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace DotCC.Libc;

/// <summary>
/// Fluent <c>scanf</c> / <c>fscanf</c> / <c>sscanf</c> reader. Each
/// <see cref="Read(int*)"/> / <see cref="Read(double*)"/> /
/// <see cref="Read(byte*)"/> consumes the next <c>%</c> spec from the
/// format pointer and writes the parsed value to the out pointer.
/// <see cref="Done"/> returns the count of successful matches (real
/// scanf's return value).
/// </summary>
/// <remarks>
/// Supported conversions: <c>%d</c>/<c>%u</c> (decimal), <c>%i</c> (base-detecting integer),
/// <c>%x</c>/<c>%X</c> (hex int), <c>%o</c> (octal int), <c>%f</c>/<c>%e</c>/<c>%g</c>
/// (double), <c>%s</c> (whitespace-delimited token → NUL-terminated UTF-8),
/// <c>%c</c> (byte(s)). A <b>maximum field width</b> (<c>%3d</c>, <c>%8s</c>) is
/// honored — the conversion consumes at most that many input characters. Length
/// modifiers (<c>l</c>/<c>L</c>/<c>h</c>/<c>z</c>/<c>j</c>/<c>t</c>) are accepted
/// and ignored (the receiving <c>Read</c> overload already carries the type).
/// Parsing rules mirror C's: leading whitespace skipped before each conversion
/// (except <c>%c</c>); numeric parsing stops at the first non-conforming char;
/// <c>%s</c> stops at whitespace or EOF.
/// <para>
/// A conversion the routed overload can't satisfy — a genuinely unsupported spec
/// (<c>%n</c>, a <c>%[…]</c> scanset), or a format/argument-type mismatch (a
/// float spec against an <c>int*</c>) — <b>throws</b> <see cref="FormatException"/>
/// rather than silently skipping it. dotcc fails loudly, never silently wrong.
/// Assignment suppression is supported for integer and string/character conversions.
/// Literal separators are matched before the following conversion; a failed match
/// leaves that and subsequent destinations unchanged.
/// </para>
/// </remarks>
public unsafe ref struct ScanfReader
{
    private readonly TextReader _r;
    private byte* _fmt;
    private int _matched;
    private bool _failed;

    internal ScanfReader(TextReader r, byte* fmt)
    {
        _r = r;
        _fmt = fmt;
        _matched = 0;
        _failed = false;
    }

    public ScanfReader Read(int* dst) { if (ReadInteger(out ulong value)) *dst = unchecked((int)value); return this; }
    public ScanfReader Read(uint* dst) { if (ReadInteger(out ulong value)) *dst = unchecked((uint)value); return this; }
    public ScanfReader Read(long* dst) { if (ReadInteger(out ulong value)) *dst = unchecked((long)value); return this; }
    public ScanfReader Read(ulong* dst) { if (ReadInteger(out ulong value)) *dst = value; return this; }
    public ScanfReader Read(short* dst) { if (ReadInteger(out ulong value)) *dst = unchecked((short)value); return this; }
    public ScanfReader Read(ushort* dst) { if (ReadInteger(out ulong value)) *dst = unchecked((ushort)value); return this; }
    public ScanfReader Read(sbyte* dst) { if (ReadInteger(out ulong value)) *dst = unchecked((sbyte)value); return this; }
    public ScanfReader Read(float* dst)
    {
        double value = 0;
        int before = _matched;
        Read(&value);
        if (_matched != before) *dst = (float)value;
        return this;
    }

    private bool ReadInteger(out ulong value)
    {
        value = 0;
        if (_failed) return false;
        byte spec = ExpectSpec(out int width);
        if (_failed) return false;
        if (!ReadIntegerValue(spec, width, out value)) { _failed = true; return false; }
        _matched++;
        return true;
    }

    private bool ReadIntegerValue(byte spec, int width, out ulong value)
    {
        int radix = spec switch
        {
            (byte)'d' or (byte)'u' => 10,
            (byte)'i' => 0,
            (byte)'x' or (byte)'X' => 16,
            (byte)'o' => 8,
            _ => throw Unsupported(spec, "an integer (%d %i %u %x %X %o)"),
        };
        SkipInputWs();
        int remaining = width < 0 ? int.MaxValue : width;
        bool negative = false, any = false;
        value = 0;
        int peek = _r.Peek();
        if (remaining > 0 && (peek == '-' || peek == '+'))
        {
            negative = peek == '-'; _r.Read(); remaining--;
        }
        if (remaining > 0 && _r.Peek() == '0')
        {
            any = true; _r.Read(); remaining--;
            if (radix == 0) radix = 8;
            if (remaining > 0 && (radix == 8 && spec == 'i' || radix == 16) && (_r.Peek() == 'x' || _r.Peek() == 'X'))
            {
                _r.Read(); remaining--; radix = 16;
            }
        }
        if (radix == 0) radix = 10;
        while (remaining > 0 && (peek = _r.Peek()) != -1)
        {
            int digit = DigitValue(peek, radix);
            if (digit < 0) break;
            value = unchecked(value * (uint)radix + (uint)digit);
            any = true; _r.Read(); remaining--;
        }
        if (negative) value = unchecked(0UL - value);
        return any;
    }

    public ScanfReader Read(double* dst)
    {
        if (_failed) return this;
        var spec = ExpectSpec(out int width);
        if (_failed) return this;
        if (spec != (byte)'f' && spec != (byte)'e' && spec != (byte)'g'
            && spec != (byte)'F' && spec != (byte)'E' && spec != (byte)'G')
        {
            throw Unsupported(spec, "a double (%f %e %g)");
        }
        SkipInputWs();
        int consumed = 0;
        var sb = new StringBuilder();
        int peek = _r.Peek();
        if ((peek == '-' || peek == '+') && (width < 0 || consumed < width))
        {
            sb.Append((char)_r.Read());
            consumed++;
        }
        bool sawDigit = false;
        bool sawDot = false;
        bool sawExp = false;
        while ((width < 0 || consumed < width) && (peek = _r.Peek()) != -1)
        {
            if (peek >= '0' && peek <= '9') { sawDigit = true; sb.Append((char)_r.Read()); consumed++; }
            else if (peek == '.' && !sawDot && !sawExp) { sawDot = true; sb.Append((char)_r.Read()); consumed++; }
            else if ((peek == 'e' || peek == 'E') && sawDigit && !sawExp && (width < 0 || consumed < width))
            {
                sawExp = true;
                sb.Append((char)_r.Read());
                consumed++;
                int p2 = _r.Peek();
                if ((p2 == '-' || p2 == '+') && (width < 0 || consumed < width)) { sb.Append((char)_r.Read()); consumed++; }
            }
            else { break; }
        }
        if (sawDigit && double.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
        {
            *dst = v;
            _matched++;
        }
        else _failed = true;
        return this;
    }

    public ScanfReader Read(byte* dst)
    {
        if (_failed) return this;
        var spec = ExpectSpec(out int width);
        if (_failed) return this;
        if (spec != (byte)'s' && spec != (byte)'c')
        {
            if (ReadIntegerValue(spec, width, out ulong value)) { *dst = unchecked((byte)value); _matched++; }
            else _failed = true;
            return this;
        }
        // %c: default field width 1 (read exactly one byte); %s: unbounded unless capped.
        int max = width >= 0 ? width : (spec == (byte)'c' ? 1 : int.MaxValue);
        if (spec != (byte)'c') { SkipInputWs(); }
        // Hoisted once — CA2014 (no per-iteration stackalloc inside the loop).
        Span<char> chBuf = stackalloc char[1];
        Span<byte> b = stackalloc byte[4];
        int written = 0;
        int count = 0;
        while (count < max)
        {
            int peek = _r.Peek();
            if (peek == -1) { break; }
            if (spec == (byte)'s' && char.IsWhiteSpace((char)peek)) { break; }
            // ASCII fast path — caller's buffer is byte* (UTF-8). For
            // multi-byte chars, encode via Encoding.UTF8.
            int ch = _r.Read();
            count++;
            if (ch < 0x80)
            {
                dst[written++] = (byte)ch;
            }
            else
            {
                chBuf[0] = (char)ch;
                int n = Encoding.UTF8.GetBytes(chBuf, b);
                for (int i = 0; i < n; i++) { dst[written++] = b[i]; }
            }
        }
        if (spec != (byte)'c') { dst[written] = 0; }
        if (written > 0) { _matched++; } else _failed = true;
        return this;
    }

    /// <summary>Wide <c>%s</c> / <c>%c</c> target — a <c>wchar_t*</c> (= C#
    /// <c>char*</c>) for the <c>w*scanf</c> family. Mirrors <see cref="Read(byte*)"/>
    /// but stores UTF-16 code units straight from the reader (no UTF-8 encoding —
    /// the reader already yields UTF-16); <c>%s</c> stops at whitespace/EOF and
    /// NUL-terminates, <c>%c</c> reads a single unit (or <c>width</c> units). Narrow
    /// <c>scanf</c> never targets a <c>char*</c>, so this overload is wide-only.</summary>
    public ScanfReader Read(char* dst)
    {
        if (_failed) return this;
        var spec = ExpectSpec(out int width);
        if (_failed) return this;
        if (spec != (byte)'s' && spec != (byte)'c')
        {
            throw Unsupported(spec, "a wide string/char (%ls %lc)");
        }
        int max = width >= 0 ? width : (spec == (byte)'c' ? 1 : int.MaxValue);
        if (spec != (byte)'c') { SkipInputWs(); }
        int written = 0;
        while (written < max)
        {
            int peek = _r.Peek();
            if (peek == -1) { break; }
            if (spec == (byte)'s' && char.IsWhiteSpace((char)peek)) { break; }
            dst[written++] = (char)_r.Read();
        }
        if (spec != (byte)'c') { dst[written] = '\0'; }
        if (written > 0) { _matched++; } else _failed = true;
        return this;
    }

    public int Done() => _matched;

    /// <summary>Consume the next <c>%</c> spec from the format: skip leading fmt
    /// whitespace and matching input literals, suppressed conversions, the
    /// optional max field <paramref name="width"/>, and the length modifier;
    /// return the conversion letter (0 at end of format).</summary>
    private byte ExpectSpec(out int width)
    {
        width = -1;
        while (true)
        {
            while (*_fmt != 0)
            {
                if (IsAsciiWs(*_fmt)) { _fmt++; SkipInputWs(); continue; }
                if (*_fmt == '%' && _fmt[1] != '%') break;
                byte literal = *_fmt++;
                if (literal == '%') _fmt++;
                if (_r.Peek() != literal) { _failed = true; return 0; }
                _r.Read();
            }
            if (*_fmt != '%') return 0;
            _fmt++;
            bool suppress = *_fmt == '*';
            if (suppress) _fmt++;
            width = -1;
            while (*_fmt >= '0' && *_fmt <= '9')
            {
                if (width < 0) width = 0;
                width = width > (int.MaxValue - 9) / 10 ? int.MaxValue : width * 10 + (*_fmt - '0');
                _fmt++;
            }
            while (*_fmt is (byte)'l' or (byte)'L' or (byte)'h' or (byte)'z' or (byte)'j' or (byte)'t') _fmt++;
            byte spec = *_fmt;
            if (spec != 0) _fmt++;
            if (!suppress) return spec;
            if (spec == 's' || spec == 'c')
            {
                if (spec == 's') SkipInputWs();
                int remaining = width < 0 ? (spec == 'c' ? 1 : int.MaxValue) : width;
                bool any = false;
                while (remaining-- > 0 && _r.Peek() != -1 && (spec == 'c' || !char.IsWhiteSpace((char)_r.Peek())))
                { _r.Read(); any = true; }
                if (!any) { _failed = true; return 0; }
            }
            else if (!ReadIntegerValue(spec, width, out _)) { _failed = true; return 0; }
        }
    }

    private void SkipInputWs()
    {
        int c;
        while ((c = _r.Peek()) != -1 && char.IsWhiteSpace((char)c)) { _r.Read(); }
    }

    /// <summary>The value of ASCII digit <paramref name="c"/> in <paramref name="base"/>,
    /// or -1 if it is not a digit of that base.</summary>
    private static int DigitValue(int c, int @base)
    {
        int v = c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'a' and <= 'f' => c - 'a' + 10,
            >= 'A' and <= 'F' => c - 'A' + 10,
            _ => -1,
        };
        return v >= 0 && v < @base ? v : -1;
    }

    /// <summary>A loud, named failure for a conversion the routed overload can't
    /// satisfy — the dotcc "fail loudly, never silently wrong" contract for scanf.</summary>
    private static FormatException Unsupported(byte spec, string expected)
    {
        string s = spec == 0 ? "<end of format>" : "%" + (char)spec;
        return new FormatException(
            $"dotcc scanf: conversion '{s}' is not supported for {expected}. " +
            "Supported: %d %i %u %x %X %o (integer), %f %e %g (floating point), %s %c (string/char); " +
            "%n and %[...] scansets are not implemented.");
    }

    private static bool IsAsciiWs(byte b) => b is (byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n';
}

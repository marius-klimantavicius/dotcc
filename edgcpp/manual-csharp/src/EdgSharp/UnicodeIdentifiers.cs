// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten adaptation of lexical.c is_valid_UCN_identifier_char,
// scan_universal_character and make_canonical_identifier. Unicode data has
// its own retained Unicode-3.0 license; no host Unicode classification is used.
using System.Globalization;
using System.Text;

namespace EdgSharp;

internal static class UnicodeIdentifiers
{
    internal const string UnicodeVersion = "17.0.0";
    private readonly record struct Interval(int First, int Last);
    private sealed class IdentifierRanges
    {
        internal readonly Interval[] Start;
        internal readonly Interval[] Continue;
        internal IdentifierRanges()
        {
            using Stream stream = typeof(UnicodeIdentifiers).Assembly.GetManifestResourceStream("EdgSharp.Unicode17.DerivedCoreProperties.txt")
                ?? throw new InvalidOperationException("The pinned Unicode17 identifier resource is missing.");
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
            if (reader.ReadLine() != "# DerivedCoreProperties-17.0.0.txt")
                throw new InvalidOperationException("Unexpected identifier data version.");
            var start = new List<Interval>();
            var continuation = new List<Interval>();
            while (reader.ReadLine() is string line)
            {
                int comment = line.IndexOf('#');
                if (comment >= 0) line = line[..comment];
                string[] fields = line.Split(';', StringSplitOptions.TrimEntries);
                if (fields.Length != 2 || fields[1] is not ("XID_Start" or "XID_Continue")) continue;
                string[] limits = fields[0].Split("..", StringSplitOptions.None);
                int first = int.Parse(limits[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                int last = limits.Length == 1 ? first : int.Parse(limits[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                List<Interval> ranges = fields[1] == "XID_Start" ? start : continuation;
                if (limits.Length > 2 || !IsScalar(first) || !IsScalar(last) || first > last ||
                    ranges.Count != 0 && ranges[^1].Last >= first)
                    throw new InvalidOperationException("Malformed or unordered pinned identifier ranges.");
                ranges.Add(new(first, last));
            }
            if (start.Count == 0 || continuation.Count == 0) throw new InvalidOperationException("Empty pinned identifier ranges.");
            Start = start.ToArray(); Continue = continuation.ToArray();
        }
    }
    private static class Data { internal static readonly IdentifierRanges Ranges = new(); }
    internal static bool IsScalar(int scalar) => scalar is >= 0 and <= 0x10ffff && scalar is not (>= 0xd800 and <= 0xdfff);
    internal static bool IsStart(int scalar) => scalar == '_' || Contains(Data.Ranges.Start, scalar);
    internal static bool IsContinue(int scalar) => Contains(Data.Ranges.Continue, scalar);
    private static bool Contains(Interval[] ranges, int scalar)
    {
        int lo = 0, hi = ranges.Length;
        while (lo < hi)
        {
            int middle = lo + (hi - lo) / 2;
            if (ranges[middle].Last < scalar) lo = middle + 1;
            else hi = middle;
        }
        return lo < ranges.Length && ranges[lo].First <= scalar;
    }
    // Physical offsets remain UTF-16 code units even for supplementary scalars.
    internal static bool ReadScalar(string text, int offset, out int scalar, out int length)
    {
        scalar = 0; length = 0;
        if (offset >= text.Length) return false;
        char first = text[offset]; length = 1; scalar = first;
        if (first is >= '\ud800' and <= '\udbff')
        {
            if (offset + 1 >= text.Length || text[offset + 1] is not (>= '\udc00' and <= '\udfff')) return false;
            scalar = 0x10000 + ((first - 0xd800) << 10) + text[offset + 1] - 0xdc00; length = 2;
        }
        else if (first is >= '\udc00' and <= '\udfff') return false;
        return true;
    }
    internal static bool IsUcn(string text, int offset) => offset + 1 < text.Length && text[offset] == '\\' && text[offset + 1] is 'u' or 'U';
    internal readonly record struct Problem(string Code, string Message, int Offset, int Length, bool Unsupported = false);
    // Fixed-width C++20 UCNs. Invalid hex is left for subsequent token scanning,
    // as scan_universal_character backs up over the first nonhex character.
    internal static bool ReadUcn(string text, int offset, out int scalar, out int length, out Problem? problem)
    {
        scalar = 0; length = 2; problem = null;
        int digits = text[offset + 1] == 'u' ? 4 : 8, cursor = offset + 2;
        if (cursor < text.Length && text[cursor] == '{')
        {
            while (cursor < text.Length && text[cursor] is not ('}' or '\r' or '\n')) ++cursor;
            if (cursor < text.Length && text[cursor] == '}') ++cursor;
            length = cursor - offset;
            problem = new("UNS031", "Delimited universal character names require an unimplemented language profile.", offset, length, true);
            return false;
        }
        uint value = 0;
        for (int count = 0; count < digits; ++count)
        {
            int digit = cursor < text.Length ? Hex(text[cursor]) : -1;
            if (digit < 0)
            {
                length = cursor - offset;
                problem = new("LEX030", "A universal character name requires exactly four or eight hexadecimal digits.", offset, length);
                return false;
            }
            value = (value << 4) | (uint)digit; ++cursor;
        }
        length = cursor - offset;
        if (value > 0x10ffff || value is >= 0xd800 and <= 0xdfff)
        {
            problem = new("LEX031", "A universal character name must name a Unicode scalar, not a surrogate or an out-of-range value.", offset, length);
            return false;
        }
        scalar = (int)value; return true;
    }
    internal static int Hex(char value) => value is >= '0' and <= '9' ? value - '0' :
        value is >= 'a' and <= 'f' ? value - 'a' + 10 : value is >= 'A' and <= 'F' ? value - 'A' + 10 : -1;
    internal static int EncodeUtf8(int scalar, Span<uint> units)
    {
        if (!IsScalar(scalar)) throw new ArgumentOutOfRangeException(nameof(scalar));
        if (scalar <= 0x7f) { units[0] = (uint)scalar; return 1; }
        if (scalar <= 0x7ff)
        { units[0] = (uint)(0xc0 | scalar >> 6); units[1] = (uint)(0x80 | scalar & 63); return 2; }
        if (scalar <= 0xffff)
        { units[0] = (uint)(0xe0 | scalar >> 12); units[1] = (uint)(0x80 | scalar >> 6 & 63); units[2] = (uint)(0x80 | scalar & 63); return 3; }
        units[0] = (uint)(0xf0 | scalar >> 18); units[1] = (uint)(0x80 | scalar >> 12 & 63);
        units[2] = (uint)(0x80 | scalar >> 6 & 63); units[3] = (uint)(0x80 | scalar & 63); return 4;
    }
    internal static int PartLength(string text, int offset, bool initial)
        => PartLength(text, offset, initial, out _);
    internal static int PartLength(string text, int offset, bool initial, out Problem? problem)
    {
        problem = null;
        if (IsUcn(text, offset)) { ReadUcn(text, offset, out _, out int count, out problem); return count; }
        return ReadScalar(text, offset, out int scalar, out int length) && (initial ? IsStart(scalar) : IsContinue(scalar)) ? length : 0;
    }
    internal static string Identity(string spelling) => Analyze(spelling, out string identity) == null ? identity : spelling;
    // proc_define's ordinary identifier replacement text uses
    // make_canonical_identifier(force_ucn: TRUE). This is a representation
    // boundary, distinct from scalar identity and from raw argument spelling.
    internal static string CanonicalUcnSpelling(string spelling)
    {
        if (Analyze(spelling, out string identity) != null) return spelling;
        if (identity.All(c => c <= 0x7f)) return identity;
        var result = new StringBuilder();
        for (int offset = 0; offset < identity.Length;)
        {
            ReadScalar(identity, offset, out int scalar, out int length);
            if (scalar <= 0x7f) result.Append((char)scalar);
            else result.Append(scalar <= 0xffff ? "\\u" : "\\U")
                .Append(scalar.ToString(scalar <= 0xffff ? "x4" : "x8", CultureInfo.InvariantCulture));
            offset += length;
        }
        return result.ToString();
    }
    internal static Problem? Analyze(string spelling, out string identity)
    {
        StringBuilder? canonical = spelling.Contains('\\') ? new StringBuilder() : null;
        identity = spelling;
        for (int offset = 0; offset < spelling.Length;)
        {
            int scalar, length;
            bool escaped = IsUcn(spelling, offset);
            if (escaped)
            {
                if (!ReadUcn(spelling, offset, out scalar, out length, out Problem? malformed)) return malformed;
                // Basic source letters/digits/underscore cannot be spelled as
                // UCNs in identifiers; literals intentionally do not use this rule.
                if (scalar < 128)
                    return new("LEX032", "A universal character name cannot name a basic character in an identifier.", offset, length);
            }
            else if (!ReadScalar(spelling, offset, out scalar, out length))
                return new("SRC001", "Managed source text contains an unpaired UTF-16 surrogate.", offset, Math.Max(1, length));
            if (!(offset == 0 ? IsStart(scalar) : IsContinue(scalar)))
                return new("LEX033", offset == 0 ? "This Unicode scalar cannot start an identifier." : "This Unicode scalar cannot continue an identifier.", offset, length);
            if (canonical != null) canonical.Append(char.ConvertFromUtf32(scalar));
            offset += length;
        }
        identity = canonical?.ToString() ?? spelling;
        return spelling.Length == 0 ? new("LEX033", "An identifier cannot be empty.", 0, 0) : null;
    }
    internal static void Diagnose(string spelling, SourceRange range, DiagnosticBag diagnostics)
    {
        if (Analyze(spelling, out _) is not Problem issue) return;
        diagnostics.Add(issue.Code, issue.Message, range.File.Range(range.Start + issue.Offset, issue.Length),
            issue.Unsupported ? DiagnosticClass.UnsupportedFeature : DiagnosticClass.InvalidSource);
    }
}

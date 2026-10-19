using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DotCC;

/// <summary>Relocatable, byte-exact string records. Layout happens once, after object linking.</summary>
internal sealed class LiteralPool
{
    internal const string TypeKeyPrefix = "__dotcc_literal_";
    private const string RecordPrefix = "// literal-bytes: ";
    private const int LineWidth = 500;
    private readonly Dictionary<string, string> _records = new(StringComparer.Ordinal);

    internal string Add(IEnumerable<byte> data) => Add(data, payload: false);

    // A payload is immutable assembly data, not the C object it initializes.
    // Keep its record distinct from pointer literals and their owned lifetime.
    internal string AddPayload(IEnumerable<byte> data) => Add(data, payload: true);

    // Reuse the literal record envelope and linker deduplication. The backing
    // type is part of its identity; views never own or merge C destinations.
    internal const int PayloadElements = 4096;
    internal string AddPayload(string backing, ReadOnlySpan<byte> bytes)
    {
        int width = PayloadWidth(backing);
        if (bytes.Length == 0 || bytes.Length % width != 0 || bytes.Length / width > PayloadElements)
            throw new CompileException("primitive literal payload exceeds its element boundary");
        if (backing == "byte") return AddPayload(bytes.ToArray());
        string name = "P" + backing + "_" + Convert.ToHexString(SHA256.HashData(bytes));
        _records[TypeKeyPrefix + name] = Record(bytes);
        return "DotCcLiterals." + name;
    }

    private static int PayloadWidth(string backing) => backing switch
    {
        "byte" => 1, "ushort" => 2, "uint" => 4, "ulong" => 8,
        _ => throw new CompileException("unsupported primitive literal backing")
    };

    private static string Record(ReadOnlySpan<byte> bytes)
    {
        string encoded = Convert.ToBase64String(bytes);
        var record = new StringBuilder();
        for (int i = 0; i < encoded.Length; i += 480)
            record.Append(RecordPrefix).Append(encoded.AsSpan(i, Math.Min(480, encoded.Length - i))).Append('\n');
        return record.ToString();
    }

    private string Add(IEnumerable<byte> data, bool payload)
    {
        var bytes = data.ToArray();
        var name = (payload ? "B" : "S") + Convert.ToHexString(SHA256.HashData(bytes));
        _records[TypeKeyPrefix + name] = Record(bytes);
        return $"DotCcLiterals.{name}";
    }

    private static string Preview(ReadOnlySpan<byte> bytes)
    {
        // Omit only the implicit C terminator. Keep embedded/explicit NULs visible.
        if (!bytes.IsEmpty && bytes[^1] == 0) bytes = bytes[..^1];
        var text = new StringBuilder();
        bool truncated = false;
        for (int i = 0; i < bytes.Length;)
        {
            string escaped;
            if (Rune.DecodeFromUtf8(bytes[i..], out var rune, out int count) != OperationStatus.Done)
            {
                escaped = "\\x" + bytes[i].ToString("X2");
                count = 1;
            }
            else if (rune.Value == '/' && (i > 0 && bytes[i - 1] == '*' || i + 1 < bytes.Length && bytes[i + 1] == '*'))
                escaped = "\\u002F"; // Neither comment delimiter may appear inside the preview.
            else escaped = EscapeRune(rune);
            // A usage-site annotation must not reintroduce giant source lines.
            if (text.Length + escaped.Length > 160) { truncated = true; break; }
            text.Append(escaped);
            i += count;
        }
        return "/*\"" + text + "\"" + (truncated ? "…" : "") + "*/";
    }

    internal void AddDeclarations(IDictionary<string, string> declarations)
    {
        foreach (var entry in _records) declarations.Add(entry.Key, entry.Value);
    }

    internal static Output CreateOutput(IReadOnlyDictionary<string, string> declarations, string className, bool enabled)
    {
        var replacements = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Legacy object markers represent declared array storage, not
            // string literals. Preserve identity in either final output mode.
            ["ConstArray"] = "Libc.GlobalArrayFrom",
            ["ConstBytes"] = "Libc.GlobalArrayFrom<byte>"
        };
        int index = 0;
        var utf8 = new List<byte>();
        var binary = new List<byte>();
        var offsets = new StringBuilder();
        var binaryOffsets = new List<(string Name, int Offset)>();
        var strictUtf8 = new UTF8Encoding(false, true);
        var payloads = new StringBuilder();
        foreach (var (key, record) in declarations.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            if (!key.StartsWith(TypeKeyPrefix, StringComparison.Ordinal)) continue;
            var name = key[TypeKeyPrefix.Length..];
            byte[] bytes;
            string backing = "byte";
            try
            {
                var lines = record.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Any(line => !line.StartsWith(RecordPrefix, StringComparison.Ordinal)))
                    throw new FormatException();
                bytes = Convert.FromBase64String(string.Concat(lines.Select(line => line[RecordPrefix.Length..])));
                string hash;
                if (name.StartsWith('P'))
                {
                    int separator = name.IndexOf('_');
                    if (separator < 0) throw new FormatException();
                    backing = name[1..separator];
                    if (backing is not ("ushort" or "uint" or "ulong")) throw new FormatException();
                    int width = PayloadWidth(backing);
                    if (bytes.Length == 0 || bytes.Length % width != 0 || bytes.Length / width > PayloadElements)
                        throw new FormatException();
                    hash = name[(separator + 1)..];
                }
                else
                {
                    if (name.Length == 0 || name[0] is not ('S' or 'B')) throw new FormatException();
                    hash = name[1..];
                }
                if (hash != Convert.ToHexString(SHA256.HashData(bytes))) throw new FormatException();
            }
            catch (FormatException) { throw new CompileException("invalid literal object record; regenerate objects"); }
            if (name[0] is 'B' or 'P')
            {
                // C# lowers this constant collection expression to RVA data.
                // Never substitute a cached/new byte[]: callers only borrow the
                // span while copying into each C declaration's own storage.
                replacements.Add(name, "DotCcLiterals." + name);
                payloads.Append("        internal static global::System.ReadOnlySpan<").Append(backing).Append("> ").Append(name).Append(" => [\n            ");
                int column = 12;
                int width = PayloadWidth(backing);
                for (int offset = 0; offset < bytes.Length; offset += width)
                {
                    // The target ABI is little-endian. Assemble explicitly,
                    // independently of the compiler host's byte order.
                    ulong bits = 0;
                    for (int b = 0; b < width; b++) bits |= (ulong)bytes[offset + b] << (b * 8);
                    string value = "0x" + bits.ToString("X" + (width * 2))
                        + (width == 8 ? "UL" : width == 4 ? "U" : "") + ", ";
                    if (column + value.Length > LineWidth) { payloads.Append("\n            "); column = 12; }
                    payloads.Append(value); column += value.Length;
                }
                payloads.Append("\n        ];\n");
                continue;
            }
            if (!enabled)
            {
                replacements.Add(name, LegacyLiteral(bytes));
                continue;
            }
            var shortName = "S" + index++;
            replacements.Add(name, $"(DotCcLiterals.Pointer + DotCcLiterals.{shortName} {Preview(bytes)})");
            bool isUtf8;
            try { strictUtf8.GetCharCount(bytes); isUtf8 = true; }
            catch (DecoderFallbackException) { isUtf8 = false; }
            if (isUtf8)
            {
                AppendOffset(offsets, shortName, utf8.Count);
                utf8.AddRange(bytes);
            }
            else
            {
                binaryOffsets.Add((shortName, binary.Count));
                binary.AddRange(bytes);
            }
        }
        if (offsets.Length == 0 && binaryOffsets.Count == 0)
            return new Output(payloads.Length == 0 ? ""
                : "internal static class " + className + "\n{\n" + payloads + "}\n", replacements);
        foreach (var (name, offset) in binaryOffsets) AppendOffset(offsets, name, checked(utf8.Count + offset));
        int length = checked(utf8.Count + binary.Count);
        var sb = new StringBuilder("internal static unsafe class ").Append(className).Append("\n{\n");
        sb.Append(payloads);
        sb.Append("        private static readonly byte[] Storage;\n        internal static readonly byte* Pointer;\n");
        sb.Append(offsets);
        sb.Append("        static ").Append(className).Append("()\n        {\n");
        sb.Append("            Storage = GC.AllocateUninitializedArray<byte>(").Append(length).Append(", pinned: true);\n");
        sb.Append("            Span<byte> storage = Storage;\n");
        if (utf8.Count != 0)
        {
            sb.Append("            ReadOnlySpan<byte> utf8 =\n").Append(FormatUtf8(strictUtf8.GetString(utf8.ToArray()), "                ")).Append(";\n");
            sb.Append("            utf8.CopyTo(storage.Slice(0, ").Append(utf8.Count).Append("));\n");
        }
        if (binary.Count != 0)
        {
            sb.Append("            ReadOnlySpan<byte> binary = [\n                ");
            int column = 16;
            foreach (byte b in binary)
            {
                if (column + 6 > LineWidth) { sb.Append("\n                "); column = 16; }
                sb.Append("0x").Append(b.ToString("X2")).Append(", "); column += 6;
            }
            sb.Append("\n            ];\n            binary.CopyTo(storage.Slice(").Append(utf8.Count).Append(", ").Append(binary.Count).Append("));\n");
        }
        return new Output(sb.Append("            Pointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(Storage));\n        }\n}\n").ToString(), replacements);
    }

    private static string LegacyLiteral(byte[] bytes)
    {
        try
        {
            var value = new UTF8Encoding(false, true).GetString(bytes);
            return "Libc.L(\"" + string.Concat(value.EnumerateRunes().Select(EscapeRune)) + "\"u8)";
        }
        catch (DecoderFallbackException)
        {
            return "Libc.L(new byte[]{ " + string.Join(", ", bytes.Select(b => "0x" + b.ToString("X2"))) + " })";
        }
    }

    /// <summary>Resolve backend references before source splitting or adding runtime code.</summary>
    internal sealed class Output(string source, IReadOnlyDictionary<string, string> replacements)
    {
        internal string Source { get; } = source;

        internal string Rewrite(string text)
        {
            const string prefix = "DotCcLiterals.";
            if (!text.Contains(prefix, StringComparison.Ordinal)) return text;
            var result = new StringBuilder(text.Length);
            int copied = 0;
            // Backend snippets contain regular/verbatim quoted literals and comments.
            // Skip them so a C wide string or comment resembling a reference is untouched.
            for (int i = 0; i < text.Length;)
            {
                if (text[i] is '"' or '\'')
                {
                    char quote = text[i++];
                    bool verbatim = quote == '"' && i >= 2 && text[i - 2] == '@';
                    while (i < text.Length)
                    {
                        if (!verbatim && text[i] == '\\') { i = Math.Min(i + 2, text.Length); continue; }
                        if (text[i++] != quote) continue;
                        if (verbatim && i < text.Length && text[i] == quote) { i++; continue; }
                        break;
                    }
                }
                else if (text.AsSpan(i).StartsWith("//"))
                {
                    int end = text.IndexOf('\n', i + 2);
                    i = end < 0 ? text.Length : end + 1;
                }
                else if (text.AsSpan(i).StartsWith("/*"))
                {
                    int end = text.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    i = end < 0 ? text.Length : end + 2;
                }
                else if (text.AsSpan(i).StartsWith(prefix) && (i == 0 || !Identifier(text[i - 1])))
                {
                    int end = i + prefix.Length;
                    while (end < text.Length && Identifier(text[end])) end++;
                    var name = text[(i + prefix.Length)..end];
                    if (!replacements.TryGetValue(name, out var replacement))
                        throw new CompileException("unknown literal reference; regenerate objects");
                    result.Append(text.AsSpan(copied, i - copied)).Append(replacement);
                    copied = i = end;
                }
                else i++;
            }
            return result.Append(text.AsSpan(copied)).ToString();
        }

        private static bool Identifier(char c) => char.IsLetterOrDigit(c) || c is '_' or '@';
    }

    private static void AppendOffset(StringBuilder sb, string name, int offset) =>
        sb.Append("        internal const int ").Append(name).Append(" = ").Append(offset).Append(";\n");

    /// <summary>C# folds u8 + u8 at compile time. Never split an escape or Unicode scalar.</summary>
    internal static string FormatUtf8(string value, string indent)
    {
        var chunks = new List<string>();
        var line = new StringBuilder();
        foreach (var rune in value.EnumerateRunes())
        {
            string text = EscapeRune(rune);
            if (line.Length + text.Length + indent.Length + 6 > LineWidth && line.Length != 0)
            {
                chunks.Add(line.ToString()); line.Clear();
            }
            line.Append(text);
        }
        if (line.Length != 0 || chunks.Count == 0) chunks.Add(line.ToString());
        return string.Join(" +\n", chunks.Select(chunk => indent + "\"" + chunk + "\"u8"));
    }

    private static string EscapeRune(Rune rune) => rune.Value switch
    {
        0 => "\\0", 9 => "\\t", 10 => "\\n", 13 => "\\r", 34 => "\\\"", 92 => "\\\\",
        >= 32 and <= 126 => rune.ToString(),
        _ when rune.Value > 127 && !Rune.IsControl(rune) && rune.Value is not (0x2028 or 0x2029) => rune.ToString(),
        _ => "\\u" + rune.Value.ToString("X4")
    };
}

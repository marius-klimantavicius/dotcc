#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private readonly Dictionary<string, LitStr> _embeddedFiles = new(StringComparer.Ordinal);

    // C const-array pointees qualify the element as well. Zig's value/child TYPE has
    // no const qualifier: const belongs to the pointer, not the loaded array value.
    private static CType SourceSentinelArrayValueType(CType type)
        => type.Unqualified is CType.Array { ZigSentinel: not null } array
            ? array with { Element = array.Element.Unqualified } : type;

    /// <summary>Read a demanded source-relative resource once, without text decoding. LitStr's
    /// existing comptime byte array and literal object record preserve its bytes and trailing zero.
    /// Its source type is a single const pointer to [N:0]u8, rather than a C string array.</summary>
    private LitStr LowerEmbedFile(IReadOnlyList<Item> args)
    {
        if (args.Count != 1)
            throw new CompileException("zig `@embedFile` expects one comptime path argument");
        var name = TryComptimeByteName(args[0]);
        if (name is null)
            throw new CompileException("zig `@embedFile` path must be a comptime UTF-8 string");
        if (name.Length == 0 || name.Contains('\0'))
            throw new CompileException("zig `@embedFile` path must be nonempty and contain no zero bytes");
        if (_importerDir is null)
            throw new CompileException("zig `@embedFile` requires a source file directory");
        var path = Path.GetFullPath(name, Path.GetFullPath(_importerDir));
        if (_embeddedFiles.TryGetValue(path, out var known)) return known;
        byte[] bytes;
        try { bytes = File.ReadAllBytes(path); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new CompileException($"zig `@embedFile` cannot read '{path}' (source '{_module?.Path ?? _fileStem}'): {error.Message}", error);
        }
        var array = new CType.Array(CType.UChar, bytes.Length) { ZigSentinel = 0 }.WithQuals(TypeQual.Const);
        var literal = ComptimeStringFromBytes(bytes.Select(b => (int)b)) with
        {
            Type = SourcePointer(array),
        };
        _embeddedFiles.Add(path, literal);
        return literal;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using LALR.CC;
using LALR.CC.LexicalGrammar;

namespace DotCC;

/// <summary>Phase-two text plus the removed UTF-8 byte ranges. Lexer coordinates
/// stay logical until preprocessing has finished; only source reporting uses
/// the original physical coordinates.</summary>
internal sealed class PhysicalSourceMap
{
    public string Text { get; }
    internal SourceFileOrigin? SourceFile { get; }
    public bool HasSplices => _spliceOffsets.Length != 0;
    private readonly byte[] _original;
    private readonly int[] _spliceOffsets, _removedBytes, _lineStarts;
    private readonly int _initialLine;

    public PhysicalSourceMap(string source, int initialLine = 1, string? filename = null, string? identity = null)
    {
        _initialLine = initialLine;
        _original = Encoding.UTF8.GetBytes(source);
        // Headers retain identity across TUs without depending on include order.
        // Main files supply their absolute path to keep private declarations distinct.
        SourceFile = filename is null ? null : new SourceFileOrigin(filename,
            identity ?? filename + ":" + Convert.ToHexString(SHA256.HashData(_original)));
        var offsets = new List<int>();
        var removed = new List<int>();
        var lines = new List<int> { 0 };
        var output = new List<byte>(_original.Length);
        var removedCount = 0;
        for (var i = 0; i < _original.Length; ++i)
        {
            if (_original[i] == '\n') lines.Add(i + 1);
            var spliceLength = _original[i] == '\\' && i + 1 < _original.Length
                ? _original[i + 1] == '\n' ? 2
                    : _original[i + 1] == '\r' && i + 2 < _original.Length && _original[i + 2] == '\n' ? 3 : 0
                : 0;
            if (spliceLength != 0)
            {
                lines.Add(i + spliceLength);
                removedCount += spliceLength;
                offsets.Add(output.Count);
                removed.Add(removedCount);
                i += spliceLength - 1;
            }
            else output.Add(_original[i]);
        }
        _spliceOffsets = offsets.ToArray();
        _removedBytes = removed.ToArray();
        _lineStarts = lines.ToArray();
        Text = offsets.Count == 0 ? source : Encoding.UTF8.GetString(output.ToArray());
    }

    public SourcePosition Physical(SourcePosition logical)
    {
        if (!HasSplices || !logical.IsKnown) return logical;
        var splice = LastAtOrBefore(_spliceOffsets, logical.ByteOffset);
        var offset = logical.ByteOffset + (splice < 0 ? 0 : _removedBytes[splice]);
        var line = LastAtOrBefore(_lineStarts, offset);
        var column = 1;
        for (var i = _lineStarts[line]; i < offset && i < _original.Length; ++i)
            if ((_original[i] & 0xc0) != 0x80) ++column;
        return new SourcePosition(line + _initialLine, column, offset);
    }

    public int PhysicalEndLine(SourcePosition logical)
    {
        if (!HasSplices) return logical.Line;
        // A #line directive controls the physical line following the whole
        // logical directive, including any trailing continued empty lines.
        var bytes = Encoding.UTF8.GetBytes(Text);
        var offset = (int)logical.ByteOffset;
        while (offset < bytes.Length && bytes[offset] != '\n') ++offset;
        return Physical(new SourcePosition(logical.Line, logical.Column, offset)).Line;
    }

    private static int LastAtOrBefore(int[] values, long value)
    {
        var low = 0;
        var high = values.Length;
        while (low < high)
        {
            var middle = low + (high - low) / 2;
            if (values[middle] <= value) low = middle + 1;
            else high = middle;
        }
        return low - 1;
    }
}

/// <summary>Logical Position remains available for macro adjacency. Origin may
/// instead refer to a macro invocation, so diagnostics and __LINE__ report the
/// use site while whitespace still comes from replacement-list spelling.</summary>
internal class SourceMappedItem : Item
{
    internal int? Packing { get; set; }
    private readonly PhysicalSourceMap? _map;
    private readonly SourcePosition _origin;

    internal SourceMappedItem(Item item) : this(item, item) { }
    internal SourceMappedItem(Item item, Item origin) : base(item.ID, item.Content, item.Position)
    {
        _map = (origin as SourceMappedItem)?._map;
        _origin = origin is SourceMappedItem mapped ? mapped._origin : origin.Position;
        Packing = SourcePacking.Recorded(origin);
    }
    internal SourceMappedItem(Item item, PhysicalSourceMap map) : base(item.ID, item.Content, item.Position)
    {
        _map = map;
        _origin = item.Position;
    }
    internal static string LogicalSpelling(IReadOnlyList<Item> tokens)
    {
        if (tokens.Count == 0) return "";
        var first = tokens[0]; var last = tokens[^1];
        if (first is not SourceMappedItem { _map: { } map })
            throw new CompileException("macro override requires original source spelling");
        var bytes = Encoding.UTF8.GetBytes(map.Text);
        var start = checked((int)first.Position.ByteOffset);
        var end = checked((int)last.Position.ByteOffset + Encoding.UTF8.GetByteCount(last.Content?.ToString() ?? ""));
        return Encoding.UTF8.GetString(bytes, start, end - start);
    }
    internal static SourceFileOrigin? FileOf(Item item) => (item as SourceMappedItem)?._map?.SourceFile;
    internal static SourcePosition Physical(Item item) => item is SourceMappedItem mapped
        ? mapped._map?.Physical(mapped._origin) ?? mapped._origin : item.Position;
    internal static int PhysicalEndLine(Item item) => item is SourceMappedItem mapped
        ? mapped._map?.PhysicalEndLine(mapped._origin) ?? mapped._origin.Line : item.Position.Line;
    internal static Item Create(int id, object? content, Item origin) =>
        new SourceMappedItem(new Item(id, content, origin.Position), origin);
}

internal sealed class SourceMappingLexer : ISyncIterator<Item>
{
    private readonly ISyncIterator<Item> _inner;
    private readonly PhysicalSourceMap _map;
    public Item Current { get; private set; } = null!;
    public bool SupportsResetting => _inner.SupportsResetting;
    internal SourceMappingLexer(ISyncIterator<Item> inner, PhysicalSourceMap map) { _inner = inner; _map = map; }
    public bool MoveNext()
    {
        try
        {
            if (!_inner.MoveNext()) return false;
            Current = _map.HasSplices || _map.SourceFile is not null ? new SourceMappedItem(_inner.Current, _map) : _inner.Current;
            return true;
        }
        catch (LexerException error)
        {
            var physicalError = new LexerException(_map.Physical(error.Position), error.OffendingByte, error.LexerStateName);
            if (_map.SourceFile is { } file) throw new CompileException($"lex failed in {file.Name}: {physicalError.Message}", physicalError);
            throw physicalError;
        }
    }
    public void Reset() => _inner.Reset();
    public void Dispose() => _inner.Dispose();
}

internal sealed class PhysicalPositionRewriter(ISyncIterator<Item> inner) : RewritingTokenStream(inner)
{
    protected override void ProcessToken(Item token)
    {
        var position = SourceMappedItem.Physical(token);
        Emit(SourceMappedItem.FileOf(token) is { } source
            ? new SourceLocatedItem(token.ID, token.Content, position, source, SourcePacking.Of(token))
            : token is SourceMappedItem
                ? new SourcePackingItem(new Item(token.ID, token.Content, position), SourcePacking.Of(token)) : token);
    }
}

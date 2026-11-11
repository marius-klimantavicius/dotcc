using System.Runtime.InteropServices;
using System.Text;
using Raw = global::Managed.Cui.Cui;
using static global::Managed.Cui.Cui;

namespace Cui.Managed;

public sealed class Texture : IDisposable
{
    internal readonly Ui Owner;
    internal readonly ulong Value;
    internal bool Disposed;
    internal Texture(Ui owner, ulong value) { Owner = owner; Value = value; }
    public void Dispose() { Owner.CheckOwnerThread(); if (Disposed) return; Owner.Check(); Disposed = true; Owner.ReleaseTexture(this); }
    internal void Invalidate() => Disposed = true;
}

/// <summary>Persistent storage for the upstream ANSI terminal parser.</summary>
public sealed unsafe class TerminalBuffer : IDisposable
{
    internal cui_terminal_buffer* Pointer;
    private byte* text;
    private cui_terminal_line* lines;
    private readonly int owner = Environment.CurrentManagedThreadId;
    internal Ui? Context;
    private bool disposed;
    private bool leased;
    public TerminalBuffer(int textCapacity = 65536, int lineCapacity = 1024)
    {
        if (textCapacity < 2 || lineCapacity < 1) throw new ArgumentOutOfRangeException(nameof(textCapacity));
        try
        {
            Pointer = (cui_terminal_buffer*)NativeMemory.AllocZeroed((nuint)sizeof(cui_terminal_buffer));
            text = (byte*)NativeMemory.AllocZeroed((nuint)textCapacity);
            lines = (cui_terminal_line*)NativeMemory.AllocZeroed(checked((nuint)lineCapacity * (nuint)sizeof(cui_terminal_line)));
            if (Pointer == null || text == null || lines == null) throw new OutOfMemoryException();
            Raw.cui_terminal_init(Pointer, text, (ulong)textCapacity, lines, lineCapacity);
        }
        catch { Free(); throw; }
    }
    private void Check()
    {
        if (owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Use the terminal on its owner thread.");
        ObjectDisposedException.ThrowIf(disposed, this); Context?.Check();
    }
    public void Write(string text)
    {
        Check(); ArgumentNullException.ThrowIfNull(text);
        int count = Encoding.UTF8.GetByteCount(text);
        byte[]? rented = null;
        Span<byte> bytes = count <= 1024 ? stackalloc byte[count] : (rented = System.Buffers.ArrayPool<byte>.Shared.Rent(count)).AsSpan(0, count);
        try { Encoding.UTF8.GetBytes(text, bytes); Write(bytes); }
        finally { if (rented != null) System.Buffers.ArrayPool<byte>.Shared.Return(rented); }
    }
    public void Write(ReadOnlySpan<byte> bytes) { Check(); fixed (byte* p = bytes) Raw.cui_terminal_write(Pointer, p, bytes.Length); }
    public void Clear() { Check(); Raw.cui_terminal_clear(Pointer); }
    internal void Acquire(Ui ui)
    {
        Check();
        if (Context != null && Context != ui) throw new ArgumentException("Terminal belongs to another context.");
        Context = ui; leased = true;
    }
    internal void Release() { leased = false; if (disposed) Free(); }
    public void Dispose()
    {
        if (owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Dispose on the terminal owner thread.");
        if (disposed) return; disposed = true; if (!leased) Free();
    }
    private void Free() { NativeMemory.Free(Pointer); NativeMemory.Free(text); NativeMemory.Free(lines); Pointer = null; text = null; lines = null; GC.SuppressFinalize(this); }
    ~TerminalBuffer() { Free(); }
}

public sealed unsafe partial class Ui
{
    private ulong Unwrap(Texture? texture)
    {
        if (texture == null) return 0;
        if (texture.Owner != this) throw new ArgumentException("Texture belongs to another context.");
        ObjectDisposedException.ThrowIf(texture.Disposed, texture);
        return texture.Value;
    }
    private Texture? FindTexture(ulong value)
    {
        if (value == 0) return null;
        if (textures.TryGetValue(value, out var texture)) return texture;
        texture = new Texture(this, value); textures.Add(value, texture); return texture;
    }
    public Texture ImageCreate(int width, int height, ReadOnlySpan<byte> rgba)
    {
        Check();
        if (width < 1 || height < 1) throw new ArgumentOutOfRangeException(nameof(width));
        CheckLength(checked(width * height * 4), rgba.Length, nameof(rgba));
        fixed (byte* p = rgba)
        {
            ulong value = Raw.cui_image_create(ctx, width, height, p);
            if (value == 0) throw new InvalidOperationException("CUI image creation failed.");
            return FindTexture(value)!;
        }
    }
    public Texture ImageLoad(string path, out int width, out int height)
    {
        Check(); ArgumentNullException.ThrowIfNull(path); int w, h;
        ulong value = Raw.cui_image_load(ctx, Utf8(path), &w, &h); width = w; height = h;
        if (value == 0) throw new InvalidOperationException("CUI image loading failed.");
        return FindTexture(value)!;
    }
    internal void ReleaseTexture(Texture texture)
    {
        // Draw data may be read more than once (capture/re-render); drain at next frame.
        pendingTextures.Add(texture);
    }
    private void ReleasePendingTextures()
    {
        foreach (var texture in pendingTextures)
        {
            Raw.cui_image_destroy(ctx, texture.Value); textures.Remove(texture.Value);
        }
        pendingTextures.Clear();
    }
    public TerminalResult Terminal(string id, TerminalBuffer terminal, TerminalOpts opts = default)
    {
        CheckFrame(); ArgumentNullException.ThrowIfNull(terminal);
        terminal.Acquire(this); terminals.Add(terminal);
        var native = ToNative(opts);
        var result = Raw.cui_terminal_ex(ctx, Utf8(id), terminal.Pointer, &native);
        SyncState(); return FromNative(result);
    }
    public int FsList(string path, Span<FileEntry> entries, bool hidden = false, int nameBytes = 65536)
    {
        Check(); ArgumentNullException.ThrowIfNull(path);
        if (nameBytes < 1) throw new ArgumentOutOfRangeException(nameof(nameBytes));
        var output = (cui_file_entry*)arena.Allocate(checked(entries.Length * sizeof(cui_file_entry)));
        var names = (byte*)arena.Allocate(nameBytes);
        int count = Raw.cui_fs_list(null, Utf8(path), output, entries.Length, names, (ulong)nameBytes, hidden);
        if (count > entries.Length) throw new InvalidOperationException("Invalid CUI filesystem result.");
        for (int i = 0; i < count; i++) entries[i] = FromNative(output[i], entries[i]);
        return count;
    }
    public ulong QueryText(ReadOnlySpan<QueryField> fields, int fieldCount, ReadOnlySpan<QueryNode> nodes, int count, TextBuffer output)
    {
        Check(); CheckLength(fieldCount, fields.Length, nameof(fieldCount)); CheckLength(count, nodes.Length, nameof(count));
        ValidateQueryNodes(nodes[..count], fieldCount);
        return Raw.cui_query_text(CopyQueryField(fields), fieldCount, CopyQueryNode(nodes), count, Use(output), (ulong)output.Capacity);
    }
    public int UrlParams(string url, Span<KvPair> pairs)
    {
        Check(); var native = CopyKvPair(pairs); int result = Raw.cui_url_params(Utf8Required(url), native, pairs.Length);
        for (int i = 0; i < Math.Min(result, pairs.Length); i++) pairs[i] = FromNative(native[i], pairs[i]); return result;
    }
    public bool UrlSetParams(TextBuffer url, ReadOnlySpan<KvPair> pairs, int count)
    {
        Check(); CheckLength(count, pairs.Length, nameof(count));
        return Raw.cui_url_set_params(Use(url), (ulong)url.Capacity, CopyKvPair(pairs), count) != 0;
    }
}

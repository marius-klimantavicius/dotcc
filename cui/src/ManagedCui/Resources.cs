using System.Runtime.InteropServices;
using System.Text;
using static global::Managed.Forms.Cui;

namespace Cui.Managed;

public sealed class Texture : IDisposable
{
    internal readonly Ui Owner;
    internal readonly ulong Value;
    internal bool Disposed;

    internal Texture(Ui owner, ulong value)
    {
        Owner = owner;
        Value = value;
    }

    public void Dispose()
    {
        Owner.CheckOwnerThread();
        if (Disposed) return;

        Owner.Check();
        Disposed = true;
        Owner.ReleaseTexture(this);
    }

    internal void Invalidate() => Disposed = true;
}

/// <summary>Persistent storage for the upstream ANSI terminal parser.</summary>
public sealed unsafe class TerminalBuffer : IDisposable
{
    internal cui_terminal_buffer* Pointer;
    private byte* _text;
    private cui_terminal_line* _lines;
    private readonly int _owner = Environment.CurrentManagedThreadId;
    internal Ui? Context;
    private bool _disposed;
    private bool _leased;

    public TerminalBuffer(int textCapacity = 65536, int lineCapacity = 1024)
    {
        if (textCapacity < 2 || lineCapacity < 1) throw new ArgumentOutOfRangeException(nameof(textCapacity));

        try
        {
            Pointer = (cui_terminal_buffer*)NativeMemory.AllocZeroed((nuint)sizeof(cui_terminal_buffer));
            _text = (byte*)NativeMemory.AllocZeroed((nuint)textCapacity);
            _lines = (cui_terminal_line*)NativeMemory.AllocZeroed(checked((nuint)lineCapacity * (nuint)sizeof(cui_terminal_line)));
            if (Pointer == null || _text == null || _lines == null) throw new OutOfMemoryException();

            cui_terminal_init(Pointer, _text, (ulong)textCapacity, _lines, lineCapacity);
        }
        catch
        {
            Free();
            throw;
        }
    }

    private void Check()
    {
        if (_owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Use the terminal on its owner thread.");

        ObjectDisposedException.ThrowIf(_disposed, this);
        Context?.Check();
    }

    public void Write(string text)
    {
        Check();
        ArgumentNullException.ThrowIfNull(text);
        var count = Encoding.UTF8.GetByteCount(text);
        byte[]? rented = null;
        var bytes = count <= 1024 ? stackalloc byte[count] : (rented = System.Buffers.ArrayPool<byte>.Shared.Rent(count)).AsSpan(0, count);
        try
        {
            Encoding.UTF8.GetBytes(text, bytes);
            Write(bytes);
        }
        finally
        {
            if (rented != null) System.Buffers.ArrayPool<byte>.Shared.Return(rented);
        }
    }

    public void Write(ReadOnlySpan<byte> bytes)
    {
        Check();
        fixed (byte* p = bytes) cui_terminal_write(Pointer, p, bytes.Length);
    }

    public void Write(TextBuffer text)
    {
        Check();
        ArgumentNullException.ThrowIfNull(text);
        Write(text.ReadUtf8());
    }

    public void Clear()
    {
        Check();
        cui_terminal_clear(Pointer);
    }

    internal void Acquire(Ui ui)
    {
        Check();
        if (Context != null && Context != ui) throw new ArgumentException("Terminal belongs to another context.");

        Context = ui;
        _leased = true;
    }

    internal void Release()
    {
        _leased = false;
        if (_disposed) Free();
    }

    public void Dispose()
    {
        if (_owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Dispose on the terminal owner thread.");

        if (_disposed) return;

        _disposed = true;
        if (!_leased) Free();
    }

    private void Free()
    {
        NativeMemory.Free(Pointer);
        NativeMemory.Free(_text);
        NativeMemory.Free(_lines);
        Pointer = null;
        _text = null;
        _lines = null;
        GC.SuppressFinalize(this);
    }

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
        if (_textures.TryGetValue(value, out var texture)) return texture;

        texture = new Texture(this, value);
        _textures.Add(value, texture);
        return texture;
    }

    public Texture ImageCreate(int width, int height, ReadOnlySpan<byte> rgba)
    {
        Check();
        if (width < 1 || height < 1) throw new ArgumentOutOfRangeException(nameof(width));

        CheckLength(checked(width * height * 4), rgba.Length, nameof(rgba));
        fixed (byte* p = rgba)
        {
            var value = cui_image_create(_ctx, width, height, p);
            if (value == 0) throw new InvalidOperationException("CUI image creation failed.");

            return FindTexture(value)!;
        }
    }

    public Texture ImageLoad(string path, out int width, out int height)
    {
        Check();
        ArgumentNullException.ThrowIfNull(path);
        int w, h;
        using var __pathUtf8 = new ScopedUtf8(path, stackalloc byte[256]);
        fixed (byte* __pathPointer = __pathUtf8.Bytes)
        {
            var value = cui_image_load(_ctx, __pathPointer, &w, &h);
            width = w;
            height = h;
            if (value == 0) throw new InvalidOperationException("CUI image loading failed.");

            return FindTexture(value)!;
        }
    }

    internal void ReleaseTexture(Texture texture)
    {
        // Draw data may be read more than once (capture/re-render); drain at next frame.
        _pendingTextures.Add(texture);
    }

    private void ReleasePendingTextures()
    {
        foreach (var texture in _pendingTextures)
        {
            cui_image_destroy(_ctx, texture.Value);
            _textures.Remove(texture.Value);
        }

        _pendingTextures.Clear();
    }

    public TerminalResult Terminal(string id, TerminalBuffer terminal, TerminalOpts opts = default)
    {
        CheckFrame();
        ArgumentNullException.ThrowIfNull(terminal);
        terminal.Acquire(this);
        _terminals.Add(terminal);
        var native = ToNative(opts);
        using var __idUtf8 = new ScopedUtf8(id, stackalloc byte[256]);
        fixed (byte* __idPointer = __idUtf8.Bytes)
        {
            var result = cui_terminal_ex(_ctx, __idPointer, terminal.Pointer, &native);
            SyncState();
            return FromNative(result);
        }
    }

    public int FsList(string path, Span<FileEntry> entries, bool hidden = false, int nameBytes = 65536)
    {
        Check();
        ArgumentNullException.ThrowIfNull(path);
        if (nameBytes < 1) throw new ArgumentOutOfRangeException(nameof(nameBytes));

        var output = (cui_file_entry*)_arena.Allocate(checked(entries.Length * sizeof(cui_file_entry)));
        var names = (byte*)_arena.Allocate(nameBytes);
        using var __pathUtf8 = new ScopedUtf8(path, stackalloc byte[256]);
        fixed (byte* __pathPointer = __pathUtf8.Bytes)
        {
            var count = cui_fs_list(null, __pathPointer, output, entries.Length, names, (ulong)nameBytes, hidden);
            if (count > entries.Length) throw new InvalidOperationException("Invalid CUI filesystem result.");

            for (var i = 0; i < count; i++) entries[i] = FromNative(output[i], entries[i]);
            return count;
        }
    }

    public ulong QueryText(ReadOnlySpan<QueryField> fields, int fieldCount, ReadOnlySpan<QueryNode> nodes, int count, TextBuffer output)
    {
        Check();
        CheckLength(fieldCount, fields.Length, nameof(fieldCount));
        CheckLength(count, nodes.Length, nameof(count));
        ValidateQueryNodes(nodes[..count], fieldCount);
        return cui_query_text(CopyQueryField(fields), fieldCount, CopyQueryNode(nodes), count, Use(output), (ulong)output.Capacity);
    }

    public int UrlParams(string url, Span<KvPair> pairs)
    {
        Check();
        var native = CopyKvPair(pairs);
        using var __urlUtf8 = new ScopedUtf8(url, stackalloc byte[256]);
        fixed (byte* __urlPointer = __urlUtf8.Bytes)
        {
            var result = cui_url_params(__urlPointer, native, pairs.Length);
            for (var i = 0; i < Math.Min(result, pairs.Length); i++) pairs[i] = FromNative(native[i], pairs[i]);
            return result;
        }
    }

    public bool UrlSetParams(TextBuffer url, ReadOnlySpan<KvPair> pairs, int count)
    {
        Check();
        CheckLength(count, pairs.Length, nameof(count));
        return cui_url_set_params(Use(url), (ulong)url.Capacity, CopyKvPair(pairs), count) != 0;
    }
}
using System.Runtime.InteropServices;
using System.Text;

namespace Cui.Managed;

public enum Font
{
    Body,
    Medium,
    Display,
    Mono,
    Icons,
}

/// <summary>A CUI widget identity bound to its creating context.</summary>
public readonly struct Id : IEquatable<Id>
{
    internal readonly Ui? Owner;
    internal readonly uint Value;

    internal Id(Ui owner, uint value)
    {
        Owner = owner;
        Value = value;
    }

    public bool IsEmpty => Value == 0;
    public bool Equals(Id other) => ReferenceEquals(Owner, other.Owner) && Value == other.Value;
    public override bool Equals(object? obj) => obj is Id other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Owner, Value);
    public static bool operator ==(Id a, Id b) => a.Equals(b);
    public static bool operator !=(Id a, Id b) => !a.Equals(b);
}

/// <summary>Reusable, context-bound scalar state for writable pointers in options.</summary>
public sealed class State<T>
{
    private T _value;
    internal Ui? Owner;
    internal IStateLease? Lease;
    public State(T value = default!) { _value = value; }

    public T Value
    {
        get
        {
            Owner?.Check();
            return _value;
        }
        set
        {
            Owner?.Check();
            _value = value;
            Lease?.Write();
        }
    }

    internal T Current { get => _value; set => _value = value; }
}

internal interface IStateLease : IDisposable
{
    void Read();
    void Write();
}

internal sealed unsafe class StateLease<T, R> : IStateLease where R : unmanaged
{
    private readonly Ui _ui;
    private readonly State<T> _state;
    internal R* Pointer;
    private readonly delegate*<Ui, T, R> _encode;
    private readonly delegate*<Ui, R, T, T> _decode;

    internal StateLease(Ui ui, State<T> state, delegate*<Ui, T, R> encode, delegate*<Ui, R, T, T> decode)
    {
        _ui = ui;
        _state = state;
        _encode = encode;
        _decode = decode;
        Pointer = (R*)NativeMemory.AllocZeroed((nuint)sizeof(R));
        if (Pointer is null) throw new OutOfMemoryException();
    }

    public void Write()
    {
        if (Pointer != null) *Pointer = _encode(_ui, _state.Current);
    }

    public void Read()
    {
        if (Pointer != null) _state.Current = _decode(_ui, *Pointer, _state.Current);
    }

    public void Dispose()
    {
        NativeMemory.Free(Pointer);
        Pointer = null;
    }
}

/// <summary>Persistent UTF-8 storage. Capacity includes the trailing NUL byte.</summary>
public sealed unsafe class TextBuffer : IDisposable
{
    private byte* _data;
    private readonly int _owner = Environment.CurrentManagedThreadId;
    internal Ui? Context;
    private bool _disposed;
    private int _leases;
    public int Capacity { get; }

    public TextBuffer(int capacity, string text = "")
    {
        if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));

        Capacity = capacity;
        _data = (byte*)NativeMemory.AllocZeroed((nuint)capacity);
        if (_data == null) throw new OutOfMemoryException();

        try { Set(text); }
        catch
        {
            NativeMemory.Free(_data);
            _data = null;
            throw;
        }
    }

    public TextBuffer(int capacity, ReadOnlySpan<byte> text) : this(capacity)
    {
        try { SetUtf8(text); }
        catch
        {
            Free();
            throw;
        }
    }

    private void Check()
    {
        if (_owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Use the text buffer on its owner thread.");

        ObjectDisposedException.ThrowIf(_disposed, this);
        Context?.Check();
    }

    public string Text
    {
        get
        {
            Check();
            return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(_data, Length));
        }
        set => Set(value);
    }

    public int Length
    {
        get
        {
            Check();
            var n = 0;
            while (n < Capacity && _data[n] != 0) n++;
            return n;
        }
    }

    public void Set(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        Set(text.AsSpan());
    }

    public void Set(ReadOnlySpan<char> text)
    {
        Check();
        if (text.Contains('\0')) throw new ArgumentException("Text cannot contain NUL.", nameof(text));

        var bytes = Encoding.UTF8.GetByteCount(text);
        if (bytes >= Capacity) throw new ArgumentException("UTF-8 text exceeds buffer capacity.", nameof(text));

        Encoding.UTF8.GetBytes(text, new Span<byte>(_data, bytes));
        _data[bytes] = 0;
    }

    public int CopyUtf8To(Span<byte> destination)
    {
        Check();
        var length = Length;
        if (destination.Length < length) throw new ArgumentException("Destination is too short.", nameof(destination));

        new ReadOnlySpan<byte>(_data, length).CopyTo(destination);
        return length;
    }

    public void SetUtf8(ReadOnlySpan<byte> text)
    {
        Check();
        Utf8Validation.Check(text);
        if (text.Length >= Capacity) throw new ArgumentException("UTF-8 text exceeds buffer capacity.", nameof(text));

        text.CopyTo(new Span<byte>(_data, text.Length));
        _data[text.Length] = 0;
    }

    public void Set(ReadOnlySpan<byte> text) => SetUtf8(text);

    // Public API must not expose spans into disposable unmanaged storage.
    internal ReadOnlySpan<byte> ReadUtf8()
    {
        var length = Length;
        return new ReadOnlySpan<byte>(_data, length);
    }

    internal ReadOnlySpan<byte> AcquireReadUtf8()
    {
        var length = Length;
        _leases++;
        return new ReadOnlySpan<byte>(_data, length + 1);
    }

    internal byte* Pointer => _data;

    internal byte* Acquire(Ui ui)
    {
        Check();
        if (Context != null && !ReferenceEquals(Context, ui)) throw new ArgumentException("Text buffer belongs to another context.");

        Context = ui;
        _leases++;
        return _data;
    }

    internal void Release()
    {
        if (--_leases == 0 && _disposed) Free();
    }

    private void Free()
    {
        NativeMemory.Free(_data);
        _data = null;
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (_owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Dispose the text buffer on its owner thread.");

        if (_disposed) return;

        _disposed = true;
        if (_leases == 0) Free();
    }

    ~TextBuffer() { NativeMemory.Free(_data); }
}

internal sealed unsafe class FrameArena : IDisposable
{
    private struct Block
    {
        internal byte* Pointer;
        internal int Size;
        internal int Used;
    }

    private readonly List<Block> _blocks = new List<Block>();
    private readonly int _limit;
    private int _allocated;
    private int _current;
    internal int ReservedBytes => _allocated;

    internal int UsedBytes
    {
        get
        {
            var used = 0;
            foreach (var block in _blocks) used = checked(used + block.Used);
            return used;
        }
    }

    internal FrameArena(int limit)
    {
        if (limit < 4096) throw new ArgumentOutOfRangeException(nameof(limit));

        _limit = limit;
    }

    internal void* Allocate(int bytes)
    {
        if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes));

        if (bytes == 0) return null;

        var aligned = checked((bytes + 15) & ~15);
        while (_current < _blocks.Count)
        {
            var b = _blocks[_current];
            if (aligned <= b.Size - b.Used)
            {
                var p = b.Pointer + b.Used;
                b.Used += aligned;
                _blocks[_current] = b;
                new Span<byte>(p, aligned).Clear();
                return p;
            }

            _current++;
        }

        var size = Math.Max(65536, aligned);
        if (size > _limit - _allocated) size = aligned;
        if (size > _limit - _allocated) throw new InvalidOperationException("Managed CUI frame arena limit exceeded.");

        var pointer = (byte*)NativeMemory.Alloc((nuint)size);
        if (pointer == null) throw new OutOfMemoryException();

        _blocks.Add(new Block { Pointer = pointer, Size = size, Used = aligned });
        _allocated += size;
        new Span<byte>(pointer, aligned).Clear();
        return pointer;
    }

    internal void Reset()
    {
        for (var i = 0; i < _blocks.Count; i++)
        {
            var b = _blocks[i];
            b.Used = 0;
            _blocks[i] = b;
        }

        _current = 0;
    }

    public void Dispose()
    {
        foreach (var b in _blocks) NativeMemory.Free(b.Pointer);
        _blocks.Clear();
        _allocated = _current = 0;
    }
}

public readonly struct UiScope : IDisposable
{
    private readonly Ui? _ui;
    private readonly long _id;
    private readonly long _generation;
    public bool IsOpen { get; }
    public int Result { get; }

    internal UiScope(Ui ui, long id, long generation, bool open, int result)
    {
        _ui = ui;
        _id = id;
        _generation = generation;
        IsOpen = open;
        Result = result;
    }

    public void Dispose()
    {
        if (IsOpen) _ui?.CloseScope(_id, _generation);
    }

    public static implicit operator bool(UiScope scope) => scope.IsOpen;
}

public readonly struct UiFrame : IDisposable
{
    private readonly Ui? _ui;
    private readonly long _generation;

    internal UiFrame(Ui ui, long generation)
    {
        _ui = ui;
        _generation = generation;
    }

    public void Dispose() => _ui?.FinishFrame(_generation);
}

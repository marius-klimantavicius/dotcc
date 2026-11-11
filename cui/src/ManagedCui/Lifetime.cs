using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using Raw = global::Managed.Cui.Cui;

namespace Cui.Managed;

public enum Font { Body, Medium, Display, Mono, Icons }

/// <summary>A CUI widget identity bound to its creating context.</summary>
public readonly struct Id : IEquatable<Id>
{
    internal readonly Ui? Owner;
    internal readonly uint Value;
    internal Id(Ui owner, uint value) { Owner = owner; Value = value; }
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
    private T value;
    internal Ui? Owner;
    internal IStateLease? Lease;
    public State(T value = default!) { this.value = value; }
    public T Value
    {
        get { Owner?.Check(); return value; }
        set { Owner?.Check(); this.value = value; Lease?.Write(); }
    }
    internal T Current { get => value; set => this.value = value; }
}
internal interface IStateLease : IDisposable { void Read(); void Write(); }
internal sealed unsafe class StateLease<T, R> : IStateLease where R : unmanaged
{
    private readonly Ui ui;
    private readonly State<T> state;
    internal R* Pointer;
    private readonly delegate*<Ui, T, R> encode;
    private readonly delegate*<Ui, R, T, T> decode;
    internal StateLease(Ui ui, State<T> state, delegate*<Ui, T, R> encode, delegate*<Ui, R, T, T> decode)
    {
        this.ui = ui; this.state = state; this.encode = encode; this.decode = decode;
        Pointer = (R*)NativeMemory.AllocZeroed((nuint)sizeof(R));
        if (Pointer is null) throw new OutOfMemoryException();
    }
    public void Write() { if (Pointer != null) *Pointer = encode(ui, state.Current); }
    public void Read() { if (Pointer != null) state.Current = decode(ui, *Pointer, state.Current); }
    public void Dispose() { NativeMemory.Free(Pointer); Pointer = null; }
}

/// <summary>Persistent UTF-8 storage. Capacity includes the trailing NUL byte.</summary>
public sealed unsafe class TextBuffer : IDisposable
{
    private byte* data;
    private readonly int owner = Environment.CurrentManagedThreadId;
    internal Ui? Context;
    private bool disposed;
    private int leases;
    public int Capacity { get; }
    public TextBuffer(int capacity, string text = "")
    {
        if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
        Capacity = capacity;
        data = (byte*)NativeMemory.AllocZeroed((nuint)capacity);
        if (data == null) throw new OutOfMemoryException();
        try { Set(text); } catch { NativeMemory.Free(data); data = null; throw; }
    }
    private void Check()
    {
        if (owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Use the text buffer on its owner thread.");
        ObjectDisposedException.ThrowIf(disposed, this);
        Context?.Check();
    }
    public string Text { get { Check(); return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(data, Length)); } set => Set(value); }
    public int Length { get { Check(); int n = 0; while (n < Capacity && data[n] != 0) n++; return n; } }
    public void Set(string text) { ArgumentNullException.ThrowIfNull(text); Set(text.AsSpan()); }
    public void Set(ReadOnlySpan<char> text)
    {
        Check();
        if (text.Contains('\0')) throw new ArgumentException("Text cannot contain NUL.", nameof(text));
        int bytes = Encoding.UTF8.GetByteCount(text);
        if (bytes >= Capacity) throw new ArgumentException("UTF-8 text exceeds buffer capacity.", nameof(text));
        Encoding.UTF8.GetBytes(text, new Span<byte>(data, bytes)); data[bytes] = 0;
    }
    public int CopyUtf8To(Span<byte> destination)
    {
        Check(); int length = Length;
        if (destination.Length < length) throw new ArgumentException("Destination is too short.", nameof(destination));
        new ReadOnlySpan<byte>(data, length).CopyTo(destination); return length;
    }
    public void SetUtf8(ReadOnlySpan<byte> text)
    {
        Check(); Utf8Validation.Check(text);
        if (text.Length >= Capacity) throw new ArgumentException("UTF-8 text exceeds buffer capacity.", nameof(text));
        text.CopyTo(new Span<byte>(data, text.Length)); data[text.Length] = 0;
    }
    internal byte* Pointer => data;
    internal byte* Acquire(Ui ui)
    {
        Check();
        if (Context != null && !ReferenceEquals(Context, ui)) throw new ArgumentException("Text buffer belongs to another context.");
        Context = ui; leases++; return data;
    }
    internal void Release() { if (--leases == 0 && disposed) Free(); }
    private void Free() { NativeMemory.Free(data); data = null; GC.SuppressFinalize(this); }
    public void Dispose()
    {
        if (owner != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Dispose the text buffer on its owner thread.");
        if (disposed) return; disposed = true;
        if (leases == 0) Free();
    }
    ~TextBuffer() { NativeMemory.Free(data); }
}

internal sealed unsafe class FrameArena : IDisposable
{
    private struct Block { internal byte* Pointer; internal int Size; internal int Used; }
    private readonly List<Block> blocks = new();
    private readonly int limit;
    private int allocated;
    private int current;
    internal int ReservedBytes => allocated;
    internal int UsedBytes
    {
        get { int used = 0; foreach (var block in blocks) used = checked(used + block.Used); return used; }
    }
    internal FrameArena(int limit) { if (limit < 4096) throw new ArgumentOutOfRangeException(nameof(limit)); this.limit = limit; }
    internal void* Allocate(int bytes)
    {
        if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes));
        if (bytes == 0) return null;
        int aligned = checked((bytes + 15) & ~15);
        while (current < blocks.Count)
        {
            var b = blocks[current];
            if (aligned <= b.Size - b.Used)
            {
                byte* p = b.Pointer + b.Used; b.Used += aligned; blocks[current] = b;
                new Span<byte>(p, aligned).Clear(); return p;
            }
            current++;
        }
        int size = Math.Max(65536, aligned);
        if (size > limit - allocated) size = aligned;
        if (size > limit - allocated) throw new InvalidOperationException("Managed CUI frame arena limit exceeded.");
        byte* pointer = (byte*)NativeMemory.Alloc((nuint)size);
        if (pointer == null) throw new OutOfMemoryException();
        blocks.Add(new Block { Pointer = pointer, Size = size, Used = aligned }); allocated += size;
        new Span<byte>(pointer, aligned).Clear(); return pointer;
    }
    internal void Reset()
    {
        for (int i = 0; i < blocks.Count; i++) { var b = blocks[i]; b.Used = 0; blocks[i] = b; }
        current = 0;
    }
    public void Dispose() { foreach (var b in blocks) NativeMemory.Free(b.Pointer); blocks.Clear(); allocated = current = 0; }
}

public readonly struct UiScope : IDisposable
{
    private readonly Ui? ui;
    private readonly long id;
    private readonly long generation;
    public bool IsOpen { get; }
    public int Result { get; }
    internal UiScope(Ui ui, long id, long generation, bool open, int result) { this.ui = ui; this.id = id; this.generation = generation; IsOpen = open; Result = result; }
    public void Dispose() { if (IsOpen) ui?.CloseScope(id, generation); }
    public static implicit operator bool(UiScope scope) => scope.IsOpen;
}
public readonly struct UiFrame : IDisposable
{
    private readonly Ui? ui;
    private readonly long generation;
    internal UiFrame(Ui ui, long generation) { this.ui = ui; this.generation = generation; }
    public void Dispose() => ui?.FinishFrame(generation);
}

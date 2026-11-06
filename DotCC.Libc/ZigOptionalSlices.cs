#nullable enable

namespace DotCC.Libc;

/// <summary>A slice over existing managed Nullable&lt;T&gt; elements. The payload,
/// rather than Nullable&lt;T&gt;, supplies the unmanaged generic argument.</summary>
public struct OptionalSlice<T> where T : unmanaged
{
    private nint _ptr;
    public ulong Len;

    public unsafe OptionalSlice(T?* ptr, ulong len) { _ptr = (nint)ptr; Len = len; }

    public unsafe T?* Ptr
    {
        readonly get => (T?*)_ptr;
        set => _ptr = (nint)value;
    }

    public unsafe ref T? this[ulong index] => ref ((T?*)_ptr)[index];
    public unsafe OptionalSlice<T> Sub(ulong lo, ulong hi) => new((T?*)_ptr + lo, hi - lo);
    public unsafe global::System.Span<T?> AsSpan() => new((void*)_ptr, checked((int)Len));
    public static implicit operator ConstOptionalSlice<T>(OptionalSlice<T> value) => new(value._ptr, value.Len);
}

/// <summary>The const element view over the same managed optional storage.</summary>
public struct ConstOptionalSlice<T> where T : unmanaged
{
    private nint _ptr;
    public ulong Len;

    public unsafe ConstOptionalSlice(T?* ptr, ulong len) { _ptr = (nint)ptr; Len = len; }
    internal ConstOptionalSlice(nint ptr, ulong len) { _ptr = ptr; Len = len; }

    public unsafe T?* Ptr
    {
        readonly get => (T?*)_ptr;
        set => _ptr = (nint)value;
    }

    public unsafe T? this[ulong index] => ((T?*)_ptr)[index];
    public unsafe ConstOptionalSlice<T> Sub(ulong lo, ulong hi) => new((T?*)_ptr + lo, hi - lo);
    public unsafe global::System.ReadOnlySpan<T?> AsSpan() => new((void*)_ptr, checked((int)Len));
}

public static partial class ZigMem
{
    public static unsafe void CopyForwards<T>(OptionalSlice<T> dest, ConstOptionalSlice<T> source) where T : unmanaged
    {
        T?* d = dest.Ptr;
        T?* s = source.Ptr;
        for (ulong i = 0; i < source.Len; i++) d[i] = s[i];
    }

    public static unsafe void CopyForwards<T>(OptionalSlice<T> dest, T?* source) where T : unmanaged
    {
        T?* d = dest.Ptr;
        for (ulong i = 0; i < dest.Len; i++) d[i] = source[i];
    }

    public static unsafe void CopyForwards<T>(T?* dest, ConstOptionalSlice<T> source) where T : unmanaged
    {
        T?* s = source.Ptr;
        for (ulong i = 0; i < source.Len; i++) dest[i] = s[i];
    }

    public static unsafe void Set<T>(OptionalSlice<T> dest, T? value) where T : unmanaged
    {
        T?* d = dest.Ptr;
        for (ulong i = 0; i < dest.Len; i++) d[i] = value;
    }

    public static unsafe OptionalSlice<T> ConstCast<T>(ConstOptionalSlice<T> source) where T : unmanaged
        => new(source.Ptr, source.Len);

    public static unsafe void Move<T>(OptionalSlice<T> dest, ConstOptionalSlice<T> source) where T : unmanaged
    {
        T?* d = dest.Ptr;
        T?* s = source.Ptr;
        if (d <= s)
            for (ulong i = 0; i < source.Len; i++) d[i] = s[i];
        else
            for (ulong i = source.Len; i > 0; i--) d[i - 1] = s[i - 1];
    }

    public static unsafe void Move<T>(OptionalSlice<T> dest, T?* source) where T : unmanaged
        => Move(dest, new ConstOptionalSlice<T>(source, dest.Len));

    public static unsafe void Move<T>(T?* dest, ConstOptionalSlice<T> source) where T : unmanaged
        => Move(new OptionalSlice<T>(dest, source.Len), source);
}

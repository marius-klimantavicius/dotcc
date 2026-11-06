#nullable enable

namespace DotCC.Libc;

/// <summary>A []**T slice: each element is a T**, so its data pointer is T***.</summary>
public struct PtrPtrSlice<T> where T : unmanaged
{
    private nint _ptr;
    public ulong Len;
    public unsafe PtrPtrSlice(T*** ptr, ulong len) { _ptr = (nint)ptr; Len = len; }
    public unsafe T*** Ptr { readonly get => (T***)_ptr; set => _ptr = (nint)value; }
    public unsafe ref T** this[ulong index] => ref ((T***)_ptr)[index];
    public unsafe PtrPtrSlice<T> Sub(ulong lo, ulong hi) => new((T***)_ptr + lo, hi - lo);
    public static implicit operator ConstPtrPtrSlice<T>(PtrPtrSlice<T> value) => new(value._ptr, value.Len);
}

/// <summary>The const element view of a []**T slice, retaining all pointer levels.</summary>
public struct ConstPtrPtrSlice<T> where T : unmanaged
{
    private nint _ptr;
    public ulong Len;
    public unsafe ConstPtrPtrSlice(T*** ptr, ulong len) { _ptr = (nint)ptr; Len = len; }
    internal ConstPtrPtrSlice(nint ptr, ulong len) { _ptr = ptr; Len = len; }
    public unsafe T*** Ptr { readonly get => (T***)_ptr; set => _ptr = (nint)value; }
    public unsafe T** this[ulong index] => ((T***)_ptr)[index];
    public unsafe ConstPtrPtrSlice<T> Sub(ulong lo, ulong hi) => new((T***)_ptr + lo, hi - lo);
}

public static partial class ZigMem
{
    public static unsafe void CopyForwards<T>(PtrPtrSlice<T> dest, ConstPtrPtrSlice<T> source) where T : unmanaged
    {
        T*** d = dest.Ptr;
        T*** s = source.Ptr;
        for (ulong i = 0; i < source.Len; i++) d[i] = s[i];
    }

    public static unsafe void CopyForwards<T>(PtrPtrSlice<T> dest, T*** source) where T : unmanaged
    {
        T*** d = dest.Ptr;
        for (ulong i = 0; i < dest.Len; i++) d[i] = source[i];
    }

    public static unsafe void CopyForwards<T>(T*** dest, ConstPtrPtrSlice<T> source) where T : unmanaged
    {
        T*** s = source.Ptr;
        for (ulong i = 0; i < source.Len; i++) dest[i] = s[i];
    }

    public static unsafe void Set<T>(PtrPtrSlice<T> dest, T** value) where T : unmanaged
    {
        T*** d = dest.Ptr;
        for (ulong i = 0; i < dest.Len; i++) d[i] = value;
    }

    public static unsafe void Move<T>(PtrPtrSlice<T> dest, ConstPtrPtrSlice<T> source) where T : unmanaged
    {
        T*** d = dest.Ptr;
        T*** s = source.Ptr;
        if (d <= s)
            for (ulong i = 0; i < source.Len; i++) d[i] = s[i];
        else
            for (ulong i = source.Len; i > 0; i--) d[i - 1] = s[i - 1];
    }
}

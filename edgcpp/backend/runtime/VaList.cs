using System;
using System.Runtime.InteropServices;

namespace Edg.Runtime;

// Managed internal varargs ABI, independent of native register save areas.
// The size also accommodates EDG's current builtin va_list tag. The variadic
// callee owns storage through VaPack; forwarded lists and va_copy cursors borrow
// that activation's lifetime.
[StructLayout(LayoutKind.Sequential, Size = 24)]
public unsafe struct VaList
{
    public byte* Next;
    public byte* End;
    public byte* Begin;

    public static void Start(VaList* destination, VaList* arguments) => *destination = *arguments;
    public static void Copy(VaList* destination, VaList* source) => *destination = *source;
    public static void EndList(VaList* list) => *list = default;

    public static void* Take(VaList* list, ulong size, ulong alignment)
    {
        byte* address = (byte*)(((nuint)list->Next + (nuint)alignment - 1) & ~((nuint)alignment - 1));
        if (list->Begin == null || address < list->Begin || address > list->End ||
            size > (ulong)(list->End - address))
            throw new InvalidOperationException("va_arg exceeds the translated argument pack");
        list->Next = address + size;
        return address;
    }
}

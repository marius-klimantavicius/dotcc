#nullable enable
using System;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // These C APIs receive no allocation capacity. Validate only representable
    // address ranges; checking that the caller owns n accessible bytes remains
    // its obligation, just as in native C. Never truncate size_t to int.
    private static nuint ValidateMemoryRange(void* pointer, ulong length)
    {
        nuint count = checked((nuint)length);
        if (count > (nuint)nint.MaxValue) throw new ArgumentOutOfRangeException(nameof(length), "Memory extent exceeds the native signed address range.");
        if (count != 0)
        {
            if (pointer == null) throw new ArgumentNullException(nameof(pointer));
            if (count > nuint.MaxValue - (nuint)pointer)
                throw new ArgumentOutOfRangeException(nameof(length), "Memory address range wraps.");
        }
        return count;
    }

    public static void* memset(void* dst, int value, ulong count)
    {
        nuint length = ValidateMemoryRange(dst, count);
        if (length != 0) NativeMemory.Fill(dst, length, (byte)value);
        return dst;
    }
    public static void* memcpy(void* dst, void* src, ulong count)
    {
        ValidateMemoryRange(dst, count); ValidateMemoryRange(src, count);
        if (count != 0) Buffer.MemoryCopy(src, dst, count, count);
        return dst;
    }
    public static void* memmove(void* dst, void* src, ulong count)
    {
        ValidateMemoryRange(dst, count); ValidateMemoryRange(src, count);
        // BCL MemoryCopy guarantees the source is preserved when ranges overlap.
        if (count != 0) Buffer.MemoryCopy(src, dst, count, count);
        return dst;
    }
    public static int memcmp(void* a, void* b, ulong count)
    {
        nuint length = ValidateMemoryRange(a, count); ValidateMemoryRange(b, count);
        byte* left = (byte*)a; byte* right = (byte*)b;
        for (nuint i = 0; i < length; i++)
            if (left[i] != right[i]) return left[i] - right[i];
        return 0;
    }
    public static void* memchr(void* s, int value, ulong count)
    {
        nuint length = ValidateMemoryRange(s, count);
        byte* data = (byte*)s; byte needle = (byte)value;
        for (nuint i = 0; i < length; i++)
            if (data[i] == needle) return data + i;
        return null;
    }
}

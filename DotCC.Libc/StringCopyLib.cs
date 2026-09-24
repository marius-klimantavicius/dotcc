#nullable enable

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Copy through the NUL byte and return its address in destination.</summary>
    public static byte* stpcpy(byte* destination, byte* source)
    {
        while ((*destination = *source) != 0) { destination++; source++; }
        return destination;
    }

    /// <summary>Allocate an independent, NUL-terminated copy, released by free.
    /// Allocation failure returns NULL and sets ENOMEM; success preserves errno.</summary>
    public static byte* strdup(byte* source) => DuplicateStringBytes(source, strnlen(source, ulong.MaxValue));

    /// <summary>Read at most maximum source bytes, append NUL, and return a
    /// freeable independent copy. A large bound does not imply a large allocation.</summary>
    public static byte* strndup(byte* source, ulong maximum) => DuplicateStringBytes(source, strnlen(source, maximum));

    internal static byte* DuplicateStringBytes(byte* source, ulong length)
    {
        // Reserve the terminator before narrowing to the native allocation size.
        if (length >= (ulong)nuint.MaxValue) { errno = ENOMEM; return null; }
        byte* result = (byte*)AllocateHeap((nuint)length + 1);
        if (result == null) { errno = ENOMEM; return null; }
        if (length != 0) memcpy(result, source, length);
        result[length] = 0;
        return result;
    }
}

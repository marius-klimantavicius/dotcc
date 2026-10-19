#nullable enable
using System;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Erase exactly count bytes, including buffers never read again.
    /// The caller owns writable unmanaged storage (or a pin) for the whole call.
    /// A null pointer is accepted for zero bytes. Success preserves errno.</summary>
    public static void explicit_bzero(void* destination, ulong count)
    {
        nuint remaining = ValidateMemoryRange(destination, count);
        byte* cursor = (byte*)destination;
        while (remaining != 0)
        {
            // Span lengths are signed int; C size_t is LP64. Validate the full
            // range before storing, then advance without narrowing the extent.
            int length = remaining > int.MaxValue ? int.MaxValue : (int)remaining;
            // ZeroMemory's documented contract retains writes even if no read
            // follows. Plain memset/Span.Clear alone does not give that contract.
            global::System.Security.Cryptography.CryptographicOperations.ZeroMemory(new Span<byte>(cursor, length));
            cursor += length;
            remaining -= (nuint)length;
        }
    }
}

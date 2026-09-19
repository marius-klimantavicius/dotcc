#nullable enable
using System;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Secure host entropy. Flags select Linux-compatible behavior;
    /// the BCL always supplies a fully initialized cryptographic generator.</summary>
    public static long getrandom(void* buffer, ulong length, uint flags)
    {
        if ((flags & ~7U) != 0 || length > long.MaxValue) { errno = EINVAL; return -1; }
        if (buffer == null && length != 0) { errno = EFAULT; return -1; }
        ulong done = 0;
        try
        {
            while (done < length)
            {
                int chunk = (int)Math.Min(length - done, 65536UL);
                global::System.Security.Cryptography.RandomNumberGenerator.Fill(new Span<byte>((byte*)buffer + done, chunk));
                done += (uint)chunk;
            }
            return (long)done;
        }
        catch (global::System.Security.Cryptography.CryptographicException)
        {
            errno = EIO;
            return done == 0 ? -1 : (long)done;
        }
    }

    public static int getentropy(void* buffer, ulong length)
    {
        if (length > 256) { errno = EIO; return -1; }
        return getrandom(buffer, length, 0) == (long)length ? 0 : -1;
    }

    /// <summary>The void-returning contract cannot return incomplete entropy.
    /// Propagate failure to the managed owner rather than returning stale bytes.</summary>
    public static void arc4random_buf(void* buffer, ulong length)
    {
        if (getrandom(buffer, length, 0) != (long)length)
            throw new global::System.Security.Cryptography.CryptographicException("Unable to obtain requested entropy");
    }
}

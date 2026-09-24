#nullable enable
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>BCL host name encoded as UTF-8. Linux-shaped truncation returns
    /// ENAMETOOLONG and does not append a NUL when the name fills the buffer.</summary>
    public static int gethostname(byte* name, ulong length)
    {
        if (name == null) { errno = EFAULT; return -1; }
        try
        {
            byte[] value = Encoding.UTF8.GetBytes(Dns.GetHostName());
            int count = (int)Math.Min(length, (ulong)value.Length);
            value.AsSpan(0, count).CopyTo(new Span<byte>(name, count));
            if (length <= (ulong)value.Length) { errno = ENAMETOOLONG; return -1; }
            name[value.Length] = 0;
            return 0;
        }
        catch (SocketException) { errno = EIO; return -1; }
        catch (OutOfMemoryException) { errno = ENOMEM; return -1; }
    }

    /// <summary>BCL compatibility mapping: returns Environment.UserName, not
    /// the terminal's utmp login identity. Returns an error number directly and
    /// leaves errno alone. Callers needing terminal-session identity must not
    /// use this mapping as an authentication decision.</summary>
    public static int getlogin_r(byte* name, ulong length)
    {
        if (name == null) return EFAULT;
        try
        {
            string username = Environment.UserName;
            if (username.Length == 0) return ENOENT;
            byte[] value = Encoding.UTF8.GetBytes(username);
            if (length <= (ulong)value.Length) return ERANGE;
            value.AsSpan().CopyTo(new Span<byte>(name, value.Length));
            name[value.Length] = 0;
            return 0;
        }
        catch (OutOfMemoryException) { return ENOMEM; }
        catch (global::System.Security.SecurityException) { return EACCES; }
        catch (InvalidOperationException) { return EIO; }
    }

    private static readonly object RandomGate = new();
    private static Random PosixRandom = new(1);

    /// <summary>Owner-scoped 31-bit noncryptographic PRNG, process-wide when unbound. Seed repeatability
    /// and range match random(); sequences are not glibc-compatible.</summary>
    public static long random()
    {
        if (OwnedRandom is { } state)
        {
            lock (state.Gate) return state.Posix.NextInt64(0, 1L << 31);
        }
        lock (RandomGate) return PosixRandom.NextInt64(0, 1L << 31);
    }

    public static void srandom(uint seed)
    {
        if (OwnedRandom is { } state)
        {
            lock (state.Gate) state.Posix = new Random(unchecked((int)seed));
            return;
        }
        lock (RandomGate) PosixRandom = new Random(unchecked((int)seed));
    }
}

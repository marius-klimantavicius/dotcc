#nullable enable
using System;

namespace Edg.Runtime;

public sealed unsafe partial class PosixContext
{
    // Only the actual promoted C argument is consumed. No callee-side heap pack
    // or generated per-call helper is needed at this fixed POSIX boundary.
    private static T Variadic<T>(ReadOnlySpan<VaArg> values) where T : unmanaged
    {
        if (values.Length == 0 || values[0].Size != sizeof(T))
            throw new ArgumentException("POSIX variadic argument has the wrong promoted type");
        T result = default;
        values[0].CopyTo((byte*)&result, sizeof(T));
        return result;
    }

    public int OpenVariadic(sbyte* path, int flags, ReadOnlySpan<VaArg> arguments)
    {
        try { return Open(path, flags, (flags & 0x40) != 0 ? Variadic<int>(arguments) : 0); }
        catch (ArgumentException error) { return Fail(error); }
    }

    public int OpenAtVariadic(int directory, sbyte* path, int flags, ReadOnlySpan<VaArg> arguments)
    {
        try { return OpenAt(directory, path, flags, (flags & 0x40) != 0 ? Variadic<int>(arguments) : 0); }
        catch (ArgumentException error) { return Fail(error); }
    }

    public int FcntlVariadic(int fd, int command, ReadOnlySpan<VaArg> arguments)
    {
        try
        {
            return command switch
            {
                1 or 3 => FcntlFlags(fd, command),
                2 or 4 => FcntlFlags(fd, command, Variadic<int>(arguments)),
                5 or 6 or 7 => Fcntl(fd, command, (PosixFlock*)Variadic<nuint>(arguments)),
                _ => UnsupportedCommand(),
            };
        }
        catch (ArgumentException error) { return Fail(error); }
    }

    private int UnsupportedCommand() { SetError(22); return -1; }
}

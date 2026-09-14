using global::System;
using global::System.Text;
using Managed.Emulation.Host;

namespace Managed.Emulation;

public static partial class Blink
{
    public static unsafe int blink_host_access(byte* path, int mode)
        => blink_host_faccessat(-100, path, mode, 0);
    public static unsafe int blink_host_faccessat(int directory, byte* path, int mode, int flags)
    {
        try
        {
            if (io == null) return IoError(19);
            if (path == null) return IoError(14);
            int length = 0;
            while (length < PathLimit && path[length] != 0) ++length;
            if (length == PathLimit) return IoError(36);
            string name;
            try { name = PathEncoding.GetString(new ReadOnlySpan<byte>(path, length)); }
            catch (DecoderFallbackException) { return IoError(22); }
            return (int)IoResult(io.AccessAt(directory, name, mode, flags));
        }
        catch (Exception error) { return IoException(error); }
    }
}

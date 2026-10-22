#nullable enable
using System;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public sealed partial class RuntimeContext
    {
        internal int GetoptIndex = 1;
    }

    /// <summary>POSIX option-parser cursor. This is process state, shared by
    /// threads bound to the same owner, and initially one for each owner.
    /// This storage does not implement getopt itself.</summary>
    public static ref int optind => ref RuntimeState.GetoptIndex;

    /// <summary>The actual host memory page size, in bytes, matching
    /// sysconf(_SC_PAGESIZE). Successful queries leave errno unchanged.</summary>
    public static int getpagesize() => Environment.SystemPageSize;

    /// <summary>No host process transport is installed in the shared runtime.
    /// Reject process creation explicitly. Embedders such as PGlite must use
    /// their reviewed in-process callbacks instead of a native shell/backend.</summary>
    public static FILE* popen(byte* command, byte* mode)
    {
        if (command == null || mode == null) { errno = EFAULT; return null; }
        // The Linux-shaped surface also accepts the close-on-exec suffix.
        if ((*mode != (byte)'r' && *mode != (byte)'w') ||
            (mode[1] != 0 && (mode[1] != (byte)'e' || mode[2] != 0)))
        { errno = EINVAL; return null; }
        errno = ENOTSUP;
        return null;
    }

    /// <summary>No stream has an associated child process here. Validate the
    /// FILE ownership without dereferencing a foreign handle, then report that
    /// there is no child to wait for. Never close an ordinary caller's stream.</summary>
    public static int pclose(FILE* stream)
    {
        errno = Slot(stream) is null ? EBADF : ECHILD;
        return -1;
    }
}

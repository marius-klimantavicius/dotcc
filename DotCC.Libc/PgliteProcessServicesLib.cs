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

    /// <summary>The exact read-only locale inventory command uses the reviewed
    /// host utility. Other commands require an embedder's in-process callbacks;
    /// this surface never starts a shell or a native database backend.</summary>
    public static FILE* popen(byte* command, byte* mode)
    {
        if (command == null || mode == null) { errno = EFAULT; return null; }
        // The Linux-shaped surface also accepts the close-on-exec suffix.
        if ((*mode != (byte)'r' && *mode != (byte)'w') ||
            (mode[1] != 0 && (mode[1] != (byte)'e' || mode[2] != 0)))
        { errno = EINVAL; return null; }
        if (*mode == (byte)'r' && LocaleInventoryCommand(command))
            return OpenLocaleInventory(mode);
        errno = ENOTSUP;
        return null;
    }

    /// <summary>Close an owner-local locale inventory stream and return its
    /// actual POSIX wait status. Never close an ordinary caller's stream.</summary>
    public static int pclose(FILE* stream)
    {
        if (Slot(stream) is null) { errno = EBADF; return -1; }
        return CloseLocaleInventory(stream);
    }
}

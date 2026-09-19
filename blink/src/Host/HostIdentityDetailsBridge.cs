using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    private static int IdentityDetailError(int error) { Libc.errno = error; return -1; }
    public static long blink_host_sysconf(int selector)
    {
        if (identity == null) return IdentityError();
        return selector switch
        {
            2 => 100, // Virtual process-accounting ticks per second.
            3 => 0, // This immutable identity has no supplementary groups.
            30 => 4096, // Actual authored host mapping page size.
            _ => IdentityDetailError(22)
        };
    }
    public static unsafe int blink_host_getgroups(int count, uint* groups)
    {
        if (identity == null) return IdentityError();
        if (count < 0) return IdentityDetailError(22);
        // No output elements exist; even a size-zero null query is valid.
        return 0;
    }
    private static unsafe int IdentityIds(uint* real, uint* effective, uint* saved, bool group)
    {
        if (identity == null) return IdentityError();
        if (real == null || effective == null || saved == null) return IdentityDetailError(14);
        uint value = group ? identity.GroupId : identity.UserId;
        *real = value; *effective = value; *saved = value;
        return 0;
    }
    public static unsafe int blink_host_getresuid(uint* real, uint* effective, uint* saved)
        => IdentityIds(real, effective, saved, false);
    public static unsafe int blink_host_getresgid(uint* real, uint* effective, uint* saved)
        => IdentityIds(real, effective, saved, true);
    public static int blink_host_getpgid(int process)
    {
        if (identity == null) return IdentityError();
        return process == 0 || process == identity.ProcessId ? identity.ProcessId : IdentityDetailError(3);
    }
    public static int blink_host_getsid(int process) => blink_host_getpgid(process);
    public static unsafe int blink_host_gethostname(byte* name, ulong length)
    {
        if (identity == null) return IdentityError();
        if (name == null) return IdentityDetailError(14);
        string text = identity.HostName;
        if (length <= (ulong)text.Length) return IdentityDetailError(36);
        // Constructor restricts the owned name to 63 ASCII bytes.
        for (int i = 0; i < text.Length; ++i) name[i] = (byte)text[i];
        name[text.Length] = 0;
        return 0;
    }
}

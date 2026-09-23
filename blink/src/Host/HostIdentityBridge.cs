using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    [ThreadStatic] private static HostIdentity? identity;
    public static void BindHostIdentity(HostIdentity value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (identity != null) throw new InvalidOperationException("Identity already bound on this worker.");
        identity = value;
    }
    public static void UnbindHostIdentity() => identity = null;
    private static int IdentityError() { Libc.errno = 19; return -1; }
    public static int blink_host_getpid() => identity?.ProcessId ?? IdentityError();
    public static int blink_host_getppid() => identity?.ParentProcessId ?? IdentityError();
    public static uint blink_host_getuid() => identity?.UserId ?? unchecked((uint)IdentityError());
    public static uint blink_host_geteuid() => blink_host_getuid();
    public static uint blink_host_getgid() => identity?.GroupId ?? unchecked((uint)IdentityError());
    public static uint blink_host_getegid() => blink_host_getgid();
}

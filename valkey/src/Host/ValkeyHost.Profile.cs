using CoreLibc = Managed.Database.ValkeyCore.Libc;

namespace Managed.Database;

public static unsafe partial class ValkeyHost
{
    // The managed host owns descriptor capacity and worker lifetime. Upstream
    // process-wide signal, cancellation and watchdog setup has no host role.
    public static void ManagedProcessSetup(ValkeyCore core) { }

    // POSIX pthread functions return the error number directly.
    public static int PthreadSigmask(ValkeyCore core, int how, ulong* set, ulong* previous)
        => CoreLibc.ENOTSUP;
}

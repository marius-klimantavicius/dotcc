using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;
#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    [ThreadStatic] private static HostSingleThreadMemoryBarrier? membarrierOwner;

    public static void BindHostMembarrier(HostSingleThreadMemoryBarrier owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (!owner.IsOwnerThread) throw new InvalidOperationException("Memory barrier owner belongs to another thread.");
        if (membarrierOwner != null) throw new InvalidOperationException("Memory barrier owner already bound.");
        membarrierOwner = owner;
    }

    public static void UnbindHostMembarrier() => membarrierOwner = null;

    public static int blink_host_membarrier(int command, uint flags, int cpuId)
    {
        if (membarrierOwner == null) { Libc.errno = 19; return -1; } // ENODEV
        var result = membarrierOwner.Invoke(command, flags, cpuId);
        if (result.Succeeded) return result.Value;
        Libc.errno = (int)result.Error;
        return -1;
    }
}

using global::System;
using global::System.Threading;
namespace Managed.Emulation;
public static partial class Blink
{
    public static int blink_host_sched_yield()
    {
        if(environment==null)return EnvironmentError(19);
        // Yield is a scheduling hint: false means no other eligible thread ran,
        // not failure of the POSIX operation.
        Thread.Yield();
        return 0;
    }
}

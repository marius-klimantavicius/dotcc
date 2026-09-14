using global::System.Diagnostics.CodeAnalysis;
using Managed.Emulation.Host;

namespace Managed.Emulation;

public static partial class Blink
{
    [DoesNotReturn]
    public static void blink_host_exit(int status) => throw new HostTerminationException(HostTerminationKind.Exit, status);
    [DoesNotReturn]
    public static void blink_host_immediate_exit(int status) => throw new HostTerminationException(HostTerminationKind.ImmediateExit, status);
    [DoesNotReturn]
    public static void blink_host_abort() => throw new HostTerminationException(HostTerminationKind.Abort, 0);
}

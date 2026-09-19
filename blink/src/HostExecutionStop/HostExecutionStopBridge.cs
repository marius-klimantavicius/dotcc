using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;
#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    [ThreadStatic] private static HostExecutionStop? executionStopOwner;
    public static void BindHostExecutionStop(HostExecutionStop owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (executionStopOwner != null) throw new InvalidOperationException("Execution stop owner already bound.");
        executionStopOwner = owner;
    }
    public static void UnbindHostExecutionStop() => executionStopOwner = null;
    // Unbound historical consumers retain their prior behavior.
    public static int blink_host_execution_stop_reason() => (int)(executionStopOwner?.Reason ?? HostExecutionStopReason.None);
}

namespace Managed.Emulation;
#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    public static unsafe int BlinkHostInitializeBoundResourceLimits(System* system)
    {
        if (io == null) { Libc.errno = 19; return -1; }
        var capacity = io.DescriptorCapacity();
        if (!capacity.Succeeded) { Libc.errno = (int)capacity.Error; return -1; }
        return BlinkHostInitializeResourceLimits(system, (ulong)capacity.Value);
    }
}

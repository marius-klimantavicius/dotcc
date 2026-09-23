namespace Managed.Emulation;
#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public partial class Blink
#endif
{
#if DOTCC_INSTANCE_FOR_HOST
    public unsafe int BlinkHostInitializeBoundResourceLimits(System* system)
#else
    public static unsafe int BlinkHostInitializeBoundResourceLimits(System* system)
#endif
    {
        if (io == null) { Libc.errno = 19; return -1; }
        var capacity = io.DescriptorCapacity();
        if (!capacity.Succeeded) { Libc.errno = (int)capacity.Error; return -1; }
        return BlinkHostInitializeResourceLimits(system, (ulong)capacity.Value);
    }
}

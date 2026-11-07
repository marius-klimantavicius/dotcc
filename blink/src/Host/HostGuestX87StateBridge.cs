using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    private static IHostGuestX87State? GuestX87
    {
        get
        {
#if BLINK_GUEST_THREAD_COPIES
            return guestThreads as IHostGuestX87State;
#else
            return null;
#endif
        }
    }
    // Source adapter passes ONLY private C stack images, not guest addresses.
    // BLINK_GUEST_THREAD_COPIES excludes owner/TLS references from base builds.
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe int blink_host_guest_x87_active(BlinkCore program, Machine* machine, uint protocol)
#else
    public static unsafe int blink_host_guest_x87_active(Machine* machine, uint protocol)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST && BLINK_GUEST_THREAD_COPIES
        _ = GuestOwner(program);
#endif
        if (protocol != 1) throw new InvalidOperationException("x87 producer protocol mismatch");
        var owner = GuestX87;
        return owner != null && owner.GetX87RegisterFile((nint)machine, protocol) != 0 ? 1 : 0;
    }
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe int blink_host_guest_x87_state(BlinkCore program, Machine* machine,
        uint protocol, int operation, void* image, uint bytes)
#else
    public static unsafe int blink_host_guest_x87_state(Machine* machine,
        uint protocol, int operation, void* image, uint bytes)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST && BLINK_GUEST_THREAD_COPIES
        _ = GuestOwner(program);
#endif
        if (protocol != HostX87StateProtocol.Version || bytes > 512 ||
            HostX87StateProtocol.ImageSize((HostX87Transfer)operation) != bytes || (bytes != 0 && image == null))
            throw new InvalidOperationException("x87 producer host image protocol mismatch");
        var owner = GuestX87;
        if (owner == null) return 0;
        bool active = owner.GetX87RegisterFile((nint)machine, protocol) != 0;
        var result = owner.TransferX87State((nint)machine, protocol, (HostX87Transfer)operation,
            new Span<byte>(image, checked((int)bytes)));
        if (result is not (HostX87TransferResult.Inactive or HostX87TransferResult.Complete or HostX87TransferResult.BadImage))
            throw new InvalidOperationException("x87 producer result mismatch");
        if ((result == HostX87TransferResult.Inactive) == active)
            throw new InvalidOperationException("x87 transfer disagrees with active binding");
        if (result == HostX87TransferResult.BadImage && (HostX87Transfer)operation is not
            (HostX87Transfer.LinuxValidate or HostX87Transfer.LinuxRestore or HostX87Transfer.Fx64Restore or
             HostX87Transfer.FxLong32Restore or HostX87Transfer.FxLegacyRestore))
            throw new InvalidOperationException("x87 non-restore returned BadImage");
        return (int)result;
    }
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_x87_reject_legacy(BlinkCore program, Machine* machine, uint protocol)
#else
    public static unsafe void blink_host_guest_x87_reject_legacy(Machine* machine, uint protocol)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST && BLINK_GUEST_THREAD_COPIES
        _ = GuestOwner(program);
#endif
        if (protocol != 1) throw new InvalidOperationException("x87 legacy protocol mismatch");
        var owner = GuestX87;
        owner?.RejectLegacyX87State((nint)machine, protocol);
        if (owner != null && owner.GetX87RegisterFile((nint)machine, protocol) != 0)
            throw new InvalidOperationException("Active true80 state cannot enter a legacy f64/MMX helper");
    }
}

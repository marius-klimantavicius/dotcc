using Managed.Emulation.Host;

namespace Managed.Emulation;

// Selected only by the threaded, non-linear-memory override profile. Page
// allocation/free remains upstream; these replace its unsynchronized index.
public partial class BlinkCore
{
#if DOTCC_INSTANCE_FOR_HOST
    private readonly HostPageTable hostPageTable = new();
    public static unsafe ulong blink_host_track_page(BlinkCore program, byte* page)
        => program.hostPageTable.Track((nint)page);
    public static unsafe byte* blink_host_find_page(BlinkCore program, ulong entry)
        => (byte*)program.hostPageTable.Find(entry);
#else
    private static readonly HostPageTable hostPageTable = new();
    public static unsafe ulong blink_host_track_page(byte* page) => hostPageTable.Track((nint)page);
    public static unsafe byte* blink_host_find_page(ulong entry) => (byte*)hostPageTable.Find(entry);
#endif
}

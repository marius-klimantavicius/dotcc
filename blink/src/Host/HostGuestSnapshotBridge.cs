using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;

// Only the full/threaded producer uses tagged freelist allocations. The base
// host manifest also includes this file, where it deliberately has no symbols.
#if BLINK_GUEST_THREAD_COPIES
public partial class BlinkCore
{
    private const ulong SnapshotHeader = 16, SnapshotAllowance = 32;

    private static unsafe byte* AllocateGuestSnapshot(IHostGuestMemoryCopies? owner, ulong count)
    {
        if (count > int.MaxValue - SnapshotHeader ||
            owner != null && !owner.TryReserveGuestCopyBytes(count + SnapshotAllowance))
        { Libc.errno = 12; return null; }
        byte* raw = null;
        try
        {
            raw = (byte*)Libc.malloc(count + SnapshotHeader);
            if (raw == null) { Libc.errno = 12; return null; }
            ((ulong*)raw)[0] = count;
            ((ulong*)raw)[1] = owner == null ? 0 : count + SnapshotAllowance;
            return raw;
        }
        finally { if (raw == null) owner?.ReleaseGuestCopyBytes(count + SnapshotAllowance); }
    }

    private static unsafe void FreeGuestSnapshot(IHostGuestMemoryCopies? owner, void* raw)
    {
        ulong count = ((ulong*)raw)[1];
        // A failed free retains its charge. Never advertise quota for memory
        // whose deallocation did not finish (e.g. debug-heap invariant failure).
        Libc.free(raw);
        if (count != 0) owner?.ReleaseGuestCopyBytes(count);
    }

#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void* blink_host_guest_freelist_append(BlinkCore program, Machine* machine, void* memory)
#else
    public static unsafe void* blink_host_guest_freelist_append(Machine* machine, void* memory)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        var owner = GuestCopies;
        int count = machine->freelist.n;
        byte* old = machine->freelist.p == null ? null : (byte*)machine->freelist.p - SnapshotHeader;
        ulong capacity = old == null ? 0 : ((ulong*)old)[0] / (ulong)sizeof(nint);
        if (count == int.MaxValue) { Libc.free(memory); Libc.errno = 12; return null; }
        if ((ulong)count == capacity)
        {
            // Charge the actual vector allocation as well as snapshot payloads.
            // Retained capacity stays charged until replacement/final free,
            // including partial/nested CollectGarbage(mark) and an empty list.
            ulong bytes = checked(((ulong)count + 1) * (ulong)sizeof(nint));
            byte* replacement = AllocateGuestSnapshot(owner, bytes);
            if (replacement == null) { Libc.free(memory); return null; }
            bool published = false;
            try
            {
                if (count != 0)
                    new ReadOnlySpan<nint>(machine->freelist.p, count).CopyTo(
                        new Span<nint>(replacement + SnapshotHeader, count + 1));
                machine->freelist.p = (void**)(replacement + SnapshotHeader);
                published = true;
                if (old != null) FreeGuestSnapshot(owner, old);
            }
            finally { if (!published) FreeGuestSnapshot(owner, replacement); }
        }
        machine->freelist.p[count] = memory;
        machine->freelist.n = count + 1;
        return memory;
    }

#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_freelist_free(BlinkCore program, Machine* machine)
#else
    public static unsafe void blink_host_guest_freelist_free(Machine* machine)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        if (machine->freelist.n != 0) throw new InvalidOperationException("Freelist vector is still occupied.");
        if (machine->freelist.p == null) return;
        byte* raw = (byte*)machine->freelist.p - SnapshotHeader;
        machine->freelist.p = null;
        FreeGuestSnapshot(GuestCopies, raw);
    }

#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void* blink_host_guest_snapshot_alloc(BlinkCore program, Machine* machine, ulong count)
#else
    public static unsafe void* blink_host_guest_snapshot_alloc(Machine* machine, ulong count)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        var owner = GuestCopies;
        byte* raw = AllocateGuestSnapshot(owner, count);
        if (raw == null) return null;
        bool published = false, alreadyFreed = false;
        try
        {
#if DOTCC_INSTANCE_FOR_HOST
            void* added = program.AddToFreeList(machine, raw);
#else
            void* added = AddToFreeList(machine, raw);
#endif
            if (added == null)
            {
                // Selected AddToFreeList frees its input on allocation failure.
                alreadyFreed = true; Libc.errno = 12;
                return null;
            }
            machine->freelist.p[machine->freelist.n - 1] = (void*)((nuint)raw | 1);
            published = true;
            return raw + SnapshotHeader;
        }
        finally
        {
            if (!published)
            {
                if (alreadyFreed) owner?.ReleaseGuestCopyBytes(count + SnapshotAllowance);
                else FreeGuestSnapshot(owner, raw);
            }
        }
    }

#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void* blink_host_guest_snapshot_resize(BlinkCore program, Machine* machine, void* copy, ulong count, int slot)
#else
    public static unsafe void* blink_host_guest_snapshot_resize(Machine* machine, void* copy, ulong count, int slot)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        byte* old = (byte*)copy - SnapshotHeader;
        if ((uint)slot >= (uint)machine->freelist.n ||
            machine->freelist.p[slot] != (void*)((nuint)old | 1) || ((ulong*)old)[0] > count)
            throw new InvalidOperationException("Snapshot resize does not own its freelist slot.");
        var owner = GuestCopies;
        // Full replacement reservation also covers the temporary overlap of
        // old and new allocations. Allocation failure leaves the old slot live.
        byte* replacement = AllocateGuestSnapshot(owner, count);
        if (replacement == null) return null;
        bool published = false;
        try
        {
            new ReadOnlySpan<byte>(copy, checked((int)((ulong*)old)[0])).CopyTo(
                new Span<byte>(replacement + SnapshotHeader, checked((int)count)));
            machine->freelist.p[slot] = (void*)((nuint)replacement | 1);
            published = true;
            FreeGuestSnapshot(owner, old);
            return replacement + SnapshotHeader;
        }
        finally { if (!published) FreeGuestSnapshot(owner, replacement); }
    }

#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_snapshot_free(BlinkCore program, Machine* machine, void* raw)
#else
    public static unsafe void blink_host_guest_snapshot_free(Machine* machine, void* raw)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
#endif
        FreeGuestSnapshot(GuestCopies, raw);
    }
}
#endif

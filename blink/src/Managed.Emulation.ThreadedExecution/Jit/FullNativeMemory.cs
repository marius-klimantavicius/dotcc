using System.Runtime.InteropServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

[Flags]
internal enum FullMemoryAccess : uint { Read = 1, Write = 2, Atomic = 4 }
[Flags]
internal enum FullMemoryFlags : uint { None = 0, WriteBarrier = 1, BarrierTicket = 2, BarrierConsumed = 4 }
internal enum FullMemoryAssistResult { Resolved, Retry, DemandPage, GuestFault, WriteBarrier, UnsupportedBacking }

// Worker-owned and allocated once. Entries contain no managed references. An
// epoch is CONTENT/MAPPING authority, not physical reclamation protection: the
// caller ALSO holds FullMemoryLifetime plus native store epoch through RET.
// Publish only outside native execution on this worker, under Authority.Gate.
[StructLayout(LayoutKind.Explicit, Size = FullMemoryTlbEntry.ByteSize)]
internal unsafe struct FullMemoryTlbEntry
{
    internal const int ByteSize = 64;
    [FieldOffset(0)] public ulong Page;
    [FieldOffset(8)] public byte* Host;
    [FieldOffset(16)] public ulong AuthorityEpoch;
    [FieldOffset(24)] public ulong Context;
    [FieldOffset(32)] public FullMemoryAccess Permissions;
    [FieldOffset(36)] public FullMemoryFlags Flags;
    [FieldOffset(40)] public ulong Identity;
    [FieldOffset(48)] public uint ValidStart;
    [FieldOffset(52)] public uint ValidEnd;
}

// Entries are nonowning hints. Native checks the STABLE owner generation word
// before even reading an entry's Header pointer. That word and these arrays
// must outlive ALL retained arenas. Header dereferences require native epoch.
[StructLayout(LayoutKind.Explicit, Size = FullNativeJumpEntry.ByteSize)]
internal unsafe struct FullNativeJumpEntry
{
    internal const int ByteSize = 64;
    [FieldOffset(0)] public ulong GuestPc;
    [FieldOffset(8)] public ulong Context;
    [FieldOffset(16)] public ulong StoreGeneration;
    [FieldOffset(24)] public ulong OwnerCookie;
    [FieldOffset(32)] public ulong BodyGeneration;
    [FieldOffset(40)] public TranslationBlockHeader* Header;
}

internal static unsafe partial class FullNativeMemory
{
    // Four-way larger bounded worker table targets measured conflict misses.
    // Slot/tag/authentication semantics and maximum4096 remain unchanged.
    internal const int TlbCount = 256;
    internal const int JumpCount = 256;
    internal const int MaximumTlbCount = 4096;
    // Dense architectural records (x87 m80/environment, FX state) share the
    // same bounded two-page mapping protocol as scalar/vector operands.
    internal const uint MaximumDenseMemoryBytes = 512;
    internal static bool ValidMemoryWidth(uint bits) => bits >= 8 &&
        bits <= MaximumDenseMemoryBytes * 8 && (bits & 7) == 0;
    internal static bool ValidTlbMask(uint mask) => mask >= 1 && mask < MaximumTlbCount && (mask & (mask + 1)) == 0;
    // Precharged for EVERY configured worker by FullTranslationJit, before any
    // allocation. The old owner allowance of16 covers only the old native-state
    // object. This separate allowance covers the binding plus new Worker fields.
    internal const long X87BindingAndWorkerAllowance = 128;
    internal static long WorkerNativeStorageBytesFor(int tlbCount) => ValidTlbMask(unchecked((uint)(tlbCount - 1)))
        ? checked(((long)tlbCount * FullMemoryTlbEntry.ByteSize + JumpCount * FullNativeJumpEntry.ByteSize +
            FullStackMemoryDescriptor.ByteSize + FullX87NativeStorage.ByteSize + 15) & ~15L)
        : throw new ArgumentOutOfRangeException(nameof(tlbCount));
    internal static long WorkerChargeFor(int tlbCount) => checked(WorkerNativeStorageBytesFor(tlbCount) + X87BindingAndWorkerAllowance + FullWorkerNativeState.MappingCacheCharge);
    internal const long WorkerNativeStorageBytes = (TlbCount * FullMemoryTlbEntry.ByteSize +
        JumpCount * FullNativeJumpEntry.ByteSize + FullStackMemoryDescriptor.ByteSize + FullX87NativeStorage.ByteSize + 15) & ~15L;
    internal const long WorkerCharge = WorkerNativeStorageBytes + X87BindingAndWorkerAllowance + FullWorkerNativeState.MappingCacheCharge;
    // Fold higher page bits into the index, preserving bit0 so adjacent pages
    // NEVER collide (including fold boundaries and every supported mask).
    // NativeTlbLookup emits this same unsigned expression. Capacity is unchanged.
    internal static int TlbIndex(ulong address, uint mask)
    {
        ulong page = address >> 12;
        return (int)((page ^ ((page >> 5) & ~1UL)) & mask);
    }
    // Byte-PC/fold8: mix bits8..15 into the low byte at the fixed 256-entry capacity.
    // NativeJumpCache emits the same unsigned hash; exact tags still authenticate every hit.
    internal static int JumpIndex(ulong address, uint mask) => (int)((address ^ (address >> 8)) & mask);

    // The MMU assist handles mappings/permissions only. It NEVER loads a guest
    // scalar value, performs an ALU operation, updates RSP/IP, or calls Op*.
    // Caller: native has fully returned; own Authority.Gate and passive physical
    // read scope, reset the producer TLB after acquiring that scope. Retry after
    // materializing PAGE_RSRV through the authored runtime lookup boundary.
    internal static FullMemoryAssistResult TryFill(Blink program, Blink.Machine* machine,
        FullBlockFrame* frame, FullCodeAuthority authority)
    {
        if (!Monitor.IsEntered(authority.Gate)) throw new InvalidOperationException("Native MMU fill needs authority gate.");
        authority.ResetDataPermissionDenial();
        if (IsAtomicRequest(frame)) return TryFillAtomic(program, machine, frame, authority);
        if (frame->StackMemory != null && frame->StackMemory->Version != FullStackMemoryDescriptor.CurrentVersion)
        {
            if (IsCanonicalX87MemoryProbe(frame)) return TryFillCanonicalX87MemoryProbe(program, machine, frame, authority);
            if (IsMaskedStoreRequest(frame)) return TryFillMaskedStore(program, machine, frame, authority);
            if (IsEnterRequest(frame)) return TryFillEnter(program, machine, frame, authority);
            throw new InvalidOperationException("Unknown or canceled native memory descriptor tag.");
        }
        if (frame->StackMemory != null && frame->StackMemory->Count != 0)
            return TryFillStack(program, machine, frame, authority);
        if (frame->MemoryResume != 1 || !ValidMemoryWidth(frame->MemoryWidth) ||
            (uint)frame->MemoryAccess is < 1 or > 3 ||
            frame->MemoryTlb == null || !ValidTlbMask(frame->MemoryTlbMask))
            throw new InvalidOperationException("Invalid native memory assist request.");
        ++frame->MemoryAssists;
        uint bytes = frame->MemoryWidth / 8;
        ulong address = frame->MemoryAddress;
        // Blink's long-mode userspace accepts signed canonical 48-bit addresses.
        if (!Canonical(address) || !Canonical(unchecked(address + bytes - 1)) || address > ulong.MaxValue - (bytes - 1))
            return Fault(frame, address, 1);
        ulong epoch = authority.Epoch;
        int first = Math.Min((int)bytes, 4096 - (int)(address & 4095));
        var result = ResolvePage(program, machine, frame, authority, address & ~4095UL, epoch, out var a);
        if (result != FullMemoryAssistResult.Resolved) return result;
        FullMemoryTlbEntry b = default;
        if (first != bytes)
        {
            result = ResolvePage(program, machine, frame, authority, unchecked((address & ~4095UL) + 4096), epoch, out b);
            if (result != FullMemoryAssistResult.Resolved) return result;
        }
        if (authority.Epoch != epoch) return FullMemoryAssistResult.Retry;
        // A direct-mapped collision between adjacent pages is impossible for our
        // fixed power-of-two table. Publish both only after ALL prechecks pass.
        frame->MemoryTlb[TlbIndex(a.Page, frame->MemoryTlbMask)] = a;
        if (first != bytes) frame->MemoryTlb[TlbIndex(b.Page, frame->MemoryTlbMask)] = b;
        frame->MemoryEpoch = epoch;
        return FullMemoryAssistResult.Resolved;
    }

    private static FullMemoryAssistResult ResolvePage(Blink p, Blink.Machine* m, FullBlockFrame* f,
        FullCodeAuthority authority, ulong page, ulong epoch, out FullMemoryTlbEntry entry)
    {
        entry = default;
        if (!JitPageIdentity.TryRead(p, m, page, out ulong pte) || (pte & (ulong)Blink.PAGE_V) == 0)
            return Fault(f, Math.Max(f->MemoryAddress, page), 1);
        if ((pte & (ulong)Blink.PAGE_U) == 0)
            return Fault(f, Math.Max(f->MemoryAddress, page), 2);
        if ((pte & (ulong)Blink.PAGE_RSRV) != 0) return FullMemoryAssistResult.DemandPage;
        if ((f->MemoryAccess & FullMemoryAccess.Write) != 0 && (pte & (ulong)Blink.PAGE_RW) == 0)
            return Fault(f, Math.Max(f->MemoryAddress, page), 2);
        byte* host = (pte & (ulong)Blink.PAGE_HOST) != 0 ? p.FindHostPage(pte) : p.GetPageAddress(m->blink_host_system, pte, false);
        if (host == null || p.BlinkHostMemoryContains(host, 4096) == 0) return FullMemoryAssistResult.UnsupportedBacking;
        if ((pte & (ulong)Blink.PAGE_HOST) != 0 && p.BlinkHostMemoryFilePageAvailable(host) == 0)
            return Fault(f, Math.Max(f->MemoryAddress, page), unchecked((uint)-2)); // selected Blink BUS_ADRERR sentinel
        bool write = (f->MemoryAccess & FullMemoryAccess.Write) != 0;
        bool shared = (pte & (ulong)Blink.PAGE_HOST) != 0 && p.BlinkHostMemoryFilePageShared(host) != 0;
        // Retain only pure reads of the exact active local lease whose observer
        // drains this authority. PAGE_RW never proves shared write eligibility.
        // Availability/identity/ownership checks publish no pointer on failure.
        if (shared && (f->MemoryAccess != FullMemoryAccess.Read ||
            !authority.SharedReadBackingOwned(p.BlinkHostMemoryFilePageLease(host), (nint)host)))
            return FullMemoryAssistResult.UnsupportedBacking;
        bool admitted = authority.TryGetNativeDataPermission((nint)host, write, out ulong checkedEpoch);
        if (checkedEpoch != epoch) return FullMemoryAssistResult.Retry;
        if (!admitted)
        {
            f->MemoryFlags = FullMemoryFlags.WriteBarrier;
            return write || authority.Saturated ? FullMemoryAssistResult.WriteBarrier : FullMemoryAssistResult.Retry;
        }
        // A read refill can revisit a writable entry while resolving another
        // page. Retain that permission only for the exact admitted identity;
        // PAGE_RW is necessary but never sufficient to grant native stores.
        bool writable = write;
        if (!shared && !write && (pte & (ulong)Blink.PAGE_RW) != 0)
        {
            ref var cached = ref f->MemoryTlb[TlbIndex(page, f->MemoryTlbMask)];
            if (cached.Page == page && cached.Host == host && cached.Identity == (ulong)host &&
                cached.Context == f->Context && cached.AuthorityEpoch == epoch &&
                cached.Permissions == (FullMemoryAccess.Read | FullMemoryAccess.Write) &&
                cached.Flags == FullMemoryFlags.None && cached.ValidStart == 0 && cached.ValidEnd == 4096)
            {
                writable = authority.TryGetNativeDataPermission((nint)host, true, out checkedEpoch);
                if (checkedEpoch != epoch || authority.Epoch != epoch) return FullMemoryAssistResult.Retry;
                // Optional retention failure is a successful conservative read,
                // not a write-barrier request or a failed read admission.
                authority.ResetDataPermissionDenial();
            }
        }
        entry = new() { Page = page, Host = host, AuthorityEpoch = epoch, Context = f->Context,
            Permissions = FullMemoryAccess.Read | (writable ? FullMemoryAccess.Write : 0),
            Identity = (ulong)host, ValidStart = 0, ValidEnd = 4096 };
        return FullMemoryAssistResult.Resolved;
    }

    internal static bool Canonical(ulong value) => unchecked((long)value) is >= -0x800000000000L and < 0x800000000000L;
    internal static FullMemoryAssistResult Fault(FullBlockFrame* f, ulong address, uint linuxCode)
    {
        f->FaultTrap = 14; // Architectural #PF; controller maps to Blink's negative halt reason.
        f->FaultCode = linuxCode;
        f->FaultAddress = address;
        return FullMemoryAssistResult.GuestFault;
    }

    // After exact token/site reacquisition and fresh header/global revalidation.
    // Preserve Executed, NativeLimit and the issue that NeedMemory reserved.
    internal static void ResumeSameIssue(FullBlockFrame* f)
    {
        if (f->MemoryResume != 1 || f->ResumeIndex != f->MemorySite || f->FirstPreissued != 0)
            throw new InvalidOperationException("Native memory resume is not the same approved site.");
        f->FirstPreissued = 1;
    }

    // A barrier ticket is invocation/site/EA/width/epoch scoped. The parent must
    // own a dedicated write exclusion scope that has revoked ALL physical code
    // aliases and drained readers BEFORE issuing this ticket. It must retain
    // that scope through native RET. The consumed-ticket postcommit exit is
    // independent of uint NativeLimit and ulong Executed, including >4G work.
    // This publishes checked pointers, never performs the guest write itself.
    internal static void PublishBarrierTicket(FullBlockFrame* f, byte* first, byte* second,
        uint firstLength, ulong epoch, ulong ticket)
    {
        uint bytes = f->MemoryWidth / 8;
        if (f->MemoryResume != 1 || !ValidMemoryWidth(f->MemoryWidth) || ticket == 0 || first == null || firstLength == 0 || firstLength > bytes ||
            ((firstLength != bytes) != (second != null)) || epoch != f->ObservedGlobalEpoch)
            throw new InvalidOperationException("Invalid native write barrier ticket.");
        f->MemoryHost0 = first; f->MemoryHost1 = second; f->MemoryLength0 = firstLength;
        f->MemoryEpoch = epoch; f->MemoryTicket = ticket;
        f->MemoryFlags = FullMemoryFlags.BarrierTicket;
    }

    internal static void PublishJump(FullNativeJumpEntry* cache, uint mask, ulong storeGeneration,
        TranslationBlockHeader* header, in TranslationBlockHeader snapshot)
    {
        if (cache == null || mask != JumpCount - 1 || storeGeneration == 0 || header == null || snapshot.Live != 1)
            throw new InvalidOperationException("Invalid native jump hint publication.");
        cache[JumpIndex(snapshot.GuestPc, mask)] = new() { GuestPc = snapshot.GuestPc, Context = snapshot.Context,
            StoreGeneration = storeGeneration, OwnerCookie = snapshot.OwnerCookie,
            BodyGeneration = snapshot.BodyGeneration, Header = header };
    }
}

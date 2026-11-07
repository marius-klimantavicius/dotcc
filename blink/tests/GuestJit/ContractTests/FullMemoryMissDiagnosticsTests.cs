using Managed.Emulation.Execution;

unsafe partial class Program
{
    // Source-only R84 contracts. Parent wires this entry into Main and owns all
    // compilation/execution. These tests never emit or enter machine code.
    static void CheckFullMemoryMissDiagnostics()
    {
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[FullNativeMemory.TlbCount];
        new Span<FullMemoryTlbEntry>(tlb, FullNativeMemory.TlbCount).Clear();
        Span<bool> seen = stackalloc bool[FullMemoryMissDiagnostics.ReasonCount];
        seen.Clear();
        Check(sizeof(FullMemoryMissReason) == 1 && FullMemoryMissDiagnostics.ReasonCount == 28,
            "R84 compact diagnostic reason bound");

        CheckFullMemoryMissReasons(tlb, seen);
        CheckFullMemoryMissShapes(tlb, seen);
        CheckFullMemoryMissCheckOrder(tlb, seen);
        CheckFullMemoryMissCrossing(tlb, seen);
        CheckFullMemoryMissTickets(tlb, seen);
        CheckFullMemoryMissMultiAccess(tlb, seen);
        CheckFullMemoryMissObservationOnly(tlb, seen);

        for (int i = 0; i < seen.Length; ++i)
            Check(seen[i], "R84 every bounded diagnostic reason exercised");
        _ = FullMemoryMissWarmSpan(tlb, out _);
        long allocated = FullMemoryMissWarmSpan(tlb, out bool correct);
        Check(correct && allocated == 0, "R84 warmed static observation allocates zero managed bytes");
    }

    static FullBlockFrame FullMemoryMissFrame(FullMemoryTlbEntry* tlb, ulong address = 0x500000)
        => new()
        {
            Version = FullBlockFrame.AbiVersion, Size = FullBlockFrame.ByteSize,
            MemoryTlb = tlb, MemoryTlbMask = FullNativeMemory.TlbCount - 1,
            MemoryAddress = address, MemoryWidth = 64,
            MemoryAccess = FullMemoryAccess.Read | FullMemoryAccess.Write,
            MemoryResume = 1, MemorySite = 3, ResumeIndex = 3,
            ObservedGlobalEpoch = 11, Context = 0x1234, Exit = FullBlockExit.NeedMemory,
            // Poison all pointers other than the synthetic owned TLB. The
            // observer must only compare pointer values, never follow them.
            Header = (TranslationBlockHeader*)1, Machine = (void*)1,
            GlobalEpochPointer = (ulong*)1, CodeAuthorityEpochPointer = (ulong*)1,
            MemoryHost0 = (byte*)1, MemoryHost1 = (byte*)1,
            MemoryEpoch = ulong.MaxValue, ObservedCodeAuthorityEpoch = 99
        };

    static FullMemoryTlbEntry FullMemoryMissEntry(ulong page) => new()
    {
        Page = page, Host = (byte*)1, AuthorityEpoch = 11, Context = 0x1234,
        Permissions = FullMemoryAccess.Read | FullMemoryAccess.Write,
        ValidEnd = 4096, Identity = 1
    };

    static void ExpectFullMemoryMiss(in FullBlockFrame frame, ulong currentEpoch,
        FullMemoryMissReason expected, Span<bool> seen, string name)
    {
        var observed = FullMemoryMissDiagnostics.Observe(in frame, currentEpoch);
        Check((uint)observed < (uint)FullMemoryMissDiagnostics.ReasonCount, "R84 reason stays in counter array bound");
        Check(observed == expected, name);
        seen[(int)observed] = true;
    }

    static void CheckFullMemoryMissReasons(FullMemoryTlbEntry* tlb, Span<bool> seen)
    {
        ReadOnlySpan<FullMemoryMissReason> first = stackalloc FullMemoryMissReason[]
        {
            FullMemoryMissReason.EmptyPageTag, FullMemoryMissReason.ConflictPageTag,
            FullMemoryMissReason.CachedEntryEpoch, FullMemoryMissReason.ContextMismatch,
            FullMemoryMissReason.ReadToWritePermission, FullMemoryMissReason.AccessPermission,
            FullMemoryMissReason.EntryBounds, FullMemoryMissReason.WriteBarrier, FullMemoryMissReason.NullHost
        };
        ReadOnlySpan<FullMemoryMissReason> second = stackalloc FullMemoryMissReason[]
        {
            FullMemoryMissReason.CrossingSecondPageEmptyPageTag, FullMemoryMissReason.CrossingSecondPageConflictPageTag,
            FullMemoryMissReason.CrossingSecondPageCachedEntryEpoch, FullMemoryMissReason.CrossingSecondPageContextMismatch,
            FullMemoryMissReason.CrossingSecondPageReadToWritePermission, FullMemoryMissReason.CrossingSecondPageAccessPermission,
            FullMemoryMissReason.CrossingSecondPageEntryBounds, FullMemoryMissReason.CrossingSecondPageWriteBarrier,
            FullMemoryMissReason.CrossingSecondPageNullHost
        };
        var frame = FullMemoryMissFrame(tlb);
        ulong page = frame.MemoryAddress;
        int a = FullNativeMemory.TlbIndex(page, frame.MemoryTlbMask);
        int b = FullNativeMemory.TlbIndex(page + 4096, frame.MemoryTlbMask);
        for (int i = 0; i < first.Length; ++i)
        {
            tlb[a] = FullMemoryMissFaultEntry(page, i);
            ExpectFullMemoryMiss(in frame, 11, first[i], seen, "R84 first-page captured predicate");
            frame.MemoryAddress = page + 4095;
            // A first-page failure wins even when the access crosses pages.
            tlb[b] = default;
            ExpectFullMemoryMiss(in frame, 11, first[i], seen, "R84 crossing preserves first-page failure precedence");
            tlb[a] = FullMemoryMissEntry(page);
            tlb[b] = FullMemoryMissFaultEntry(page + 4096, i);
            ExpectFullMemoryMiss(in frame, 11, second[i], seen, "R84 first-good second-page observed cause");
            frame.MemoryAddress = page;
        }
        tlb[a] = FullMemoryMissEntry(page);
        ExpectFullMemoryMiss(in frame, 12, FullMemoryMissReason.FrameRuntimeEpochChanged, seen,
            "R84 retained frame detects current runtime epoch advance");
        ExpectFullMemoryMiss(in frame, 10, FullMemoryMissReason.FrameRuntimeEpochChanged, seen,
            "R84 epoch comparison uses inequality without assuming monotonic ordering");
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.ObservedReady, seen,
            "R84 unchanged runtime epoch reveals no observed cause");

        frame.MemoryAccess = FullMemoryAccess.Read;
        tlb[a].Flags = FullMemoryFlags.WriteBarrier;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.ObservedReady, seen,
            "R84 entry write barrier does not block a read");
        tlb[a].Permissions = FullMemoryAccess.Write;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.AccessPermission, seen,
            "R84 missing read permission is not a read-to-write upgrade");
        frame.MemoryAccess = FullMemoryAccess.Write;
        tlb[a].Permissions = FullMemoryAccess.Read;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.ReadToWritePermission, seen,
            "R84 write-only request upgrades a cached readable entry");
        tlb[a] = FullMemoryMissEntry(page); tlb[a].ValidStart = 1;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.EntryBounds, seen,
            "R84 partial starting bound fails even when it contains the access");
        tlb[a] = FullMemoryMissEntry(page); tlb[a].ValidEnd = 4097;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.EntryBounds, seen,
            "R84 oversized ending bound also fails exact whole-page check");
    }

    static FullMemoryTlbEntry FullMemoryMissFaultEntry(ulong page, int fault)
    {
        var entry = FullMemoryMissEntry(page);
        switch (fault)
        {
            case 0: entry = default; break;
            case 1: entry.Page = page + (ulong)FullNativeMemory.TlbCount * 4096; break;
            case 2: entry.AuthorityEpoch = 10; break;
            case 3: entry.Context = 0; break;
            case 4: entry.Permissions = FullMemoryAccess.Read; break;
            case 5: entry.Permissions = 0; break;
            case 6: entry.ValidEnd = 4095; break;
            case 7: entry.Flags = FullMemoryFlags.WriteBarrier; break;
            case 8: entry.Host = null; break;
        }
        return entry;
    }

    static void CheckFullMemoryMissShapes(FullMemoryTlbEntry* tlb, Span<bool> seen)
    {
        var frame = FullMemoryMissFrame((FullMemoryTlbEntry*)1);
        ReadOnlySpan<uint> widths = stackalloc uint[] { 0, 1, 7, 9, 4097, uint.MaxValue };
        foreach (uint width in widths)
        {
            frame.MemoryWidth = width;
            ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.MalformedRequest, seen,
                "R84 malformed width returns before unsafe TLB access");
        }
        frame.MemoryWidth = 64;
        ReadOnlySpan<uint> accesses = stackalloc uint[] { 0, 4, uint.MaxValue };
        foreach (uint access in accesses)
        {
            frame.MemoryAccess = (FullMemoryAccess)access;
            ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.MalformedRequest, seen,
                "R84 malformed or disabled atomic access returns before TLB access");
        }
        frame.MemoryAccess = FullMemoryAccess.Read;
        frame.MemoryResume = 0;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.MalformedRequest, seen, "R84 request must retain issue");
        frame.MemoryResume = 2;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.MalformedRequest, seen, "R84 request resume is exactly one");
        frame.MemoryResume = 1; frame.MemorySite = frame.ResumeIndex + 1;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.MalformedRequest, seen, "R84 request site agrees with resume site");
        frame.MemorySite = frame.ResumeIndex;

        ReadOnlySpan<uint> masks = stackalloc uint[] { 0, 2, 6, 64,
            FullNativeMemory.TlbCount * 2u - 1, FullNativeMemory.MaximumTlbCount * 2u - 1, uint.MaxValue };
        // Larger power-of-two shapes accepted by generic native validation
        // must not index the default worker allocation. A poison pointer ensures
        // every rejected mask exits before reading even its first entry.
        ulong highestPage = 0;
        while (FullNativeMemory.TlbIndex(highestPage << 12, FullNativeMemory.TlbCount - 1) != FullNativeMemory.TlbCount - 1)
            ++highestPage;
        frame.MemoryAddress = highestPage << 12; // reaches highest slot for every supported submask
        foreach (uint mask in masks)
        {
            frame.MemoryTlbMask = mask;
            ExpectFullMemoryMiss(in frame, 12, FullMemoryMissReason.MissingOrMalformedTlb, seen,
                "R84 malformed or oversized mask cannot produce out-of-range TLB read");
        }
        frame.MemoryTlbMask = FullNativeMemory.TlbCount - 1; frame.MemoryTlb = null;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.MissingOrMalformedTlb, seen, "R84 absent worker TLB");

        frame.MemoryTlb = tlb;
        for (uint mask = 1; mask < FullNativeMemory.TlbCount; mask = mask * 2 + 1)
        {
            frame.MemoryTlbMask = mask;
            int slot = FullNativeMemory.TlbIndex(frame.MemoryAddress, mask);
            Check(slot == mask, "R84 synthetic access reaches highest allowed TLB index");
            tlb[slot] = FullMemoryMissEntry(frame.MemoryAddress);
            ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.ObservedReady, seen,
                "R84 valid bounded mask remains inside owned worker allocation");
        }
        frame.MemoryTlb = (FullMemoryTlbEntry*)1;
        frame.MemoryAddress = 1UL << 47;
        ExpectFullMemoryMiss(in frame, 12, FullMemoryMissReason.NonCanonicalAddress, seen,
            "R84 starting noncanonical address precedes TLB and epochs");
        frame.MemoryAddress = (1UL << 47) - 1;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.NonCanonicalAddress, seen,
            "R84 canonical start with noncanonical final byte");
        frame.MemoryAddress = ulong.MaxValue;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.AddressOverflow, seen,
            "R84 canonical negative address addition overflow precedes lookup");
    }

    static void CheckFullMemoryMissCheckOrder(FullMemoryTlbEntry* tlb, Span<bool> seen)
    {
        var frame = FullMemoryMissFrame(tlb);
        int a = FullNativeMemory.TlbIndex(frame.MemoryAddress, frame.MemoryTlbMask);
        tlb[a] = default;
        ExpectFullMemoryMiss(in frame, 12, FullMemoryMissReason.EmptyPageTag, seen, "R84 page tag precedes live runtime epoch");
        tlb[a] = FullMemoryMissEntry(frame.MemoryAddress); tlb[a].AuthorityEpoch = 10; tlb[a].Context = 0;
        ExpectFullMemoryMiss(in frame, 12, FullMemoryMissReason.CachedEntryEpoch, seen, "R84 cached epoch precedes runtime epoch and context");
        tlb[a].AuthorityEpoch = 11;
        ExpectFullMemoryMiss(in frame, 12, FullMemoryMissReason.FrameRuntimeEpochChanged, seen, "R84 runtime epoch precedes context");
        tlb[a].Permissions = 0;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.ContextMismatch, seen, "R84 context precedes permissions");
        tlb[a].Context = frame.Context; tlb[a].ValidStart = 1;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.AccessPermission, seen, "R84 permissions precede bounds");
        tlb[a].Permissions = frame.MemoryAccess; tlb[a].Flags = FullMemoryFlags.WriteBarrier; tlb[a].Host = null;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.EntryBounds, seen, "R84 bounds precede write barrier and null host");
        tlb[a].ValidStart = 0;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.WriteBarrier, seen, "R84 write barrier precedes null host");
        tlb[a].Flags = 0;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.NullHost, seen, "R84 null host is final predicate");

        frame.MemoryAddress = 0;
        tlb[0] = default;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.CachedEntryEpoch, seen,
            "R84 zero page matches zero tag so native fails later at cached epoch");
        tlb[0] = FullMemoryMissEntry(0);
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.ObservedReady, seen, "R84 populated guest page zero is valid");
        ulong collisionPage = 1;
        while (FullNativeMemory.TlbIndex(collisionPage << 12, frame.MemoryTlbMask) != 0) ++collisionPage;
        frame.MemoryAddress = collisionPage << 12; // nonzero page colliding with populated slot zero
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.ConflictPageTag, seen,
            "R84 populated page-zero mapping is a conflict rather than an empty entry");
    }

    static void CheckFullMemoryMissCrossing(FullMemoryTlbEntry* tlb, Span<bool> seen)
    {
        var frame = FullMemoryMissFrame(tlb);
        ulong page = frame.MemoryAddress;
        int a = FullNativeMemory.TlbIndex(page, frame.MemoryTlbMask);
        int b = FullNativeMemory.TlbIndex(page + 4096, frame.MemoryTlbMask);
        ReadOnlySpan<uint> widths = stackalloc uint[] { 8, 16, 32, 64, 128 };
        foreach (uint width in widths)
        {
            frame.MemoryWidth = width;
            frame.MemoryAddress = page + 4096 - width / 8;
            tlb[a] = FullMemoryMissEntry(page); tlb[b] = default;
            ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.ObservedReady, seen,
                "R84 final-byte boundary does not inspect following page");
            frame.MemoryAddress = page + 4095;
            ExpectFullMemoryMiss(in frame, 11, width == 8 ? FullMemoryMissReason.ObservedReady : FullMemoryMissReason.CrossingSecondPageEmptyPageTag,
                seen, "R84 crossing derives from exact width and address");
            tlb[b] = FullMemoryMissEntry(page + 4096);
            ExpectFullMemoryMiss(in frame, 11, width == 8 ? FullMemoryMissReason.ObservedReady : FullMemoryMissReason.CrossingObservedReady,
                seen, "R84 both checked pages can be ready at refill observation");
        }
        // Canonical negative addresses use the same unsigned page indexing.
        frame.MemoryAddress = unchecked(0UL - 4097); frame.MemoryWidth = 16;
        page = frame.MemoryAddress & ~4095UL;
        tlb[FullNativeMemory.TlbIndex(page, frame.MemoryTlbMask)] = FullMemoryMissEntry(page);
        tlb[FullNativeMemory.TlbIndex(page + 4096, frame.MemoryTlbMask)] = FullMemoryMissEntry(page + 4096);
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.CrossingObservedReady, seen,
            "R84 negative canonical crossing remains in bounds");
    }

    static void CheckFullMemoryMissTickets(FullMemoryTlbEntry* tlb, Span<bool> seen)
    {
        var frame = FullMemoryMissFrame(tlb);
        frame.MemoryFlags = FullMemoryFlags.BarrierTicket; frame.MemoryTicket = 1;
        frame.MemoryTlb = (FullMemoryTlbEntry*)1; frame.MemoryTlbMask = uint.MaxValue;
        frame.MemoryAddress = 1UL << 47; frame.MemoryEpoch = 10;
        ExpectFullMemoryMiss(in frame, 12, FullMemoryMissReason.BarrierTicketEpochMismatch, seen,
            "R84 ticket captured epoch precedes runtime epoch and ordinary lookup");
        frame.MemoryEpoch = frame.ObservedGlobalEpoch;
        ExpectFullMemoryMiss(in frame, 12, FullMemoryMissReason.FrameRuntimeEpochChanged, seen,
            "R84 ticket observes current runtime epoch without dereferencing epoch pointer");
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.BarrierTicketObservedReady, seen,
            "R84 exact-ticket epoch checks bypass ordinary address and TLB checks");
    }

    static void CheckFullMemoryMissMultiAccess(FullMemoryTlbEntry* tlb, Span<bool> seen)
    {
        FullStackMemoryDescriptor* descriptor = stackalloc FullStackMemoryDescriptor[1];
        *descriptor = default;
        var frame = FullMemoryMissFrame(tlb);
        frame.StackMemory = descriptor;
        tlb[FullNativeMemory.TlbIndex(frame.MemoryAddress, frame.MemoryTlbMask)] = FullMemoryMissEntry(frame.MemoryAddress);
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.ObservedReady, seen,
            "R84 unused owned stack descriptor does not change scalar observation");

        descriptor->Version = FullStackMemoryDescriptor.CurrentVersion;
        descriptor->Site = frame.MemorySite;
        descriptor->Request0.Host0 = descriptor->Request0.Host1 = (byte*)1;
        descriptor->Request1.Host0 = descriptor->Request1.Host1 = (byte*)1;
        // Only Count is observed. Shape/state/host checks belong to R85's
        // authenticated multi-request assist, not scalar miss attribution.
        ReadOnlySpan<FullStackMemoryState> states = stackalloc FullStackMemoryState[]
        {
            FullStackMemoryState.Empty, FullStackMemoryState.Request, FullStackMemoryState.Ready,
            FullStackMemoryState.TicketReady, FullStackMemoryState.Consumed, (FullStackMemoryState)uint.MaxValue
        };
        frame.MemoryTlb = (FullMemoryTlbEntry*)1;
        foreach (var state in states)
        {
            descriptor->State = state;
            descriptor->Count = 1;
            ExpectFullMemoryMiss(in frame, 12, FullMemoryMissReason.UnsupportedMultiAccess, seen,
                "R84 active one-request stack descriptor cannot become scalar attribution");
            descriptor->Count = 2;
            frame.MemoryFlags = FullMemoryFlags.BarrierTicket;
            ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.UnsupportedMultiAccess, seen,
                "R84 two-request descriptor precedes scalar ticket checks and TLB reads");
        }
        descriptor->Count = uint.MaxValue; descriptor->Version = 0;
        frame.MemoryResume = 0; frame.MemoryWidth = 0;
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.UnsupportedMultiAccess, seen,
            "R84 malformed active descriptor remains outside scalar semantics");
    }

    static void CheckFullMemoryMissObservationOnly(FullMemoryTlbEntry* tlb, Span<bool> seen)
    {
        var frame = FullMemoryMissFrame(tlb);
        int a = FullNativeMemory.TlbIndex(frame.MemoryAddress, frame.MemoryTlbMask);
        tlb[a] = FullMemoryMissEntry(frame.MemoryAddress);
        // Ordinary lookup ignores staged host/length/MemoryEpoch and code-epoch
        // fields. It cannot follow a retained Header after native epoch release.
        frame.MemoryLength0 = uint.MaxValue; frame.NativeMemoryComputations = 17; frame.MemoryAssists = 19;
        FullBlockFrame before = frame;
        Span<FullMemoryTlbEntry> snapshot = stackalloc FullMemoryTlbEntry[FullNativeMemory.TlbCount];
        new ReadOnlySpan<FullMemoryTlbEntry>(tlb, FullNativeMemory.TlbCount).CopyTo(snapshot);
        ExpectFullMemoryMiss(in frame, 11, FullMemoryMissReason.ObservedReady, seen,
            "R84 observation ignores stale scratch fields and poisoned non-TLB pointers");
        Check(new ReadOnlySpan<byte>(&frame, sizeof(FullBlockFrame)).SequenceEqual(new ReadOnlySpan<byte>(&before, sizeof(FullBlockFrame))) &&
            new ReadOnlySpan<byte>(tlb, FullNativeMemory.TlbCount * FullMemoryTlbEntry.ByteSize).SequenceEqual(
                System.Runtime.InteropServices.MemoryMarshal.AsBytes(snapshot)),
            "R84 observer leaves complete frame and worker TLB unchanged");
    }

    static long FullMemoryMissWarmSpan(FullMemoryTlbEntry* tlb, out bool correct)
    {
        var frame = FullMemoryMissFrame(tlb);
        FullStackMemoryDescriptor descriptor = default;
        frame.StackMemory = &descriptor;
        const ulong page = 0x500000;
        int a = FullNativeMemory.TlbIndex(page, frame.MemoryTlbMask);
        int b = FullNativeMemory.TlbIndex(page + 4096, frame.MemoryTlbMask);
        correct = true;
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 4096; ++i)
        {
            descriptor.Count = 1;
            correct &= FullMemoryMissDiagnostics.Observe(in frame, 11) == FullMemoryMissReason.UnsupportedMultiAccess;
            descriptor.Count = 0;
            frame.MemoryAddress = page; tlb[a] = default;
            correct &= FullMemoryMissDiagnostics.Observe(in frame, 11) == FullMemoryMissReason.EmptyPageTag;
            tlb[a] = FullMemoryMissEntry(page);
            correct &= FullMemoryMissDiagnostics.Observe(in frame, 12) == FullMemoryMissReason.FrameRuntimeEpochChanged;
            tlb[a].Permissions = FullMemoryAccess.Read;
            correct &= FullMemoryMissDiagnostics.Observe(in frame, 11) == FullMemoryMissReason.ReadToWritePermission;
            tlb[a] = FullMemoryMissEntry(page); frame.MemoryAddress = page + 4095; tlb[b] = default;
            correct &= FullMemoryMissDiagnostics.Observe(in frame, 11) == FullMemoryMissReason.CrossingSecondPageEmptyPageTag;
            tlb[b] = FullMemoryMissEntry(page + 4096);
            correct &= FullMemoryMissDiagnostics.Observe(in frame, 11) == FullMemoryMissReason.CrossingObservedReady;
        }
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}

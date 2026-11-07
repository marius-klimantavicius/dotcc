using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using Read207Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // ONE serial selector. AUTHORED_UNRUN: parent builds and runs the composed
    // source on both producers. Windows encoding only on non-Windows hosts.
    static void CheckFullOwnedSharedReadR207(byte* machine)
    {
        Check(sizeof(FullMemoryTlbEntry) == 64 && FullBlockFrame.ByteSize == 416 &&
            FullBlockFrame.AbiVersion == 0x46420007, "R207 unchanged native ABI/storage charge");
        OwnedRead207Ownership();
        OwnedRead207Widths();
        OwnedRead207Pairs();
        OwnedRead207Controller();
        OwnedRead207Rep();
        OwnedRead207Topology();
        OwnedRead207Drain();
        OwnedRead207Boundaries();
        OwnedRead207FreshValues();
        OwnedRead207StackControl();
        // Independent existing guard, issue, permissions, provenance and fault
        // oracles are grouped here; none is weakened for shared data admission.
        Cache181Encoding(); // actual SysV + Windows memory encoding
        SharedMemoryTicketsAndPolls188(machine);
        CheckFullMemoryMissAndPermissions(machine);
        CheckFullCanonicalX87ProbeMaterialization(); // permission-only, no pointer
        CheckFullCodeRoleMappingsR204(machine); // includes R193/R200 and both ABI guards
        CheckRepKernelFailurePaths();
        CheckFullRepStop();
        CheckFullRepRejectedIssue();
    }

    static int OwnedRead207Map(Read207Fixture f, ulong address, byte fill = 0x35)
    {
        var opened = f.Io.CreateMemoryFile("owned-data-r207", 0);
        Check(opened.Succeeded, "R207 create real local memfd");
        Check(f.Io.WriteAt(opened.Value, Enumerable.Repeat(fill, 8192).ToArray(), 0).Value == 8192,
            "R207 seed backing through observed host API");
        f.AtomicMapSharedPage(address, opened.Value, 0);
        f.AtomicMapSharedPage(address + 4096, opened.Value, 4096);
        f.DataScope(0, () => { f.Expose(address, 0); f.Expose(address + 4096, 0); });
        return opened.Value;
    }

    static void OwnedRead207Ownership()
    {
        using var f = new Read207Fixture(); const ulong data = Read207Fixture.Base;
        int fd = OwnedRead207Map(f, data);
        nint host = f.Physical(data);
        ulong lease = f.Program.BlinkHostMemoryFilePageLease((void*)host);
        Check(lease != 0 && f.Io.MemoryFileSharedReadOwned(lease, host, f.Jit.Authority),
            "R207 actual C mapping lease authenticates bound authority and backing");
        using var otherAuthority = new FullCodeAuthority(f.Io, 4096, 1, 1 << 20);
        Check(!f.Io.MemoryFileSharedReadOwned(lease, host, otherAuthority) &&
            !f.Io.MemoryFileSharedReadOwned(lease, host + 4096, f.Jit.Authority) &&
            !f.Io.MemoryFileSharedReadOwned(lease, host + 1, f.Jit.Authority) &&
            !f.Io.MemoryFileSharedReadOwned(0, host, f.Jit.Authority) &&
            !f.Io.MemoryFileSharedReadOwned(ulong.MaxValue, host, f.Jit.Authority),
            "R207 wrong authority/address/alignment/unknown/zero lease deny ownership");
        var foreign = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
        try
        {
            Check(!foreign.MemoryFileSharedReadOwned(lease, host, f.Jit.Authority), "R207 foreign IO denies local lease");
        }
        finally { foreign.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
        f.Io.UnbindMemoryFileMutation(f.Jit);
        try
        {
            var request = Cache181Request(tlb, data);
            Check(FillFullMemoryMmu(f, &request) == FullMemoryAssistResult.UnsupportedBacking &&
                tlb[FullNativeMemory.TlbIndex(data, 63)].Host == null, "R207 unbound read publishes no hint");
            var observer = new HostMemoryFileMutationProbe();
            f.Io.BindMemoryFileMutation(observer);
            try
            {
                Check(f.Io.MemoryFileMutationOwned(lease) && !f.Io.MemoryFileSharedReadOwned(lease, host, f.Jit.Authority),
                    "R207 legacy nonnull observer is insufficient for retained shared data");
                Check(FillFullMemoryMmu(f, &request) == FullMemoryAssistResult.UnsupportedBacking,
                    "R207 foreign observer cannot admit a native data mapping");
            }
            finally { f.Io.UnbindMemoryFileMutation(observer); }
        }
        finally { f.Io.BindMemoryFileMutation(f.Jit); }
        int regular = f.Io.OpenFile("/r207-regular", FileAccessMode.Read | FileAccessMode.Write, create: true).Value;
        Check(f.Io.WriteAt(regular, new byte[4096], 0).Value == 4096, "R207 regular backing seed");
        ulong regularAddress = data + 0x10000;
        // Selected ordinary regular-file shared mappings support this read-only
        // form; the memfd helper requests RWX and is not a valid regular fixture.
        f.Owner.FullRetainedTestMapReadOnlyShared(regular, regularAddress);
        f.DataScope(0, () => f.Expose(regularAddress, 0));
        var r = Cache181Request(tlb, regularAddress);
        Check(FillFullMemoryMmu(f, &r) == FullMemoryAssistResult.UnsupportedBacking,
            "R207 general external regular-file read keeps conservative fallback");
        Check(f.Io.Close(fd).Succeeded && f.Io.MemoryFileSharedReadOwned(lease, host, f.Jit.Authority),
            "R207 descriptor close retains real active backing lease");
        f.Io.DisposeAsync().AsTask().GetAwaiter().GetResult();
        Check(!f.Io.MemoryFileSharedReadOwned(lease, host, f.Jit.Authority), "R207 disposed IO cannot grant retained-read ownership");
    }

    static void OwnedRead207Widths()
    {
        using var f = new Read207Fixture(); const ulong data = Read207Fixture.Base;
        OwnedRead207Map(f, data);
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
        // Every accepted dense byte width, including scalar, vector, x87 and
        // 512-byte records. No data is loaded by the mapping assist itself.
        for (uint bytes = 1; bytes <= FullNativeMemory.MaximumDenseMemoryBytes; ++bytes)
        foreach (uint offset in new uint[] { 64, 4096 - bytes, 4095 })
        {
            var r = Cache181Request(tlb, data + offset, FullMemoryAccess.Read, bytes * 8);
            Check(FillFullMemoryMmu(f, &r) == FullMemoryAssistResult.Resolved, "R207 dense read width/end/crossing resolves");
            var e = tlb[FullNativeMemory.TlbIndex(data, 63)];
            Check(e.Host == (byte*)f.Physical(data) && e.Identity == (ulong)e.Host && e.Page == data &&
                e.Context == r.Context && e.AuthorityEpoch == f.Jit.Authority.Epoch &&
                e.Permissions == FullMemoryAccess.Read && e.Flags == FullMemoryFlags.None &&
                e.ValidStart == 0 && e.ValidEnd == 4096, "R207 writable PTE grants exact Read only");
            if (offset + bytes > 4096)
            {
                var b = tlb[FullNativeMemory.TlbIndex(data + 4096, 63)];
                Check(b.Host == (byte*)f.Physical(data + 4096) && b.Identity == (ulong)b.Host &&
                    b.Permissions == FullMemoryAccess.Read && b.AuthorityEpoch == e.AuthorityEpoch,
                    "R207 second page independently authenticated read-only");
            }
        }
        // The same owned shared pages do not turn a permission-only proof
        // into a native pointer, TLB publication, or write reservation.
        var descriptor = new FullStackMemoryDescriptor {
            Version = FullCanonicalX87MemoryLowering.MemoryProbeTag, Count = 1, State = FullStackMemoryState.Request,
            Request0 = new() { Address = data + 4095, Access = FullMemoryAccess.Write, Width = 80 } };
        var proof = StackMemoryMmuRequest(&descriptor,tlb); proof.Machine = f.Machine();
        proof.MemoryAccess = FullMemoryAccess.Write;
        byte[] originalTlb = new ReadOnlySpan<byte>(tlb,64 * 64).ToArray();
        Check(FillFullMemoryMmu(f,&proof) == FullMemoryAssistResult.Resolved &&
            descriptor.State == FullStackMemoryState.Ready && descriptor.Request0.Host0 == null &&
            descriptor.Request0.Host1 == null && descriptor.Request0.Length0 == 0 && descriptor.Ticket == 0 &&
            new ReadOnlySpan<byte>(tlb,64 * 64).SequenceEqual(originalTlb),
            "R207 shared-page permission-only probe remains pointer/TLB/ticket free");
        int slot = FullNativeMemory.TlbIndex(data, 63);
        // Even a stale/forged prior RW hint cannot retain Write on a shared read.
        tlb[slot].Permissions = FullMemoryAccess.Read | FullMemoryAccess.Write;
        var read = Cache181Request(tlb, data);
        Check(FillFullMemoryMmu(f, &read) == FullMemoryAssistResult.Resolved &&
            tlb[slot].Permissions == FullMemoryAccess.Read, "R207 shared refill always demotes prior RW hint");
        foreach (var access in new[] { FullMemoryAccess.Write, FullMemoryAccess.Read | FullMemoryAccess.Write })
        {
            var before = tlb[slot]; var write = Cache181Request(tlb, data, access);
            Check(FillFullMemoryMmu(f, &write) == FullMemoryAssistResult.UnsupportedBacking &&
                new ReadOnlySpan<byte>(&before, 64).SequenceEqual(new ReadOnlySpan<byte>(tlb + slot, 64)),
                "R207 shared write/RMW still requires controller ticket with no publication");
        }
        var malformed = Cache181Request(tlb, data, (FullMemoryAccess)8);
        bool rejected = false;
        try { FillFullMemoryMmu(f, &malformed); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "R207 malformed access rejected before any pointer protocol");
        foreach (ulong address in new[] { 1UL << 47, ulong.MaxValue })
        {
            var r = Cache181Request(tlb, address);
            Check(FillFullMemoryMmu(f, &r) == FullMemoryAssistResult.GuestFault && r.FaultCode == 1,
                "R207 canonical/overflow faults remain precise");
        }
    }

    static FullMemoryAssistResult OwnedRead207PairFill(Read207Fixture f, FullBlockFrame* frame, bool compare)
    {
        f.Jit.BeginRead(f.Program, false);
        try
        {
            f.Program.ResetTlb(f.Machine());
            lock (f.Jit.Authority.Gate)
            {
                frame->ObservedGlobalEpoch = f.Jit.Authority.Epoch;
                return FullNativeMemory.TryFillStringPair(f.Program, f.Machine(), frame, f.Jit.Authority, compare);
            }
        }
        finally { f.Jit.EndRead(f.Program); }
    }

    static void OwnedRead207Pairs()
    {
        using var f = new Read207Fixture(); const ulong source = Read207Fixture.Base;
        ulong dest = Cache181Collision(source);
        OwnedRead207Map(f, source); f.Map(dest, 8192); f.Fill(dest, 8192);
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        foreach (bool compare in new[] { false, true })
        foreach (int width in new[] { 8, 16, 32, 64 })
        {
            new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
            var d = StringPairRequest(source + 4095, dest + 4095, width, compare);
            var frame = StackMemoryMmuRequest(&d, tlb);
            FullMemoryAssistResult result;
            // Private destination Write may need its existing first-registration
            // retry. Rebuild the unexecuted descriptor at the fresh epoch.
            for (int retry = 0;; ++retry)
            {
                result = OwnedRead207PairFill(f, &frame, compare);
                if (result != FullMemoryAssistResult.Retry || retry == 3) break;
            }
            Check(result == FullMemoryAssistResult.Resolved && d.State == FullStackMemoryState.Ready && d.Ticket == 0 &&
                d.Request0.Host0 != null && d.Request1.Host0 != null &&
                d.Request0.Length0 == 1 && d.Request1.Length0 == 1 &&
                (width == 8 || d.Request0.Host1 != null && d.Request1.Host1 != null),
                "R207 CMPS read pair/shared source private write resolves complete colliding descriptor");
        }
        var stack = StackMemoryRequest(source + 4095, dest + 4095);
        var s = StackMemoryMmuRequest(&stack, tlb);
        Check(FillFullMemoryMmu(f, &s) == FullMemoryAssistResult.Resolved && stack.State == FullStackMemoryState.Ready &&
            stack.Request0.Host1 != null && stack.Request1.Host1 != null, "R207 stack source read/private destination retains four checked pointers");
        // BOTH shared CMPS operands: independent leases and shared writes stay ticketed.
        ulong second = dest + 0x10000; OwnedRead207Map(f, second);
        foreach (bool compare in new[] { false, true })
        {
            var d = StringPairRequest(source + 4095, second + 4095, 64, compare);
            var frame = StackMemoryMmuRequest(&d, tlb);
            Check(OwnedRead207PairFill(f, &frame, compare) == (compare ? FullMemoryAssistResult.Resolved : FullMemoryAssistResult.WriteBarrier) &&
                d.State == (compare ? FullStackMemoryState.Ready : FullStackMemoryState.Request),
                "R207 two shared reads retain; shared destination Write keeps ticket and no Ready publication");
        }
    }

    static void OwnedRead207Controller()
    {
        Cache181Form[] forms = [
            new("scalar8", [0x8a,0x06,0x88,0x07], 2, 1),
            new("scalar16", [0x66,0x8b,0x06,0x66,0x89,0x07], 2, 2),
            new("scalar32", [0x8b,0x06,0x89,0x07], 2, 4),
            new("scalar64", [0x48,0x8b,0x06,0x48,0x89,0x07], 2, 8),
            new("vector128", [0xf3,0x0f,0x6f,0x06,0xf3,0x0f,0x7f,0x07], 2, 16),
            new("movsq", [0x48,0xa5], 1, 8, String:true),
            new("push-pop", [0xff,0x36,0x8f,0x07], 2, 8, Stack:true)
        ];
        foreach (var form in forms)
        foreach (int offset in new[] { 64, 4096 - form.Width, 4095 })
        {
            using var f = new Read207Fixture(); const ulong pc = Read207Fixture.Base, src = pc + 0x20000, dst = pc + 0x40000;
            f.Map(pc); f.Fill(pc); OwnedRead207Map(f, src); f.Map(dst, 8192); f.Fill(dst, 8192);
            WriteControllerCode(f, pc, [..form.Bytes, ..form.Bytes, 0xeb,0xfe]);
            Write64((byte*)f.Machine(), 72, src + (uint)offset); Write64((byte*)f.Machine(), 80, dst + (uint)offset);
            ulong rsp = dst + 256; Write64((byte*)f.Machine(), 56, rsp); f.Machine()->flags &= ~0x400u;
            Action<nint> cap = p => ((FullBlockFrame*)p)->NativeLimit = 1;
            f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap;
            ulong count = (ulong)(form.Operations * 2); f.Counts(count);
            using var deadline = f.AtomicDeadline();
            try
            {
                Check(f.Step(pc), "R207 real controller starts owned shared load");
                for (int attempt = 0; f.Completed < count && attempt < 16; ++attempt) Check(f.Continue(), "R207 same controller resumes");
                Check(f.Issued == count && f.Completed == count && f.Statistics.NativeMemoryInstructions == (long)count &&
                    f.Statistics.NativeMemoryAssistOutcomes["UnsupportedBacking"] == 0 &&
                    f.Statistics.NativeMemoryAssistOutcomes["WriteBarrier"] == 0 &&
                    RepRegister(f, 56) == rsp, "R207 every owned read/private write performs actual native work without shared tickets");
                f.DataScope(0, () =>
                {
                    for (int i = 0; i < (form.String ? 2 * form.Width : form.Width); ++i)
                        Check(*f.Program.LookupAddress2(f.Machine(), (long)(dst + (uint)(offset + i)), 0, 0) == 0x35,
                            "R207 generated scalar/vector/stack/string exact bytes");
                });
                RepClean(f);
            }
            finally { f.NativePrepared = null; f.NativeReturned = null; }
        }
        foreach (byte[] op in new byte[][] { [0x48,0x89,0x06], [0x48,0x01,0x06] })
        {
            using var f = new Read207Fixture(); const ulong pc = Read207Fixture.Base, data = pc + 0x20000;
            f.Map(pc); f.Fill(pc); OwnedRead207Map(f, data); WriteControllerCode(f, pc, [..op,0xeb,0xfe]);
            Write64((byte*)f.Machine(),72,data); Write64((byte*)f.Machine(),24,1); f.Counts(1);
            using var deadline = f.AtomicDeadline();
            Check(f.Step(pc, signalActor:true) && f.Issued == 1 && f.Completed == 1 &&
                f.Statistics.NativeMemoryAssistOutcomes["UnsupportedBacking"] > 0,
                "R207 actual shared store/RMW keeps existing conservative ticket path");
            var entry = f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data, FullNativeMemory.TlbCount - 1)];
            Check((entry.Permissions & FullMemoryAccess.Write) == 0, "R207 ticket completion never installs shared Write retention");
            RepClean(f);
        }
    }

    static void OwnedRead207Rep()
    {
        foreach (byte opcode in new byte[] { 0xa4, 0xac, 0xa6, 0xae })
        foreach (int width in new[] { 8, 16, 32, 64 })
        foreach (bool repeat in new[] { false, true })
        {
            if (!repeat && opcode == 0xac) continue; // scalar LODS covered by REP single/many below
            using var f = new Read207Fixture(); const ulong pc = Read207Fixture.Base, src = pc + 0x20000, dst = pc + 0x40000;
            f.Map(pc); f.Fill(pc); OwnedRead207Map(f, src); OwnedRead207Map(f, dst);
            // MOVS private destination is independently owned and writable.
            if (opcode == 0xa4)
            {
                Check(f.Mutate("MAP_FIXED",dst) == 0 && f.Mutate("MAP_FIXED",dst + 4096) == 0,
                    "R207 REP private destination on both crossing pages");
                f.Fill(dst,8192);
            }
            byte[] bytes = RepLoweringBytes(opcode, width, false, 0, 0xf3);
            if (!repeat) bytes = bytes[1..]; // remove the single F3 repeat prefix
            WriteControllerCode(f, pc, [..bytes,0xeb,0xfe]);
            int count = repeat ? 3 : 1;
            ulong ax = width == 64 ? 0x3535353535353535UL : 0x3535353535353535UL & ((1UL << width) - 1);
            RepSeed(f, src + 4095, dst + 4095, ax, (ulong)count, 0x202); f.Counts(1);
            using var deadline = f.AtomicDeadline();
            Check(f.Step(pc, signalActor:true) && f.Issued == 1 && f.Completed == 1 && f.Machine()->ip == pc + (uint)bytes.Length &&
                f.Statistics.NativeMemoryAssistOutcomes["UnsupportedBacking"] == 0 &&
                f.Statistics.NativeMemoryAssistOutcomes["WriteBarrier"] == 0,
                "R207 ordinary/REP LODS/MOVS/CMPS/SCAS uses owned shared reads through native RET");
            if (opcode is 0xa4 or 0xac or 0xa6) Check(RepRegister(f,72) == src + 4095 + (ulong)(count * width / 8), "R207 exact source index");
            if (opcode != 0xac) Check(RepRegister(f,80) == dst + 4095 + (ulong)(count * width / 8), "R207 exact destination index");
            if (repeat) Check(RepRegister(f,32) == 0 && f.Statistics.NativeRepIterations == count, "R207 REP count/elements complete once");
            if (opcode is 0xa6 or 0xae) Check((f.Machine()->flags & 64) != 0, "R207 final native comparison flags reflect matching shared bytes");
            if (opcode == 0xa4) f.DataScope(0, () =>
            {
                for (int i = 0; i < count * width / 8; ++i)
                    Check(*f.Program.LookupAddress2(f.Machine(), (long)(dst + 4095 + (uint)i), 0, 0) == 0x35,
                        "R207 REP generated stores copy shared source bytes");
            });
            RepClean(f);
        }
    }

    static void OwnedRead207Topology()
    {
        foreach (string mutation in new[] { "write", "truncate", "EOF", "mprotect", "munmap", "MAP_FIXED", "unbind", "release", "role" })
        {
            using var f = new Read207Fixture(); const ulong data = Read207Fixture.Base;
            int fd = OwnedRead207Map(f, data);
            var before = Mapping193Seed(f, data);
            Mapping193Pulse(f, "cpu");
            Check(Mapping193Prepare(f), "R207 runtime-only retag retains exact checked shared read");
            var retained = f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data,63)];
            Check(retained.Host == before.Host && retained.Identity == before.Identity && retained.Permissions == FullMemoryAccess.Read &&
                retained.AuthorityEpoch == f.Jit.Authority.Epoch, "R207 R193 retag preserves read rights and identity");
            ulong epoch = f.Jit.Authority.Epoch;
            switch (mutation)
            {
                case "write": Check(f.Io.WriteAt(fd, "x"u8, 0).Value == 1, "R207 host write"); break;
                case "truncate": Check(f.Io.TruncateFile(fd,1).Succeeded, "R207 partial-page truncate"); break;
                case "EOF": Check(f.Io.TruncateFile(fd,0).Succeeded, "R207 EOF"); break;
                case "unbind": f.Io.UnbindMemoryFileMutation(f.Jit); f.Io.BindMemoryFileMutation(f.Jit); break;
                case "release":
                    var extra = f.Io.AcquireMemoryFilePage(fd,0).Value;
                    Check(InstanceIo.ReleaseMemoryFilePage(extra.Lease).Succeeded &&
                        !f.Io.MemoryFileSharedReadOwned(extra.Lease, extra.Address, f.Jit.Authority), "R207 released lease cannot authenticate"); break;
                case "role":
                    f.Jit.BeginRead(f.Program,false);
                    try { lock (f.Jit.Authority.Gate) Check(f.Jit.Authority.PrepareCodeSnapshot((nint)before.Host), "R207 physical code alias registration"); }
                    finally { f.Jit.EndRead(f.Program); } break;
                default: Check(f.Mutate(mutation,data) == 0, "R207 real mapping mutation"); break;
            }
            Check(f.Jit.Authority.Epoch != epoch, "R207 every content/mapping/ownership change advances actual runtime authority");
            Check(Mapping204Prepare(f, out _), "R207 mapping cache reopens at current provenance");
            var after = f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data,63)];
            Check(mutation == "role" ? after.Host == before.Host && after.Permissions == FullMemoryAccess.Read : after.Host == null,
                "R207 R204 keeps pure read at first-Code only; real mutation discards all old shared pointers");
            var r = Cache181Request(f.MappingState193.Tlb,data);
            var result = FillFullMemoryMmu(f,&r);
            if (mutation is "EOF" or "mprotect" or "munmap")
                Check(result == FullMemoryAssistResult.GuestFault && r.FaultCode == (mutation == "EOF" ? unchecked((uint)-2) : mutation == "mprotect" ? 2u : 1u),
                    "R207 refills precise EOF/protection/unmap faults");
            else if (mutation != "MAP_FIXED")
                Check(result == FullMemoryAssistResult.Resolved && f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data,63)].Permissions == FullMemoryAccess.Read,
                    "R207 fresh read never upgrades Write after mutation/physical code alias");
            if (mutation == "truncate") Check(before.Host[1] == 0, "R207 removed tail bytes zero before regrow");
        }
        // C-owner retirement keeps the real lease alive under a passive reader,
        // removes the mapping immediately, then releases only after EndRead.
        using var pin = new Read207Fixture(); const ulong address = Read207Fixture.Base;
        OwnedRead207Map(pin,address); nint physical = pin.Physical(address);
        ulong token = pin.Program.BlinkHostMemoryFilePageLease((void*)physical);
        pin.Jit.BeginRead(pin.Program,false);
        try
        {
            Check(pin.Mutate("munmap",address) == 0 && pin.Io.MemoryFileSharedReadOwned(token, physical, pin.Jit.Authority),
                "R207 retired mapping retains active lease until passive reader exits");
        }
        finally { pin.Jit.EndRead(pin.Program); }
        Check(!pin.Io.MemoryFileSharedReadOwned(token,physical,pin.Jit.Authority), "R207 retired C backing lease released after final reader");
    }

    static void OwnedRead207Drain()
    {
        foreach (string mutation in new[] { "write", "truncate", "unbind", "dispose", "release" })
        {
            using var f = new Read207Fixture(); const ulong pc = Read207Fixture.Base, data = pc + 0x20000;
            f.Map(pc); f.Fill(pc); int fd = OwnedRead207Map(f,data);
            WriteControllerCode(f,pc,[0x48,0x8b,0x06,0xeb,0xfe]); Write64((byte*)f.Machine(),72,data); f.Counts(1);
            nint physical = f.Physical(data); var extra = f.Io.AcquireMemoryFilePage(fd,0).Value;
            bool released = false, unbound = false; Exception? failure = null; Thread? writer = null;
            using var finished = new ManualResetEventSlim();
            f.NativeReturned = pointer =>
            {
                var frame = (FullBlockFrame*)pointer;
                if (frame->NativeMemoryComputations == 0) return;
                f.NativeReturned = null;
                Check(frame->Executed == 1 && RepRegister(f,24) == 0x3535353535353535UL &&
                    f.Jit.Store.GetStatistics().ActiveNativeEpochs > 0 && f.Statistics.ActiveDataBorrows > 0,
                    "R207 actual load returned with real store/native/passive enrollment still held");
                ulong old = f.Jit.Authority.Epoch;
                writer = new Thread(() =>
                {
                    try
                    {
                        switch (mutation)
                        {
                            case "write": if (f.Io.WriteAt(fd,"x"u8,0).Value != 1) throw new InvalidOperationException("write"); break;
                            case "truncate": if (!f.Io.TruncateFile(fd,0).Succeeded) throw new InvalidOperationException("truncate"); break;
                            case "unbind": f.Io.UnbindMemoryFileMutation(f.Jit); unbound = true; break;
                            case "dispose": f.Io.DisposeAsync().AsTask().GetAwaiter().GetResult(); break;
                            case "release": if (!InstanceIo.ReleaseMemoryFilePage(extra.Lease).Succeeded) throw new InvalidOperationException("release"); released = true; break;
                        }
                    }
                    catch (Exception error) { failure = error; }
                    finally { finished.Set(); }
                }) { IsBackground = true };
                writer.Start();
                Check(SpinWait.SpinUntil(() => f.Jit.Authority.HostWriters != 0 && f.Jit.Authority.Epoch != old, TimeSpan.FromSeconds(5)) && !finished.IsSet && *((byte*)physical) == 0x35,
                    "R207 pending actual host mutation closes admission before bytes/ownership/lease effects");
                bool admitted = f.Jit.Authority.TryEnterNative(f.Jit.Authority.Epoch);
                if (admitted) f.Jit.Authority.ExitNative();
                Check(!admitted, "R207 even current-epoch admission denied during host writer drain");
                // Return through production ExitNative; no IO, authority or C
                // lifetime lock is acquired here while the writer owns IO.sync.
            };
            using var deadline = f.AtomicDeadline();
            try
            {
                Check(f.Step(pc,signalActor:true), "R207 real native shared read before concurrent mutation");
                Check(writer != null && finished.Wait(TimeSpan.FromSeconds(5)) && writer.Join(TimeSpan.FromSeconds(5)),
                    "R207 actual writer completes only after production native exit");
                if (failure != null) throw new InvalidOperationException("R207 host mutation",failure);
                Check(f.Issued == 1 && f.Completed == 1 && RepRegister(f,24) == 0x3535353535353535UL,
                    "R207 loaded value and same approved issue survive subsequent host mutation");
                if (mutation == "write") Check(*((byte*)physical) == (byte)'x', "R207 writer publishes after native drain");
                if (mutation == "truncate") Check(!InstanceIo.MemoryFilePageAvailable(extra.Lease), "R207 EOF publishes after native drain");
                RepClean(f);
            }
            finally
            {
                f.NativeReturned = null;
                if (writer != null && !writer.Join(TimeSpan.FromSeconds(5))) throw new TimeoutException("R207 writer not quiescent");
                if (unbound) f.Io.BindMemoryFileMutation(f.Jit);
                if (!released) Check(InstanceIo.ReleaseMemoryFilePage(extra.Lease).Succeeded, "R207 extra lease cleanup");
            }
        }
    }

    static void OwnedRead207Boundaries()
    {
        foreach (string gate in new[] { "stop", "cancel", "budget", "last", "attention", "epoch", "EOF", "secondEOF", "protection" })
        {
            using var f = new Read207Fixture(); const ulong pc = Read207Fixture.Base, data = pc + 0x20000;
            f.Map(pc); f.Fill(pc); int fd = OwnedRead207Map(f,data);
            WriteControllerCode(f,pc,[0x48,0x8b,0x06,0xeb,0xfe]);
            Write64((byte*)f.Machine(),72,gate == "secondEOF" ? data + 4095 : data);
            FullRax(f,0xfeedface); f.Counts(gate == "budget" ? 0UL : 7UL,6,6);
            int preissues = 0;
            using var cancellation = new CancellationTokenSource();
            using var cancellationRegistration = cancellation.Token.Register(() => f.Stop.RequestStop());
            f.Preissued = (_, _) =>
            {
                ++preissues;
                if (gate == "stop") f.Stop.RequestStop();
                if (gate == "cancel") cancellation.Cancel();
                if (gate == "attention") f.Machine()->attention = true;
            };
            if (gate == "EOF") Check(f.Io.TruncateFile(fd,0).Succeeded,"R207 boundary EOF setup");
            if (gate == "secondEOF") Check(f.Io.TruncateFile(fd,4096).Succeeded,"R207 boundary second EOF setup");
            if (gate == "protection") Check(f.Mutate("mprotect",data) == 0,"R207 boundary no-access setup");
            if (gate == "epoch")
            {
                f.NativePrepared = _ => { f.Jit.BeginMemoryFileMutation(0,0,true); f.Jit.EndMemoryFileMutation(); };
            }
            using var deadline = f.AtomicDeadline();
            try
            {
                if (gate is "EOF" or "secondEOF" or "protection")
                {
                    var fault = f.StepSynchronousFault(pc);
                    Check(fault.Signal == (gate == "protection" ? 11 : 7) && f.Issued == 7 && f.Completed == 6 &&
                        FullRax(f) == 0xfeedface && f.Machine()->ip == pc,
                        "R207 owned shared first/second EOF/protection fault has no partial native effect");
                }
                else
                {
                    bool stepped = f.Step(pc,signalActor:true);
                    if (gate is "stop" or "cancel")
                        Check(stepped && preissues == 1 && f.Issued == 7 && f.Completed == 6 && FullRax(f) == 0xfeedface,
                            "R207 stopped/canceled approved shared read never enters data computation");
                    else if (gate == "budget")
                        Check(!stepped && preissues == 0 && f.Issued == 7 && f.Completed == 6 && FullRax(f) == 0xfeedface,
                            "R207 budget rejection precedes shared-read mapping/native effects");
                    else
                        Check(stepped && preissues == 1 && f.Issued == 7 && f.Completed == 7 && FullRax(f) == 0x3535353535353535UL,
                            "R207 shared-read cold miss/attention/epoch retry preserves last allowed same issue");
                }
                var state = f.Jit.Store.GetStatistics();
                Check(state.ActiveManagedLeases == 0 && state.ActiveNativeEpochs == 0 && f.Statistics.ActiveDataBorrows == 0 &&
                    f.Jit.Authority.PendingAtomicWorker == -1, "R207 boundary cleanup retains no native/read/reservation capability");
            }
            finally { f.Preissued = null; f.NativePrepared = null; }
        }
    }

    static void OwnedRead207FreshValues()
    {
        using var f = new Read207Fixture(); const ulong pc = Read207Fixture.Base, data = pc + 0x20000, alias = pc + 0x40000;
        f.Map(pc); f.Fill(pc); int fd = OwnedRead207Map(f,data);
        f.AtomicMapSharedPage(alias,fd,0); f.DataScope(0,() => f.Expose(alias,0));
        Check(f.Physical(data) == f.Physical(alias), "R207 real physical aliases share one leased memfd page");
        WriteControllerCode(f,pc,[0x48,0x8b,0x06,0xeb,0xfe]);
        using var deadline = f.AtomicDeadline();
        long completed = 0;
        foreach (string mutation in new[] { "initial", "hostWrite", "truncate", "regrow", "aliasWrite" })
        {
            if (mutation == "hostWrite") Check(f.Io.WriteAt(fd,"x"u8,0).Value == 1,"R207 replace first byte through host writer");
            if (mutation == "truncate") Check(f.Io.TruncateFile(fd,1).Succeeded,"R207 zero retained tail on truncate");
            if (mutation == "regrow") Check(f.Io.TruncateFile(fd,8192).Succeeded,"R207 sparse regrow after truncate");
            if (mutation == "aliasWrite") f.DataScope(0,() => *f.Program.LookupAddress2(f.Machine(),(long)alias,0,0) = (byte)'z');
            Write64((byte*)f.Machine(),72,alias); f.Counts((ulong)(completed + 1),completed,completed);
            Check(f.Step(pc,signalActor:true),"R207 reload through second physical alias");
            ++completed;
            ulong expected = mutation == "initial" ? 0x3535353535353535UL : mutation == "hostWrite" ? 0x3535353535353578UL :
                mutation == "aliasWrite" ? (byte)'z' : (byte)'x';
            Check(f.Issued == (ulong)completed && f.Completed == (ulong)completed && FullRax(f) == expected,
                "R207 cached native read sees current hostWrite/truncate/regrow/alias bytes, never stale tail");
        }
        RepClean(f);
    }

    static void OwnedRead207StackControl()
    {
        foreach (bool call in new[] { false, true })
        {
            using var f = new Read207Fixture(); const ulong pc = Read207Fixture.Base, data = pc + 0x20000, stack = pc + 0x40000;
            f.Map(pc); f.Fill(pc); int fd = OwnedRead207Map(f,data); f.Map(stack); f.Fill(stack);
            WriteControllerCode(f,pc,call ? [0xff,0x16,0xeb,0xfe] : [0xc9,0xeb,0xfe]);
            ulong target = pc + 64, rsp = stack + 256;
            if (call) Check(f.Io.WriteAt(fd,BitConverter.GetBytes(target),0).Value == 8,"R207 observed indirect target bytes");
            Write64((byte*)f.Machine(),72,data); Write64((byte*)f.Machine(),56,rsp); Write64((byte*)f.Machine(),64,data);
            f.Counts(1); using var deadline = f.AtomicDeadline();
            Check(f.Step(pc,signalActor:true) && f.Issued == 1 && f.Completed == 1 &&
                f.Statistics.NativeMemoryInstructions == 1 && f.Statistics.NativeMemoryAssistOutcomes["WriteBarrier"] == 0 &&
                f.Statistics.NativeMemoryAssistOutcomes["UnsupportedBacking"] == 0,
                "R207 actual LEAVE/near CALL retains shared Read with existing complete stack grammar");
            Check(f.Machine()->ip == (call ? target : pc + 1) && RepRegister(f,56) == (call ? rsp - 8 : data + 8),
                "R207 native stack/control target and RSP commit exactly once");
            if (!call) Check(RepRegister(f,64) == 0x3535353535353535UL,"R207 LEAVE reads shared saved RBP");
            else f.DataScope(0,() =>
            {
                byte* p = f.Program.LookupAddress2(f.Machine(),(long)(rsp - 8),0,0);
                Check(*(ulong*)p == pc + 2,"R207 near CALL private stack contains native return address");
            });
            RepClean(f);
        }
    }
}

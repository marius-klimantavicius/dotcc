using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Cache181Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // ONE parent entry. AUTHORED_UNRUN: no build, emission, MMU or native work
    // was performed by the implementation agent. Counts are not a speed score.
    static void CheckFullNativeMemoryCacheR181(byte* machine)
    {
        Check(FullNativeMemory.TlbCount == 256 && sizeof(FullMemoryTlbEntry) == 64 &&
            FullBlockFrame.ByteSize == 416 && FullBlockFrame.AbiVersion == 0x46420007,
            "R181 fixed cache/frame ABI");
        Cache181IndexTable();
        Cache181Encoding();
        Cache181NativeHits(machine);
        using (var scaffold = CreateFullScaffold(out var addresses))
        {
            fullOwnerScaffold = addresses;
            try { Cache181NativeHits(machine); }
            finally { fullOwnerScaffold = default; }
        }
        CheckFullMemoryMissAndPermissions(machine);
        Cache181RefillTable();
        Cache181AuthorityDenials();
        Cache181MmuBoundaries();
        Cache181IndependentRequests();
        Cache181ControllerTable();
        Cache181ControllerRetainedWrite();
        Cache181ControllerRevocation();
        // The existing observer suite is included in this entry after its
        // page-zero collision fixture is updated for the new index.
        CheckFullMemoryMissDiagnostics();
    }

    static void Cache181IndexTable()
    {
        // Independent arithmetic oracle, including all accepted masks. This
        // also models the explicitly 32-bit XOR/AND in the native encoding.
        ulong[] pages = [0, 1, 31, 32, 63, 64, 65, 2047, 2048, 4095, 4096,
            0x7ffffffff, 0xffff800000000, 0xffffffffffffe, 0xfffffffffffff];
        foreach (uint mask in new uint[] { 1, 3, 7, 15, 31, 63, 127, 255, 511, 1023, 2047, 4095 })
        foreach (ulong page in pages)
        foreach (uint offset in new uint[] { 0, 1, 2047, 4095 })
        {
            ulong address = (page << 12) | offset;
            uint low = (uint)(address >> 12), folded = (uint)(address >> 17) & 0xfffffffeu;
            int expected = (int)((low ^ folded) & mask);
            Check(FullNativeMemory.TlbIndex(address, mask) == expected,
                "R181 native 32-bit arithmetic agrees with managed unsigned index");
            if (page != 0xfffffffffffff)
                Check(FullNativeMemory.TlbIndex(page << 12, mask) !=
                    FullNativeMemory.TlbIndex((page + 1) << 12, mask), "R181 adjacent fold-boundary pages differ");
        }
        foreach (ulong basePage in new ulong[] { 0, 0x200, 0x7ffffffc0, 0xffff800000000 })
        {
            var slots = new HashSet<int>();
            for (ulong i = 0; i < 32; ++i) slots.Add(FullNativeMemory.TlbIndex((basePage + i * 64) << 12, 63));
            Check(slots.Count == 32, "R181 thirty-two same-low-six page tags spread over thirty-two slots");
        }
        // The retained low bit is intentional: one cheap fold cannot disperse
        // every stride. A true collision is still checked by its exact page tag.
        Check(FullNativeMemory.TlbIndex(0, 63) == FullNativeMemory.TlbIndex(0x42000, 63),
            "R181 known residual collision is explicit");
    }

    static void Cache181Encoding()
    {
        byte[] needle = [0x48,0x89,0xc1,0x48,0xc1,0xe9,12,0x48,0xc1,0xe8,17,
            0x83,0xe0,0xfe,0x31,0xc1,0x41,0x23,0x8a,0x20,0x01,0,0,
            0x48,0xc1,0xe1,6,0x48,0x01,0xca,0x4c,0x89,0xc0,0x48,0x25,0,0xf0,0xff,0xff];
        byte[] buffer = new byte[FullBlockEmitter.CodeCapacity];
        FullBlockInstruction[] sites = [new(new(Ip,3,JitOperation.LoadMov,64,new(0,0),
            new(0,0,IsMemory:true),Address:new(3,-1,0,0)))];
        foreach (bool windows in new[] { false, true })
        {
            int length = FullBlockEmitter.Emit(sites, buffer, windowsAbi: windows);
            Check(buffer.AsSpan(0, length).IndexOf(needle) >= 0,
                "R181 both ABI encodings use logical SHR17/AND-2/XOR32 and restore RAX before page tag");
        }
    }

    static void Cache181NativeHits(byte* machine)
    {
        byte* pages = (byte*)NativeMemory.AllocZeroed(12288);
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach (ulong page in new ulong[] { 0x1f000, 0x3f000, 0x7ff000,
                0x7ffffffff000, 0xffff800000000000, 0xffffffffffffe000 })
            foreach (int width in new[] { 8, 16, 32, 64 })
            foreach (int offset in new[] { 16, 4095 })
            foreach (bool store in new[] { false, true })
            {
                // Skip the positive canonical edge when its final byte leaves
                // canonical space. Dedicated fault rows below cover that edge.
                if (!FullNativeMemory.Canonical(page + (uint)offset + (uint)(width / 8 - 1))) continue;
                new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
                new Span<byte>(pages, 12288).Fill(0xa5);
                SetFullMemoryEntry(tlb, page, pages);
                SetFullMemoryEntry(tlb, page + 4096, pages + 8192); // independent backing
                var i = new JitInstruction(Ip, 3, store ? JitOperation.StoreMov : JitOperation.LoadMov,
                    width, new(0, 0, IsMemory: store), new(1, 0, IsMemory: !store), Address: new(3, -1, 0, 0));
                using var code = CreateFullBlock([new(i)]);
                ResetFull(machine); ((ulong*)(machine + 48))[0] = page + (uint)offset;
                ((ulong*)(machine + 32))[0] = 0x8877665544332211;
                ulong epoch = 11; long issued = 0, completed = 0; int stop = 0;
                var h = FullHeader(Ip, code.Entry);
                var f = FullFrame(machine, &h, &epoch, &issued, &completed, &stop, limit: 1);
                f.MemoryTlb = tlb; f.MemoryTlbMask = 63;
                Check(RunFull(code, &f) == FullBlockExit.Continue && issued == 1 && completed == 1 &&
                    f.MemoryAssists == 0, "R181 actual compiled index hits high/boundary pages without a refill");
                ulong expected = 0;
                for (int b = 0; b < width / 8; ++b)
                {
                    byte* at = offset + b < 4096 ? pages + offset + b : pages + 8192 + offset + b - 4096;
                    if (store) Check(*at == (byte)(0x8877665544332211UL >> (8 * b)), "R181 exact split store bytes");
                    else expected |= (ulong)*at << (8 * b);
                }
                if (!store) Check((((ulong*)(machine + 24))[0] & (width == 64 ? ulong.MaxValue : (1UL << width) - 1)) == expected,
                    "R181 exact split read bytes");
                Check(pages[4096] == 0xa5 && pages[8191] == 0xa5, "R181 crossing never assumes contiguous host backing");
            }
        }
        finally { NativeMemory.Free(pages); }
    }

    static FullBlockFrame Cache181Request(FullMemoryTlbEntry* tlb, ulong address, FullMemoryAccess access = FullMemoryAccess.Read,
        uint width = 64) => new() { MemoryTlb = tlb, MemoryTlbMask = 63, MemoryResume = 1,
            MemoryAddress = address, MemoryAccess = access, MemoryWidth = width, Context = 0x181 };

    static void Cache181RefillTable()
    {
        string[] variants = ["exact", "page", "host", "identity", "context", "epoch", "read-only", "write-only",
            "atomic", "barrier", "ticket", "consumed", "start", "end", "fresh", "advanced"];
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        foreach (string variant in variants)
        {
            using var f = new Cache181Fixture();
            ulong page = Cache181Fixture.Base;
            f.Map(page); f.Fill(page);
            new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
            var frame = Cache181Request(tlb, page);
            Check(FillFullMemoryMmu(f, &frame) == FullMemoryAssistResult.Resolved, "R181 real read admission");
            int slot = FullNativeMemory.TlbIndex(page, 63);
            Check(tlb[slot].Permissions == FullMemoryAccess.Read, "R181 PTE_RW does not grant an initial write");
            frame.MemoryAccess = FullMemoryAccess.Write;
            Check(FillFullMemoryMmu(f, &frame) == FullMemoryAssistResult.Resolved &&
                tlb[slot].Permissions == (FullMemoryAccess.Read | FullMemoryAccess.Write), "R181 real write admission seed");
            ref var old = ref tlb[slot];
            switch (variant)
            {
                case "page": old.Page += 4096; break;
                case "host": old.Host = (byte*)1; break; // compared, never followed
                case "identity": old.Identity ^= 4096; break;
                case "context": old.Context ^= 1; break;
                case "epoch": --old.AuthorityEpoch; break;
                case "read-only": old.Permissions = FullMemoryAccess.Read; break;
                case "write-only": old.Permissions = FullMemoryAccess.Write; break;
                case "atomic": old.Permissions |= FullMemoryAccess.Atomic; break;
                case "barrier": old.Flags = FullMemoryFlags.WriteBarrier; break;
                case "ticket": old.Flags = FullMemoryFlags.BarrierTicket; break;
                case "consumed": old.Flags = FullMemoryFlags.BarrierConsumed; break;
                case "start": old.ValidStart = 1; break;
                case "end": old.ValidEnd = 4095; break;
                case "fresh": old = default; break;
                case "advanced": lock (f.Jit.Authority.Gate) f.Jit.Authority.Advance(); break;
            }
            frame.MemoryAccess = FullMemoryAccess.Read;
            Check(FillFullMemoryMmu(f, &frame) == FullMemoryAssistResult.Resolved &&
                tlb[slot].Permissions == (variant == "exact" ? FullMemoryAccess.Read | FullMemoryAccess.Write : FullMemoryAccess.Read),
                "R181 exact identity required to retain Write: " + variant);
            Check(tlb[slot].AuthorityEpoch == f.Jit.Authority.Epoch && tlb[slot].Context == frame.Context &&
                tlb[slot].Identity == (ulong)tlb[slot].Host && tlb[slot].Flags == 0 && tlb[slot].ValidEnd == 4096 &&
                f.Jit.Authority.LastDataPermissionDenial == FullDataPermissionDenial.None,
                "R181 refill replaces hints with freshly checked identity");
        }
    }

    static void Cache181AuthorityDenials()
    {
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        foreach (string denial in new[] { "physical", "code", "host", "mapping", "copy" })
        {
            using var f = new Cache181Fixture();
            ulong page = Cache181Fixture.Base;
            f.Map(page); f.Fill(page);
            new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
            var frame = Cache181Request(tlb, page);
            Check(FillFullMemoryMmu(f, &frame) == FullMemoryAssistResult.Resolved, "R181 denial seed read");
            frame.MemoryAccess = FullMemoryAccess.Write;
            Check(FillFullMemoryMmu(f, &frame) == FullMemoryAssistResult.Resolved, "R181 denial seed admitted write");
            int slot = FullNativeMemory.TlbIndex(page, 63), worker = f.WorkerId(1);
            var authority = f.Jit.Authority;
            lock (authority.Gate)
            {
                switch (denial)
                {
                    case "physical": authority.Expose(worker, (nint)tlb[slot].Host); break;
                    case "code": Check(authority.PrepareCodeSnapshot((nint)tlb[slot].Host), "R181 code alias registered"); break;
                    case "host": authority.BeginHostWriter(); break;
                    case "mapping": authority.BeginMappingMutation(); authority.Advance(); break;
                    case "copy": authority.BeginGuestWrite(worker); break;
                }
            }
            try
            {
                // Test-only forged *current* epoch makes the optional write
                // predicate itself decisive, independently of stale-tag checks.
                tlb[slot].AuthorityEpoch = authority.Epoch;
                frame.MemoryAccess = FullMemoryAccess.Read;
                var result = FillFullMemoryMmu(f, &frame);
                if (denial is "physical" or "code")
                    Check(result == FullMemoryAssistResult.Resolved && tlb[slot].Permissions == FullMemoryAccess.Read &&
                        authority.LastDataPermissionDenial == FullDataPermissionDenial.None,
                        "R181 fresh physical/code denial demotes optional write without failing a read");
                else
                {
                    bool admitted = authority.TryEnterNative(authority.Epoch);
                    if (admitted) authority.ExitNative();
                    Check(result == FullMemoryAssistResult.Retry && !admitted,
                        "R181 global writer denies refill and native admission");
                }
            }
            finally
            {
                switch (denial)
                {
                    case "physical": authority.EndBorrow(worker); break;
                    case "host": authority.EndHostWriter(); break;
                    case "mapping": authority.EndMappingMutation(); break;
                    case "copy": authority.EndGuestWrite(worker); break;
                }
            }
            if (denial != "code")
            {
                frame.MemoryAccess = FullMemoryAccess.Read;
                Check(FillFullMemoryMmu(f, &frame) == FullMemoryAssistResult.Resolved &&
                    tlb[slot].Permissions == FullMemoryAccess.Read, "R181 revoked epoch never resurrects a previous write");
            }
        }
    }

    static void Cache181MmuBoundaries()
    {
        // Existing actual MMU witnesses: demand page, second-page MAPERR,
        // PTE permission loss, MAP_FIXED, shared backing and EOF BUS_ADRERR.
        CheckFullMemoryMmu();
        ulong page = Cache181Fixture.Base;
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        using (var f = new Cache181Fixture())
        {
            f.Map(page, 8192); f.Fill(page, 8192);
            new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
            var frame = Cache181Request(tlb, page);
            Check(FillFullMemoryMmu(f, &frame) == FullMemoryAssistResult.Resolved, "R181 crossing seed read");
            frame.MemoryAccess = FullMemoryAccess.Write;
            Check(FillFullMemoryMmu(f, &frame) == FullMemoryAssistResult.Resolved, "R181 crossing seed write");
            frame.MemoryAccess = FullMemoryAccess.Read; frame.MemoryAddress = page + 4095;
            Check(FillFullMemoryMmu(f, &frame) == FullMemoryAssistResult.Resolved &&
                tlb[FullNativeMemory.TlbIndex(page, 63)].Permissions == (FullMemoryAccess.Read | FullMemoryAccess.Write) &&
                tlb[FullNativeMemory.TlbIndex(page + 4096, 63)].Permissions == FullMemoryAccess.Read,
                "R181 real cross-page read preserves admitted first-page Write but never grants second-page Write");
            foreach (ulong address in new ulong[] { 1UL << 47, (1UL << 47) - 1, ulong.MaxValue })
            {
                frame.MemoryAddress = address; frame.MemoryAccess = FullMemoryAccess.Write;
                var before = tlb[FullNativeMemory.TlbIndex(page, 63)];
                Check(FillFullMemoryMmu(f, &frame) == FullMemoryAssistResult.GuestFault && frame.FaultTrap == 14 &&
                    frame.FaultCode == 1 && frame.FaultAddress == address &&
                    new ReadOnlySpan<byte>(&before, 64).SequenceEqual(new ReadOnlySpan<byte>(tlb + FullNativeMemory.TlbIndex(page, 63), 64)),
                    "R181 MMU canonical/overflow rejection precedes lookup and publication");
            }
        }
        foreach (string mutation in new[] { "mprotect", "MAP_FIXED", "munmap" })
        {
            using var mapped = new Cache181Fixture();
            mapped.Map(page); mapped.Fill(page);
            new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
            var frame = Cache181Request(tlb, page);
            Check(FillFullMemoryMmu(mapped, &frame) == FullMemoryAssistResult.Resolved, "R181 mapping seed");
            frame.MemoryAccess = FullMemoryAccess.Write;
            Check(FillFullMemoryMmu(mapped, &frame) == FullMemoryAssistResult.Resolved, "R181 mapping write seed");
            if (mutation == "mprotect")
                mapped.DataScope(0, () => Check(mapped.Program.ProtectVirtual(mapped.Machine()->blink_host_system,
                    (long)page, 4096, 1, false) == 0, "R181 actual PROT_READ keeps read and removes write"));
            else Check(mapped.Mutate(mutation, page) == 0, "R181 real mapping mutation");
            if (mutation == "MAP_FIXED") mapped.Fill(page);
            frame.MemoryAccess = FullMemoryAccess.Read;
            var result = FillFullMemoryMmu(mapped, &frame);
            if (mutation == "munmap") Check(result == FullMemoryAssistResult.GuestFault && frame.FaultCode == 1, "R181 unmap rejects old entry");
            else Check(result == FullMemoryAssistResult.Resolved && tlb[FullNativeMemory.TlbIndex(page, 63)].Permissions == FullMemoryAccess.Read,
                "R181 readonly/remapped page cannot inherit Write");
        }
    }

    static ulong Cache181Collision(ulong page)
    {
        for (ulong delta = 1; delta < 8192; ++delta)
        {
            ulong candidate = page + delta * 4096;
            if (FullNativeMemory.TlbIndex(candidate, 63) == FullNativeMemory.TlbIndex(page, 63) &&
                FullNativeMemory.TlbIndex(candidate + 4096, 63) == FullNativeMemory.TlbIndex(page + 4096, 63)) return candidate;
        }
        throw new InvalidOperationException("R181 collision fixture search exhausted");
    }

    static void Cache181IndependentRequests()
    {
        using var f = new Cache181Fixture();
        ulong source = Cache181Fixture.Base, destination = Cache181Collision(source);
        f.Map(source, 8192); f.Fill(source, 8192); f.Map(destination, 8192); f.Fill(destination, 8192);
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
        foreach (bool strings in new[] { false, true })
        {
            var d = strings ? StringPairRequest(source + 4095, destination + 4095, 64, false) :
                StackMemoryRequest(source + 4095, destination + 4095);
            var frame = StackMemoryMmuRequest(&d, tlb);
            FullMemoryAssistResult result;
            if (strings)
            {
                f.Jit.BeginRead(f.Program, false);
                try
                {
                    f.Program.ResetTlb(f.Machine());
                    lock (f.Jit.Authority.Gate)
                    {
                        frame.ObservedGlobalEpoch = f.Jit.Authority.Epoch;
                        result = FullNativeMemory.TryFillStringPair(f.Program, f.Machine(), &frame, f.Jit.Authority, false);
                    }
                }
                finally { f.Jit.EndRead(f.Program); }
            }
            else result = FillFullMemoryMmu(f, &frame);
            Check(result == FullMemoryAssistResult.Resolved && d.State == FullStackMemoryState.Ready &&
                d.Request0.Host0 != null && d.Request0.Host1 != null && d.Request1.Host0 != null && d.Request1.Host1 != null &&
                d.Request0.Host0 != d.Request1.Host0 && d.Request0.Host1 != d.Request1.Host1 &&
                d.Request0.Length0 == 1 && d.Request1.Length0 == 1 &&
                tlb[FullNativeMemory.TlbIndex(source, 63)].Page == destination,
                "R181 real colliding two-access MMU retains four independent pointers despite last TLB publication");
        }
    }

    sealed record Cache181Form(string Name, byte[] Bytes, int Operations, int Width, bool String = false, bool Stack = false);

    static void Cache181ControllerTable()
    {
        Cache181Form[] forms = [
            new("scalar8", [0x8a,0x06,0x88,0x07], 2, 1),
            new("scalar16", [0x66,0x8b,0x06,0x66,0x89,0x07], 2, 2),
            new("scalar32", [0x8b,0x06,0x89,0x07], 2, 4),
            new("scalar64", [0x48,0x8b,0x06,0x48,0x89,0x07], 2, 8),
            new("vector128", [0xf3,0x0f,0x6f,0x06,0xf3,0x0f,0x7f,0x07], 2, 16),
            new("mmx64", [0x0f,0x6f,0x06,0x0f,0x7f,0x07], 2, 8),
            new("movsq", [0x48,0xa5], 1, 8, String:true),
            new("stack-pair", [0xff,0x36,0x8f,0x07], 2, 8, Stack:true)
        ];
        foreach (var form in forms)
        foreach (bool collision in new[] { false, true })
        foreach (bool crossing in new[] { false, true })
        {
            using var f = new Cache181Fixture();
            ulong pc = Cache181Fixture.Base, source = pc + 0x20000;
            ulong destination = collision ? Cache181Collision(source) : source + 0x40000;
            f.Map(pc); f.Fill(pc); f.Map(source, 8192); f.Fill(source, 8192); f.Map(destination, 8192); f.Fill(destination, 8192);
            var bytes = Enumerable.Range(0, 8).SelectMany(_ => form.Bytes).ToArray();
            WriteControllerCode(f, pc, [..bytes, 0xeb, 0xfe]);
            int offset = crossing ? 4095 : 64;
            byte[] expected = Enumerable.Range(0, form.String ? 64 : form.Width).Select(i => (byte)(17 + i * 3)).ToArray();
            f.DataScope(0, () =>
            {
                for (int i = 0; i < expected.Length; ++i)
                {
                    byte* a = f.Program.LookupAddress2(f.Machine(), (long)(source + (uint)(offset + i)), 0, 0);
                    byte* b = f.Program.LookupAddress2(f.Machine(), (long)(destination + (uint)(offset + i)), 0, 0);
                    Check(a != null && b != null, "R181 controller mapped test byte"); *a = expected[i]; *b = 0;
                }
            });
            byte* m = (byte*)f.Machine();
            ((ulong*)(m + 72))[0] = source + (uint)offset; ((ulong*)(m + 80))[0] = destination + (uint)offset;
            ulong rsp = destination + 256; ((ulong*)(m + 56))[0] = rsp;
            f.Machine()->flags &= ~0x400u; // forward ordinary MOVS
            uint flags = f.Machine()->flags;
            Action<nint> cap = p => ((FullBlockFrame*)p)->NativeLimit = 1;
            f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap;
            ulong count = (ulong)(form.Operations * 8);
            f.Counts(count);
            using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            try
            {
                Check(f.Step(pc), "R181 controller begins " + form.Name);
                for (int attempt = 0; f.Completed < count && attempt < 64; ++attempt)
                    Check(f.Continue(), "R181 controller resumes " + form.Name);
                Check(f.Completed == count && f.Issued == count && f.Machine()->ip == pc + (uint)bytes.Length &&
                    f.Statistics.NativeMemoryInstructions == (long)count && f.Statistics.SemanticHelperInstructions == 0 &&
                    f.Statistics.InterpreterAttempts == 0 && f.Machine()->flags == flags && ((ulong*)(m + 56))[0] == rsp,
                    "R181 real typed controller performs every memory operation natively and preserves issue/flags/RSP");
                Check(((ulong*)(m + 72))[0] == source + (uint)offset + (form.String ? 64u : 0) &&
                    ((ulong*)(m + 80))[0] == destination + (uint)offset + (form.String ? 64u : 0),
                    "R181 lookup scratch does not corrupt typed guest indexes");
                f.DataScope(0, () =>
                {
                    for (int i = 0; i < expected.Length; ++i)
                    {
                        byte* b = f.Program.LookupAddress2(f.Machine(), (long)(destination + (uint)(offset + i)), 0, 0);
                        Check(b != null && *b == expected[i], "R181 real mixed read/write result " + form.Name);
                    }
                });
                var state = f.Jit.Store.GetStatistics();
                Check(state.ActiveManagedLeases == 0 && state.ActiveNativeEpochs == 0 && f.Statistics.ActiveDataBorrows == 0,
                    "R181 controller releases all capabilities");
            }
            finally { f.NativePrepared = null; f.NativeReturned = null; }
        }
    }

    static void Cache181ControllerRetainedWrite()
    {
        using var f = new Cache181Fixture();
        ulong pc = Cache181Fixture.Base, data = pc + 0x20000;
        f.Map(pc); f.Fill(pc); f.Map(data, 8192); f.Fill(data, 8192);
        // First write is wholly within A. The vector read crosses A/B and
        // forces B's read refill. The final write to A must still be a hit.
        WriteControllerCode(f, pc, [0x48,0x89,0x06,0xf3,0x0f,0x6f,0x06,0x48,0x89,0x06,0xeb,0xfe]);
        byte* m = (byte*)f.Machine();
        ((ulong*)(m + 24))[0] = 0x8877665544332211;
        ((ulong*)(m + 72))[0] = data + 4088;
        Action<nint> cap = p => ((FullBlockFrame*)p)->NativeLimit = 1;
        f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap;
        f.Counts(3);
        using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        try
        {
            Check(f.Step(pc) && f.Completed == 1, "R181 real controller seeds admitted write on A");
            long initialAssists = f.Statistics.NativeMemoryAssists;
            Check(f.Continue() && f.Completed == 2 && f.Statistics.NativeMemoryAssists > initialAssists,
                "R181 real vector crossing forces a read refill of A/B");
            long assists = f.Statistics.NativeMemoryAssists;
            Check(f.Continue() && f.Completed == 3 && f.Issued == 3 &&
                f.Statistics.NativeMemoryAssists == assists && f.Statistics.NativeMemoryInstructions == 3 &&
                f.Statistics.InterpreterAttempts == 0 && f.Statistics.SemanticHelperInstructions == 0 &&
                ((ulong*)(m + 24))[0] == 0x8877665544332211,
                "R181 real controller preserves A write across read refill and final store needs no assist");
        }
        finally { f.NativePrepared = null; f.NativeReturned = null; }
    }

    static void Cache181ControllerRevocation()
    {
        foreach (bool codeAlias in new[] { false, true })
        {
            using var f = new Cache181Fixture();
            ulong pc = Cache181Fixture.Base, data = pc + 0x20000;
            f.Map(pc); f.Fill(pc); f.Map(data); f.Fill(data);
            WriteControllerCode(f, pc, [0x48,0x89,0x06,0x48,0x8b,0x06,0x48,0x89,0x06,0xeb,0xfe]);
            byte* m = (byte*)f.Machine(); ((ulong*)(m + 24))[0] = 0x1122334455667788; ((ulong*)(m + 72))[0] = data;
            Action<nint> cap = p => ((FullBlockFrame*)p)->NativeLimit = 1;
            f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap;
            f.Counts(3);
            using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            try
            {
                Check(f.Step(pc) && f.Completed == 1, "R181 controller first store admitted");
                if (codeAlias)
                {
                    nint physical = f.Physical(data);
                    f.Jit.BeginRead(f.Program, false);
                    try { lock (f.Jit.Authority.Gate) Check(f.Jit.Authority.PrepareCodeSnapshot(physical), "R181 physical code alias revokes Write"); }
                    finally { f.Jit.EndRead(f.Program); }
                }
                else { Check(f.Mutate("MAP_FIXED", data) == 0, "R181 controller remaps data"); f.Fill(data); }
                Check(f.Continue() && f.Completed == 2, "R181 controller reads fresh mapping after revocation");
                long barriers = f.Statistics.NativeMemoryAssistOutcomes["WriteBarrier"];
                Check(f.Continue() && f.Completed == 3 && f.Issued == 3 && f.Statistics.InterpreterAttempts == 0 &&
                    f.Statistics.SemanticHelperInstructions == 0, "R181 controller store completes under fresh authority");
                if (codeAlias) Check(f.Statistics.NativeMemoryAssistOutcomes["WriteBarrier"] > barriers, "R181 code alias still requires canonical write barrier");
            }
            finally { f.NativePrepared = null; f.NativeReturned = null; }
        }
    }
}

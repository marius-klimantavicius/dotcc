using System.Buffers.Binary;
using Managed.Emulation.Execution;
using JumpIndexFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // R109 completed in R187: SOURCE-AUTHORED / UNBUILT / UNRUN / UNQUALIFIED.
    // ONE parent entry; parent owns wiring, serial gate and matched service.
    // Dynamic source IR is classified from actual bytes using the strict R169 grammar.
    // Native tests execute only the current host ABI; encoding tests cover both ABIs.
    static void CheckFullJumpCacheIndex(byte* machine)
    {
        Check(FullNativeMemory.JumpCount == 256 && sizeof(FullNativeJumpEntry) == 64 &&
            FullNativeMemory.WorkerCharge == 33600 + FullWorkerNativeState.MappingCacheCharge &&
            FullNativeMemory.WorkerChargeFor(64) == 21312 + FullWorkerNativeState.MappingCacheCharge &&
            FullBlockFrame.ByteSize == 416 && FullBlockFrame.AbiVersion == 0x46420007 &&
            sizeof(TranslationBlockHeader) == 160, "R109 retains capacity and frame/header schema; R204 charges managed mapping witnesses separately");
        CheckJumpIndexLayout();
        CheckJumpIndexMath();
        CheckJumpIndexEncoding();
        Check(!fullOwnerScaffold.IsValid, "R109 standalone fixture starts without an owner slab");
        CheckJumpIndexNative(machine);
        CheckJumpIndexDynamicForms(machine);
        using (var common = CreateFullScaffold(out var addresses))
        {
            fullOwnerScaffold = addresses;
            try { CheckJumpIndexNative(machine); CheckJumpIndexDynamicForms(machine); }
            finally { fullOwnerScaffold = default; }
        }
        CheckJumpIndexController();
        CheckJumpIndexDynamicController();
        CheckJumpIndexControllerAdmission();
        CheckJumpIndexControllerGuards();
        ResetFull(machine);
    }

    static void CheckJumpIndexMath()
    {
        // Independent byte decomposition oracle, including the entire 16-bit
        // input period and nonzero upper address bits. No dropped low nibble.
        foreach (ulong upper in new[] { 0UL, 0x123456780000UL, 0xffff800000000000UL })
        for (uint word = 0; word <= ushort.MaxValue; ++word)
        {
            ulong pc = upper | word;
            int expected = (int)((word & 255) ^ (word / 256));
            uint nativeMasked = unchecked((uint)((pc >> 8) ^ pc)) & 255u;
            Check(FullNativeMemory.JumpIndex(pc, 255) == expected && nativeMasked == expected,
                "R109 independent byte oracle and unsigned native register model agree over the full16-bit period");
        }
        const ulong a = 0x12345001, b = a + 2, page = 0x12346001, conflict = a ^ 0x101;
        Check(((a >> 4) & 255) == ((b >> 4) & 255) &&
            FullNativeMemory.JumpIndex(a, 255) == 0x51 && FullNativeMemory.JumpIndex(b, 255) == 0x53,
            "R109 distinct entrypoints inside sixteen bytes discriminate the former deterministic collision");
        Check(FullNativeMemory.JumpIndex(page, 255) == 0x61 && (a & 255) == (page & 255),
            "R109 page bits XOR into the low byte; low eight bits are not retained unchanged");
        Check(a != conflict && FullNativeMemory.JumpIndex(a, 255) == FullNativeMemory.JumpIndex(conflict, 255),
            "R109 direct-mapped hints still have legitimate collisions at unchanged capacity");
        FullNativeJumpEntry* hints = stackalloc FullNativeJumpEntry[256];
        var header = FullHeader(a, (nint)1);
        foreach (uint mask in new[] { 0u, 1u, 15u, 511u, 4095u })
        {
            bool rejected = false;
            try { FullNativeMemory.PublishJump(hints, mask, 123, &header, in header); }
            catch (InvalidOperationException) { rejected = true; }
            Check(rejected, "R109 managed publication still admits only the validated 255 mask");
        }
        for (int n = 0; n < 32; ++n) FullNativeMemory.PublishJump(hints, 255, 123, &header, in header);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int n = 0; n < 10000; ++n) FullNativeMemory.PublishJump(hints, 255, 123, &header, in header);
        Check(GC.GetAllocatedBytesForCurrentThread() == before,
            "R109 warmed managed hint publication allocates no per-PC object");
    }

    static void CheckJumpIndexNative(byte* machine)
    {
        // Independently owned allocations and stack headers survive every RET.
        // A single root allocation owns each generated body, including targets.
        using var source = CreateFullBlock([new(JumpIndexClassify(Ip, [0xff, 0xe0]))]);
        foreach ((ulong a, ulong b) in new (ulong, ulong)[]
        {
            (0x12345001, 0x12345003), // same old sixteen-byte bucket
            (0x12345001, 0x12346001), // same byte offset, different folded page bits
            (0xffff800012345001, 0xffff800012345003)
        })
        {
            using var first = CreateFullBlock([new(new(a, 1, JitOperation.Nop, 64, default, default))]);
            using var second = CreateFullBlock([new(new(b, 1, JitOperation.Nop, 64, default, default))]);
            var ah = FullHeader(a, first.Entry, 18); ah.Reserved136 = 1;
            var bh = FullHeader(b, second.Entry, 19); bh.Reserved136 = 1;
            FullNativeJumpEntry* cache = stackalloc FullNativeJumpEntry[256];
            new Span<FullNativeJumpEntry>(cache, 256).Clear();
            FullNativeMemory.PublishJump(cache, 255, 123, &ah, in ah);
            FullNativeMemory.PublishJump(cache, 255, 123, &bh, in bh);
            Check(cache[FullNativeMemory.JumpIndex(a, 255)].Header == &ah &&
                cache[FullNativeMemory.JumpIndex(b, 255)].Header == &bh,
                "R109 actual managed publication keeps nearby/page-folded targets simultaneously");
            foreach (ulong target in new[] { a, b, a, b })
            foreach (uint flags in new[] { 0u, 0x00400cd5u, 0x00c00891u })
            foreach (uint limit in new[] { 0u, 1u, 2u })
            for (ulong budget = 0; budget <= 3; ++budget)
            foreach (int poll in new[] { 0, 1, 2 }) // running, stop, attention
            {
                ResetFull(machine); Write64(machine, 24, target);
                ((Blink.Machine*)machine)->flags = (((Blink.Machine*)machine)->flags | flags) &
                    ~((1u << Blink.FLAGS_TF) | (1u << Blink.FLAGS_RF));
                if (poll == 2) machine[11] = 1;
                byte[] expectedMachine = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
                ulong global = 11, generation = 123; long issued = 0, completed = 0; int stop = poll == 1 ? 1 : 0;
                var header = FullHeader(Ip, source.Entry);
                var frame = FullFrame(machine, &header, &global, &issued, &completed, &stop, budget, limit);
                frame.JumpCache = cache; frame.JumpCacheMask = 255;
                frame.StoreGenerationPointer = &generation; frame.ObservedStoreGeneration = 123;
                int done = poll != 0 || budget == 0 ? 0 : limit == 1 || budget == 1 ? 1 : 2;
                FullBlockExit expectedExit = poll == 1 ? FullBlockExit.Stop : poll == 2 ? FullBlockExit.Attention :
                    budget == 0 || budget == 1 && limit != 1 ? FullBlockExit.Budget : FullBlockExit.Continue;
                long expectedIssued = poll != 0 ? 0 : done + (expectedExit == FullBlockExit.Budget ? 1 : 0);
                ulong tails = poll == 0 && budget != 0 && limit != 1 ? 1UL : 0;
                if (done != 0)
                {
                    BinaryPrimitives.WriteUInt64LittleEndian(expectedMachine, target + (done == 2 ? 1UL : 0));
                    expectedMachine[8] = 0;
                }
                Check(RunFull(source, &frame) == expectedExit && issued == expectedIssued && completed == done &&
                    frame.Executed == (ulong)done && frame.TailJumps == tails && frame.NativeLimit == limit &&
                    frame.HelperIndex == FullBlockFrame.NoHelperIndex && frame.MemoryAssists == 0 &&
                    new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expectedMachine),
                    "R109 published/native lookup agrees with exact budgets, caps, polls and complete guest state/flags");
            }
        }
        CheckJumpIndexAuthentication(machine, source);
        CheckJumpIndexPostcommitPolls(machine, source);
    }

    static void CheckJumpIndexAuthentication(byte* machine, NativeCodeAllocation source)
    {
        const ulong targetPc = 0x12345003;
        using var target = CreateFullBlock([new(new(targetPc, 1, JitOperation.Nop, 64, default, default))]);
        FullNativeJumpEntry* cache = stackalloc FullNativeJumpEntry[256];
        foreach (string fault in new[] { "valid", "mask", "stable-generation", "entry-generation", "null-generation",
            "null-cache", "collision", "high-alias", "entry-pc", "entry-context", "entry-owner", "entry-body", "null-header",
            "target-live", "target-trust", "target-owner", "target-body", "target-pc", "target-context",
            "target-zero-code", "target-code", "target-code-global", "target-entry", "source-live", "source-owner",
            "source-body", "source-context", "source-pc", "source-zero-code", "source-code", "source-code-global" })
        {
            ResetFull(machine); Write64(machine, 24, targetPc);
            new Span<FullNativeJumpEntry>(cache, 256).Clear();
            ulong global = 11, generation = 123; long issued = 0, completed = 0; int stop = 0;
            var h = FullHeader(Ip, source.Entry); var t = FullHeader(targetPc, target.Entry, 18); t.Reserved136 = 1;
            FullNativeMemory.PublishJump(cache, 255, 123, &t, in t);
            var f = FullFrame(machine, &h, &global, &issued, &completed, &stop, limit: 2);
            f.JumpCache = cache; f.JumpCacheMask = 255;
            f.StoreGenerationPointer = &generation; f.ObservedStoreGeneration = 123;
            ref var entry = ref cache[FullNativeMemory.JumpIndex(targetPc, 255)];
            switch (fault)
            {
                // Poison values must be rejected BEFORE the first dereference.
                case "mask": f.JumpCacheMask = 511; f.StoreGenerationPointer = (ulong*)1; f.JumpCache = (FullNativeJumpEntry*)1; break;
                case "stable-generation": generation = 124; entry.Header = (TranslationBlockHeader*)1; break;
                case "entry-generation": entry.StoreGeneration = 122; entry.Header = (TranslationBlockHeader*)1; break;
                case "null-generation": f.StoreGenerationPointer = null; entry.Header = (TranslationBlockHeader*)1; break;
                case "null-cache": f.JumpCache = null; break;
                case "collision": entry.GuestPc ^= 0x101; entry.Header = (TranslationBlockHeader*)1; break;
                case "high-alias": entry.GuestPc |= 0xffff800000000000; entry.Header = (TranslationBlockHeader*)1; break;
                case "entry-pc": ++entry.GuestPc; entry.Header = (TranslationBlockHeader*)1; break;
                case "entry-context": ++entry.Context; entry.Header = (TranslationBlockHeader*)1; break;
                case "entry-owner": ++entry.OwnerCookie; entry.Header = (TranslationBlockHeader*)1; break;
                case "entry-body": ++entry.BodyGeneration; break;
                case "null-header": entry.Header = null; break;
                case "target-live": t.Live = 0; break;
                case "target-trust": t.Reserved136 = 0; break;
                case "target-owner": ++t.OwnerCookie; break;
                case "target-body": ++t.BodyGeneration; break;
                case "target-pc": ++t.GuestPc; break;
                case "target-context": ++t.Context; break;
                case "target-zero-code": t.CodeEpoch = 0; break;
                case "target-code": ++t.ValidatedCodeEpoch; break;
                case "target-code-global": ++t.ValidatedGlobalEpoch; break;
                case "target-entry": t.Entry = 0; break;
                case "source-live": h.Live = 0; break;
                case "source-owner": ++h.OwnerCookie; break;
                case "source-body": ++h.BodyGeneration; break;
                case "source-context": ++h.Context; break;
                case "source-pc": ++h.GuestPc; break;
                case "source-zero-code": h.CodeEpoch = 0; break;
                case "source-code": ++h.ValidatedCodeEpoch; break;
                case "source-code-global": ++h.ValidatedGlobalEpoch; break;
            }
            bool sourceFailure = fault.StartsWith("source-", StringComparison.Ordinal);
            int done = sourceFailure ? 0 : fault == "valid" ? 2 : 1;
            byte[] expected = new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).ToArray();
            if (done != 0)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(expected, targetPc + (done == 2 ? 1UL : 0));
                expected[8] = 0;
            }
            Check(RunFull(source, &f) == (sourceFailure ? FullBlockExit.Invalidated : FullBlockExit.Continue) &&
                issued == done && completed == done && f.Executed == (ulong)done &&
                f.TailJumps == (fault == "valid" ? 1UL : 0) &&
                new ReadOnlySpan<byte>(machine, sizeof(Blink.Machine)).SequenceEqual(expected),
                "R109 source/target authentication and stale-header poison preserve exact refusal/effects");
        }
    }

    static void CheckJumpIndexPostcommitPolls(byte* machine, NativeCodeAllocation source)
    {
        const ulong targetPc = 0x12345003;
        using var target = CreateFullBlock([new(new(targetPc, 1, JitOperation.Nop, 64, default, default))]);
        FullNativeJumpEntry* cache = stackalloc FullNativeJumpEntry[256];
        foreach (int changed in new[] { 0, 1, 2, 3 }) // runtime epoch, code epoch, stop, store generation
        {
            ResetFull(machine); Write64(machine, 24, targetPc);
            uint flags = ((Blink.Machine*)machine)->flags;
            new Span<FullNativeJumpEntry>(cache, 256).Clear();
            ulong global = 11, code = 11, generation = 123; long issued = 0, counterAndStop = 0; int running = 0;
            var h = FullHeader(Ip, source.Entry); var t = FullHeader(targetPc, target.Entry, 18); t.Reserved136 = 1;
            FullNativeMemory.PublishJump(cache, 255, 123, &t, in t);
            // Completion aliases a host authority/poll word deterministically;
            // the generated source commits before target authentication/poll.
            long* completion = changed == 0 ? (long*)&global : changed == 1 ? (long*)&code : changed == 3 ? (long*)&generation : &counterAndStop;
            int* stop = changed == 2 ? (int*)&counterAndStop : &running;
            var f = FullFrame(machine, &h, &global, &issued, completion, stop, limit: 2, codeEpoch: &code);
            f.JumpCache = cache; f.JumpCacheMask = 255;
            f.StoreGenerationPointer = &generation; f.ObservedStoreGeneration = 123;
            Check(RunFull(source, &f) == (changed == 2 ? FullBlockExit.Stop : FullBlockExit.Continue) &&
                issued == 1 && f.Executed == 1 && f.TailJumps == (changed == 2 ? 1UL : 0) &&
                ((ulong*)machine)[0] == targetPc && ((Blink.Machine*)machine)->flags == flags &&
                *completion == (changed == 2 ? 1 : changed == 3 ? 124 : 12),
                "R109 runtime/code/stop change after source commit prevents target effects and further issue");
        }
    }

    static void CheckJumpIndexEncoding()
    {
        byte[] storage = new byte[FullBlockEmitter.CodeCapacity + 32], slab = new byte[FullBlockEmitter.ScaffoldCapacity];
        ReadOnlySpan<byte> hash = [0x49,0x89,0xc0,0x48,0xc1,0xe8,8,0x4c,0x31,0xc0,
            0x41,0x23,0x82,0x40,1,0,0,0x48,0xc1,0xe0,6,0x48,1,0xc1];
        foreach (bool windows in new[] { false, true })
        {
            FullBlockEmitter.EmitScaffold(slab, out var layout, windows);
            var scaffold = layout.Resolve((nint)0x10000000); // encoding only; never executed
            foreach (var form in JumpIndexForms())
            foreach (bool shared in new[] { false, true })
            {
                FullBlockInstruction[] ir = [new(JumpIndexClassify(Ip, form.Bytes))];
                Span<byte> body = storage.AsSpan(8, FullBlockEmitter.CodeCapacity);
                storage.AsSpan().Fill(0xa5);
                int length = shared ? FullBlockEmitter.Emit(ir, body, in scaffold, windows) : FullBlockEmitter.Emit(ir, body, windows);
                ReadOnlySpan<byte> emitted = body[..length];
                int at = emitted.IndexOf(hash);
                Check(at >= 0 && CountFullPattern(emitted, hash) == 1 &&
                    CountFullPattern(emitted, [0x49,0x89,0xc0,0x48,0xc1,0xe8,4]) == 0 &&
                    emitted[..at].LastIndexOf((ReadOnlySpan<byte>)[0x48,0x8b,0x12,0x49,0x3b,0x92,0x30,1,0,0]) >= 0 &&
                    emitted[(at + hash.Length)..].IndexOf((ReadOnlySpan<byte>)[0x48,0x8b,0x51,32,0x48,0x8b,0x49,40]) >= 0,
                    "R109 exact MOV/SHR8/XOR/AND255/SHL6/ADD bytes between stable-generation and header loads");
                int pointer = emitted[(at + hash.Length)..].IndexOf((ReadOnlySpan<byte>)[0x48,0x8b,0x51,32,0x48,0x8b,0x49,40]);
                ReadOnlySpan<byte> tags = emitted.Slice(at + hash.Length, pointer);
                Check(tags.IndexOf((ReadOnlySpan<byte>)[0x48,0x39,0x51,16]) >= 0 && tags.IndexOf((ReadOnlySpan<byte>)[0x4c,0x39,0x01]) >= 0 &&
                    tags.IndexOf((ReadOnlySpan<byte>)[0x49,0x8b,0x42,120,0x48,0x3b,0x41,8]) >= 0 &&
                    tags.IndexOf((ReadOnlySpan<byte>)[0x49,0x8b,0x42,104,0x48,0x3b,0x41,24]) >= 0,
                    "R109 entry generation/full PC/context/owner guards precede retained Header pointer load");
                Check(CountFullPattern(emitted, windows ? new byte[] {0x4c,0x89,0xd1,0xff,0xe0} :
                    new byte[] {0x4c,0x89,0xd7,0xff,0xe0}) >= 1 && storage.AsSpan(0,8).IndexOfAnyExcept((byte)0xa5) < 0 &&
                    storage.AsSpan(8 + FullBlockEmitter.CodeCapacity).IndexOfAnyExcept((byte)0xa5) < 0,
                    "R109 both host ABI argument restores and caller-owned span canaries");
                foreach (int bytes in new[] { 0, 7, at + 7, at + 10, length - 1 })
                {
                    storage.AsSpan().Fill(0xa5);
                    Span<byte> shortBody = storage.AsSpan(8, bytes);
                    bool fits = shared ? FullBlockEmitter.TryEmit(ir, shortBody, in scaffold, out int n, windows) :
                        FullBlockEmitter.TryEmit(ir, shortBody, out n, windows);
                    Check(!fits && n == 0 && storage[7] == 0xa5 && storage[8 + bytes] == 0xa5,
                        "R109 truncated hash/branch spans publish nothing and preserve adjacent bytes");
                }
                for (int warm = 0; warm < 32; ++warm)
                {
                    if (shared) FullBlockEmitter.Emit(ir, body, in scaffold, windows);
                    else FullBlockEmitter.Emit(ir, body, windows);
                }
                long allocated = GC.GetAllocatedBytesForCurrentThread(); bool good = true;
                for (int repeat = 0; repeat < 64; ++repeat)
                    good &= (shared ? FullBlockEmitter.Emit(ir, body, in scaffold, windows) :
                        FullBlockEmitter.Emit(ir, body, windows)) == length;
                Check(good && GC.GetAllocatedBytesForCurrentThread() == allocated,
                    "R109 warmed caller-span emission allocates zero for both ABIs/scaffolds");
            }
        }
    }

    static TranslationToken JumpIndexWarm(JumpIndexFixture f, ulong pc)
    {
        TranslationToken token = default;
        // First registration can conservatively stamp zero. The second real
        // acquisition validates the same resident body at stable authority.
        for (int repeat = 0; repeat < 2; ++repeat)
        {
            Check(f.Acquire(pc, 0, out var lease, out bool multiple, out int error) && error == 0,
                "R109 actual retained indirect body acquisition");
            using (lease)
            {
                Check(repeat == 0 || token == lease.Token, "R109 stable reacquisition retains its original translation token");
                token = lease.Token;
                lock (f.Jit.Authority.Gate) f.Jit.AuthorizeLinks(in lease, multiple);
            }
        }
        return token;
    }

    static void CheckJumpIndexController()
    {
        using var f = new JumpIndexFixture();
        const ulong a = JumpIndexFixture.Base + 1, b = a + 8, c = a ^ 0x101;
        f.Map(JumpIndexFixture.Base); f.Fill(JumpIndexFixture.Base);
        WriteControllerCode(f, a, [0xff,0xe3]); // JMP RBX -> B
        WriteControllerCode(f, b, [0xff,0xe0]); // JMP RAX -> A
        WriteControllerCode(f, c, [0xff,0xe3]); // legitimate competing hint -> B
        TranslationToken ta = JumpIndexWarm(f, a), tb = JumpIndexWarm(f, b), tc = JumpIndexWarm(f, c);
        var m = f.Machine(); Write64((byte*)m, 24, a); Write64((byte*)m, 48, b);
        uint flags = m->flags; byte[] registers = new ReadOnlySpan<byte>((byte*)m + 24, 128).ToArray();
        var before = f.Jit.Store.GetStatistics();
        Action<nint> cap1 = p => ((FullBlockFrame*)p)->NativeLimit = 1;
        bool both = false, overwritten = false, restored = false;
        f.Counts(10000);
        try
        {
            // Only the actual controller publishes these worker-owned hints.
            f.NativePrepared = cap1; Check(f.Step(a), "R109 controller publishes A on its real return");
            f.NativeReturned = p =>
            {
                var frame = (FullBlockFrame*)p;
                both = frame->JumpCacheMask == 255 && frame->JumpCache[FullNativeMemory.JumpIndex(a,255)].GuestPc == a &&
                    frame->JumpCache[FullNativeMemory.JumpIndex(b,255)].GuestPc == b;
            };
            f.NativePrepared = cap1; Check(f.Step(b), "R109 controller publishes B on its real return");
            Check(both && f.Completed == 2 && f.Issued == 2,
                "R109 real nearby guest entrypoints coexist after separate controller returns");
            f.NativeReturned = null;
            for (int phase = 0; phase < 2; ++phase)
            {
                ulong done = f.Completed, issued = f.Issued;
                long entries = f.Statistics.NativeEntries, tails = f.Statistics.NativeTailJumps;
                f.NativePrepared = p => ((FullBlockFrame*)p)->NativeLimit = 64;
                Check(f.Step(a) && f.Completed == done + 64 && f.Issued == issued + 64 && m->ip == a &&
                    f.Statistics.NativeEntries == entries + 1 && f.Statistics.NativeTailJumps == tails + 63,
                    "R109 real decoded indirect loop completes sixty-four instructions in one physical entry");
                if (phase != 0) break;
                f.NativeReturned = p =>
                {
                    var frame = (FullBlockFrame*)p;
                    overwritten = frame->JumpCache[FullNativeMemory.JumpIndex(a,255)].GuestPc == c &&
                        frame->JumpCache[FullNativeMemory.JumpIndex(b,255)].GuestPc == b;
                };
                f.NativePrepared = cap1; Check(f.Step(c), "R109 real competing phase publishes C");
                Check(overwritten, "R109 legitimate same-index publication replaces only the nonowning A hint");
                foreach (var token in new[] { ta, tb, tc })
                {
                    Check(f.Jit.Store.TryAcquireToken(token, out var lease), "R109 hint collision leaves every translation resident");
                    using (lease) Check(f.Jit.Store.GetHeaderCopy(in lease).Live == 1,
                        "R109 hint replacement neither retires nor invalidates a body");
                }
                f.NativeReturned = p =>
                {
                    var frame = (FullBlockFrame*)p;
                    restored = frame->JumpCache[FullNativeMemory.JumpIndex(a,255)].GuestPc == a &&
                        frame->JumpCache[FullNativeMemory.JumpIndex(b,255)].GuestPc == b;
                };
                f.NativePrepared = cap1; Check(f.Step(a), "R109 original resident A republishes after competing phase");
                Check(restored, "R109 resident lookup restores only the nonowning hint");
                f.NativeReturned = null;
            }
        }
        finally { f.NativePrepared = null; f.NativeReturned = null; }
        var after = f.Jit.Store.GetStatistics();
        Check(after.ResidentBodies == before.ResidentBodies && after.Publications == before.Publications &&
            after.CumulativeEmittedCodeBytes == before.CumulativeEmittedCodeBytes && after.TotalFlushes == before.TotalFlushes &&
            after.CapacityRetirements == before.CapacityRetirements && after.ContentRetirements == before.ContentRetirements &&
            after.ChargedBytes == before.ChargedBytes && after.NativeReservedBytes == before.NativeReservedBytes &&
            after.BodyCapacity == before.BodyCapacity && after.ChargedBytes <= JumpIndexFixture.Budget,
            "R109 indirect phase/collision/recovery emits nothing and preserves bodies, capacity and charged budget");
        Check(m->flags == flags && new ReadOnlySpan<byte>((byte*)m + 24,128).SequenceEqual(registers) &&
            f.Statistics.SemanticHelperInstructions == 0 && f.Statistics.SemanticHelperCalls == 0 &&
            f.Statistics.InterpreterAttempts == 0 && f.Statistics.NativeMemoryAssists == 0 &&
            after.ActiveNativeEpochs == 0 && after.ActiveManagedLeases == 0 && f.Statistics.ActiveDataBorrows == 0,
            "R109 actual indirect loop preserves software flags/registers and releases capabilities without helpers");
    }
}

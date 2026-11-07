using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using ControllerFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // Parent wires this entry into Main. These contracts execute the NEW
    // controller, with the exact product sources in the test assembly. Source
    // authoring is separate from the parent's serial build/runtime receipts.
    static void CheckFullController()
    {
        CheckFullControllerQuotaDenial();
        CheckFullControllerFixtureSetup();
        CheckFullControllerAttention();
        CheckFullControllerCodeAuthorityRetry();
        CheckFullControllerMappingTransactions();
        CheckFullControllerMappingScopeNesting();
        CheckFullControllerPhysicalBorrows();
        CheckFullControllerSharedAliases();
        CheckFullControllerAuthoritySaturation();
        CheckFullControllerHostWriterEnrollment();
        CheckFullControllerWriterCompletionProtocol();
        CheckFullControllerSegmentBaseRevalidation();
        CheckFullControllerLongContextProvenance();
        CheckFullControllerAllocationCategories();
        CheckFullControllerCollidersAndPressure();
    }

    static void CheckFullControllerQuotaDenial()
    {
        var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
        using var stop = new HostExecutionStop();
        try
        {
            var owner = new ThreadedGuestExecution(io, stop, ControllerFixture.MemoryLimit, 2,
                jitOptions: new() { Mode = GuestJitMode.Full, CacheLimitBytes = 128L << 10, AllowUnverifiedWindows = true });
            bool rejected = false;
            try { owner.Run("/not-loaded", ["/not-loaded"], [], 1); }
            catch (InvalidOperationException error) when (error.Message.Contains("configured retention quota 131072") &&
                error.Message.Contains("no guest instruction executed")) { rejected = true; }
            Check(rejected && owner.IsQuiescent && owner.InstructionsIssued == 0 && owner.InstructionsCompleted == 0,
                "Full controller rejects explicit 128 KiB quota before guest work and releases owner resources");
            Check(owner.JitStatistics.CacheBytes == 0 && io.MemoryFilesUsage().Leases == 0,
                "Full underfunded startup retains neither cache charge nor backing leases");
        }
        finally { io.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
        // IsQuiescent is the controller cleanup contract. The public facade's
        // ResourcesReleased result is separately checked by the parent harness.
    }

    static void CheckFullControllerFixtureSetup()
    {
        using var f = new ControllerFixture();
        var system = f.Machine()->blink_host_system;
        ulong root = system->cr3;
        Check(root != 0 && root != ulong.MaxValue && (root & (ulong)(Blink.PAGE_V | Blink.PAGE_HOST)) ==
            (ulong)(Blink.PAGE_V | Blink.PAGE_HOST) && system->cr0 == 0x80000013u,
            "direct controller fixture supplies the protected-mode loader CPU and root-table initialization");
        byte* table = f.Program.GetPageAddress(system, root, true);
        Check(table != null && f.Program.BlinkHostMemoryContains(table, 4096) != 0 &&
            new ReadOnlySpan<byte>(table, 4096).IndexOfAnyExcept((byte)0) < 0,
            "controller fixture begins with an owned empty page-table root");
        Check(f.Machine(1)->blink_host_system == system && (f.Machine(1)->flags & (1u << 9)) != 0,
            "second fixture machine inherits initialized protected-mode process state");
        f.Map(ControllerFixture.Base);
        Check(system->cr3 == root && JitPageIdentity.TryRead(f.Program, f.Machine(), ControllerFixture.Base, out ulong reserved) &&
            (reserved & (ulong)Blink.PAGE_RSRV) != 0,
            "first direct ReserveVirtual installs a reserved leaf under the initialized root");
        f.Fill(ControllerFixture.Base);
        Check(JitPageIdentity.TryRead(f.Program, f.Machine(), ControllerFixture.Base, out ulong committed) &&
            (committed & (ulong)(Blink.PAGE_V | Blink.PAGE_HOST)) == (ulong)(Blink.PAGE_V | Blink.PAGE_HOST) &&
            (committed & (ulong)Blink.PAGE_RSRV) == 0 && f.Physical(ControllerFixture.Base) != 0 &&
            f.Issued == 0 && f.Completed == 0,
            "fixture commits real backing without issuing any guest instruction");
    }

    static ulong FullRax(ControllerFixture fixture, int worker = 0)
        => BinaryPrimitives.ReadUInt64LittleEndian(new((byte*)fixture.Machine(worker) + 24, 8));
    static void FullRax(ControllerFixture fixture, ulong value, int worker = 0)
        => BinaryPrimitives.WriteUInt64LittleEndian(new((byte*)fixture.Machine(worker) + 24, 8), value);

    static void CheckFullControllerAttention()
    {
        foreach (bool helpersOnly in new[] { false, true })
        foreach (bool late in new[] { false, true })
        {
            using var f = new ControllerFixture(helpersOnly);
            f.Map(ControllerFixture.Base); f.Fill(ControllerFixture.Base);
            f.WriteMov(ControllerFixture.Base, 0x12345678);
            f.Counts(7, issued: 6, completed: 6);
            int hookCalls = 0;
            f.Preissued = (pointer, actor) =>
            {
                ++hookCalls;
                Check(actor && pointer == (nint)f.Machine() && f.Issued == 7 && f.Completed == 6,
                    "recursive controller approves exactly the last allowed issue before native entry");
                if (late) f.Machine()->attention = true;
            };
            // With restored=false and no pending signal, the real SignalActor
            // executes its first instruction, then CheckForSignals clears
            // attention, then its next reservation encounters the budget.
            f.SignalActor(ControllerFixture.Base, attentionBefore: !late);
            Check(hookCalls == 1 && f.Completed == 7 && f.Issued == 8 && FullRax(f) == 0x12345678 &&
                f.Machine()->ip == ControllerFixture.Base + 5 && f.Stop.Reason == HostExecutionStopReason.Budget,
                "SignalActor attention does not lose/double the last approved native or helper instruction");
            var s = f.Statistics;
            Check(s.InterpreterAttempts == 0 && s.InlineNativeInstructions + s.SemanticHelperInstructions == 1,
                "recursive attention completes once through the new Full controller");
            if (helpersOnly) Check(s.SemanticHelperInstructions == 1, "baseline recursive witness invokes retained specific helper");
            else Check(s.InlineNativeInstructions == 1, "detected recursive witness enters native scalar body");
        }
        using (var f = new ControllerFixture())
        {
            f.Map(ControllerFixture.Base); f.Fill(ControllerFixture.Base); f.Counts(10);
            f.Preissued = (_, actor) => { Check(!actor, "ordinary controller late attention hook"); f.Machine()->attention = true; };
            Check(f.Step(ControllerFixture.Base) && f.Issued == 1 && f.Completed == 1 &&
                f.Machine()->ip == ControllerFixture.Base + 5,
                "late attention preserves first preissue and blocks the following native instruction");
        }
        using (var f = new ControllerFixture())
        {
            f.Map(ControllerFixture.Base); f.Fill(ControllerFixture.Base); f.Counts(10);
            FullRax(f, 0);
            f.Preissued = (_, _) => f.Stop.RequestStop();
            Check(f.Step(ControllerFixture.Base) && f.Issued == 1 && f.Completed == 0 &&
                f.Machine()->ip == ControllerFixture.Base && FullRax(f) == 0,
                "stop arriving after preissue still prevents every native effect");
        }
    }

    static void CheckFullControllerCodeAuthorityRetry()
    {
        foreach (bool helpersOnly in new[] { false, true })
        foreach (bool lastAllowance in new[] { false, true })
        foreach (bool signalActor in new[] { false, true })
        {
            using var f = new ControllerFixture(helpersOnly);
            ulong pc = ControllerFixture.Base;
            f.Map(pc); f.Fill(pc); f.WriteMov(pc, 0x13579bdf);
            Check(f.Acquire(pc, 0, out var warm, out _, out int error, requestedCount: signalActor ? 1u : 0u) && error == 0,
                "code-authority retry witness warms one actual controller translation");
            var token = warm.Token;
            var original = f.Jit.Store.GetHeaderCopy(in warm);
            warm.Dispose();
            ulong epoch = f.Jit.Authority.Epoch, codeEpoch = f.Jit.Authority.CodeEpoch;
            long starting = lastAllowance ? 6 : 0;
            ulong budget = lastAllowance ? 7UL : 100UL;
            f.Counts(budget, starting, starting); FullRax(f, 0xfeedfaceUL);
            int preissuedCalls = 0, revokedCalls = 0;
            f.Preissued = (pointer, actor) =>
            {
                ++preissuedCalls;
                Check(pointer == (nint)f.Machine() && actor == signalActor &&
                    f.Issued == (ulong)starting + 1 && f.Completed == (ulong)starting,
                    "code revocation reserves its one approved issue before the first native attempt");
                // Ordinary execution exits on attention after its first real
                // completion; the signal-actor variant has the product cap1.
                // Neither path attempts a second guest issue in this witness.
                if (!signalActor) f.Machine()->attention = true;
            };
            f.NativePrepared = pointer =>
            {
                ++revokedCalls;
                var frame = (FullBlockFrame*)pointer;
                var header = frame->Header;
                var before = f.Jit.Store.GetStatistics();
                Check(Monitor.IsEntered(f.Jit.Authority.Gate) && before.ActiveManagedLeases > 0 &&
                    before.ActiveNativeEpochs == 0 && frame->FirstPreissued == 1 && frame->ObservedGlobalEpoch == epoch &&
                    header->Live == 1 && header->CodeEpoch == header->ValidatedCodeEpoch &&
                    header->Entry == original.Entry && header->BodyGeneration == token.Generation,
                    "prepared native frame has a managed lease and stable body before its native epoch");
                // Deterministic controller model of the c6 failure's separate
                // code-revocation edge. There is no host writer, Advance call,
                // byte mutation, or claim to replay the OS-thread queue order.
                Check(f.Jit.Store.MarkAuthorityChanged(header->GuestPc, header->GuestLength) == 1,
                    "native-prepared hook revokes exactly the current retained guest range");
                Check(f.Jit.Authority.Epoch == epoch && header->ValidatedGlobalEpoch == codeEpoch && header->Live == 0 &&
                    header->CodeEpoch == original.CodeEpoch + 1 && header->ValidatedCodeEpoch == original.ValidatedCodeEpoch &&
                    header->Entry == original.Entry && header->BodyGeneration == token.Generation &&
                    f.Issued == (ulong)starting + 1 && f.Completed == (ulong)starting,
                    "body Live and code epoch revoke without changing global epoch, entry, generation, or approved issue");
                Check(f.Jit.Store.TryAcquireToken(token, out var retained), "code-only revoked token remains resident");
                retained.Dispose();
            };
            Check(f.Step(pc, signalActor: signalActor) && preissuedCalls == 1 && revokedCalls == 1 &&
                f.NativePreparedCalls == 2 && f.Issued == (ulong)starting + 1 && f.Completed == (ulong)starting + 1 &&
                f.Machine()->ip == pc + 5 && FullRax(f) == 0x13579bdf && f.Stop.Reason == HostExecutionStopReason.None,
                "zero-work code-authority rejection retries unchanged body using the same issue, including the last budget allowance");
            Check(f.Jit.Store.TryAcquireToken(token, out var finalLease) && f.Jit.Authority.Epoch == epoch && f.ResumeToken() == token,
                "code-only retry preserves exact resident token and global epoch");
            using (finalLease)
            {
                var header = f.Jit.Store.GetHeaderCopy(in finalLease);
                Check(header.Live == 1 && header.CodeEpoch == original.CodeEpoch + 1 &&
                    header.CodeEpoch == header.ValidatedCodeEpoch && header.ValidatedGlobalEpoch == codeEpoch &&
                    header.Entry == original.Entry && header.BodyGeneration == original.BodyGeneration,
                    "retry restamps the same live entry and generation after byte revalidation");
            }
            var statistics = f.Statistics;
            var store = f.Jit.Store.GetStatistics();
            Check(statistics.ActualBlockCompilations == 1 && statistics.CompileAttempts == 1 &&
                statistics.AuthorityRevalidations == 1 && statistics.NativeEntries == 2 &&
                statistics.InlineNativeInstructions == (helpersOnly ? 0 : 1) &&
                statistics.SemanticHelperInstructions == (helpersOnly ? 1 : 0) && statistics.InterpreterAttempts == 0 &&
                store.ContentRetirements == 0 && store.CapacityRetirements == 0 && store.ResidentBodies == 1 &&
                store.ActiveManagedLeases == 0 && store.ActiveNativeEpochs == 0,
                "native and helper code-only retries compile once, complete once, retain one body, and release both attempt leases");
        }

        foreach (bool corruptAbi in new[] { true, false })
        {
            using var f = new ControllerFixture();
            ulong pc = ControllerFixture.Base;
            f.Map(pc); f.Fill(pc); f.Counts(7, 6, 6); FullRax(f, 0xfeedfaceUL);
            int preissuedCalls = 0;
            f.Preissued = (_, _) => ++preissuedCalls;
            f.NativePrepared = pointer =>
            {
                var frame = (FullBlockFrame*)pointer;
                if (corruptAbi) ++frame->Version;
                else frame->Context ^= 1;
            };
            bool rejected = false;
            try { f.Step(pc, signalActor: true); }
            catch (InvalidOperationException failure) when (failure.Message ==
                "Full-JIT validated native entry rejected without a concurrent authority change.") { rejected = true; }
            Check(rejected && preissuedCalls == 1 && f.NativePreparedCalls == 1 && f.Issued == 7 && f.Completed == 6 &&
                f.Machine()->ip == pc && FullRax(f) == 0xfeedfaceUL && f.Statistics.InterpreterAttempts == 0,
                "unexpected native ABI or context rejection stays fatal without a revoked authority stamp");
        }
    }

    static void CheckFullControllerMappingTransactions()
    {
        foreach (string operation in new[] { "mprotect", "munmap", "MAP_FIXED" })
        {
            using var f = new ControllerFixture();
            ulong pc = ControllerFixture.Base;
            f.Map(pc, 8192); f.Fill(pc, 8192);
            Check(f.Acquire(pc, 1, out var warm, out bool warmMultiple, out int warmError) && warmError == 0 && warmMultiple,
                "mapping witness begins with real executable multiple authority");
            var token = warm.Token; warm.Dispose();
            TranslationLease held = default;
            ulong pendingEpoch = 0;
            int callbacks = 0;
            f.MappingRevoked = (address, size) =>
            {
                ++callbacks;
                Check(address == (long)pc && size == 4096 && f.MappingBorrow(0),
                    "real mapping callback retains writer scope before PTE publication");
                Check(JitPageIdentity.TryRead(f.Program, f.Machine(), pc, out ulong oldPte) &&
                    (oldPte & (ulong)Blink.PAGE_XD) == 0, "deterministic pause sees old executable PTE");
                f.DataScope(1, () =>
                {
                    Check(f.Acquire(pc, 1, out held, out bool cachedMultiple, out int cachedError) &&
                        cachedError == 0 && held.Token == token && !cachedMultiple,
                        "cached body revalidates without multiple authority while mapping writer is pending");
                    pendingEpoch = f.Jit.Store.GetHeaderCopy(in held).ValidatedGlobalEpoch;
                    Check(f.Acquire(pc + 16, 1, out var fresh, out bool freshMultiple, out int freshError) &&
                        freshError == 0 && !freshMultiple, "fresh compilation cannot gain multiple authority before PTE write");
                    using (fresh)
                    {
                        f.Jit.AuthorizeLinks(in fresh, freshMultiple);
                        var header = f.Jit.Store.GetHeaderCopy(in fresh);
                        Check(header.Reserved136 == 0 && header.Link0TargetHeader == 0 && header.Link1TargetHeader == 0,
                            "pending mapping body publishes no native multiple links");
                    }
                    Check(f.Acquire(pc + 4096, 1, out var unrelated, out bool unrelatedMultiple, out _) && !unrelatedMultiple,
                        "any pending mapping writer suppresses owner-wide multiple admission");
                    unrelated.Dispose();
                    ulong before = f.Completed;
                    Check(f.Step(pc, 1) && f.Completed == before + 1 && f.Machine(1)->ip == pc + 5 && FullRax(f, 1) == 1,
                        "pending writer permits exactly one already checked snapshot instruction");
                });
                Check(f.MappingBorrow(0), "reader work does not release mapping writer ownership");
            };
            try
            {
                Check(f.Mutate(operation, pc) == 0 && callbacks == 1 && !f.MappingBorrow(0) && f.Jit.Authority.CodeEpoch > pendingEpoch,
                    "real " + operation + " completes PTE mutation before final scope epoch");
                f.At(pc, 1); FullRax(f, 0xfeedface, 1);
                ulong beforeIssued = f.Issued, beforeCompleted = f.Completed;
                var frame = f.InvokeRetained(in held, pendingEpoch);
                Check(frame.Exit == FullBlockExit.Invalidated && frame.Executed == 0 && frame.FirstPreissued == 1 &&
                    f.Issued == beforeIssued && f.Completed == beforeCompleted && FullRax(f, 1) == 0xfeedface && f.Machine(1)->ip == pc,
                    "completed " + operation + " rejects saved native body before stale effects");
                if (operation == "MAP_FIXED")
                {
                    f.Fill(pc); f.WriteMov(pc, 0x87654321);
                    f.Machine(1)->attention = true;
                    Check(f.Step(pc, 1) && FullRax(f, 1) == 0x87654321 && f.Machine(1)->ip == pc + 5,
                        "replacement page runs newly fetched bytes through real controller");
                    Check(f.Statistics.ContentInvalidations > 0, "replacement causes content retirement rather than stale binding reuse");
                }
                else
                {
                    Check(!f.Acquire(pc, 1, out var denied, out bool multiple, out int error) && error != 0 && !multiple,
                        "completed " + operation + " fails ordinary checked guest instruction fetch");
                    denied.Dispose();
                }
            }
            finally { held.Dispose(); }
        }
    }

    static void CheckFullControllerMappingScopeNesting()
    {
        using var f = new ControllerFixture(); ulong pc = ControllerFixture.Base;
        f.Map(pc); f.Fill(pc);
        // These are scope/counter API contracts, distinct from the actual
        // guest-PTE transaction pauses in CheckFullControllerMappingTransactions.
        f.DataScope(0, () =>
        {
            f.NotifyMapping(pc, 0);
            f.DataScope(0, () => f.NotifyMapping(pc, 0));
            Check(f.MappingBorrow(0), "recursive mapping scope preserves the outer worker's pending mutation");
            f.DataScope(1, () =>
            {
                f.NotifyMapping(pc, 1);
                Check(f.MappingBorrow(0) && f.MappingBorrow(1), "overlapping workers own independent pending mapping scopes");
            });
            Check(f.MappingBorrow(0) && !f.MappingBorrow(1), "releasing one mapping writer preserves another writer");
            Check(f.Acquire(pc, 0, out var lease, out bool multiple, out _) && !multiple,
                "remaining mapping writer still denies multiple admission after another writer releases");
            lease.Dispose();
        });
        Check(!f.MappingBorrow(0) && !f.MappingBorrow(1), "last outer mapping scope clears worker mutation flags");
        try
        {
            f.DataScope(0, () => { f.NotifyMapping(pc, 0); throw new InvalidOperationException("mapping unwind"); });
        }
        catch (InvalidOperationException error) when (error.Message == "mapping unwind") { }
        Check(f.Acquire(pc, 1, out var recovered, out bool restored, out _) && restored && !f.MappingBorrow(0),
            "mapping-scope finally ends pending mutation on a throwing consumer");
        recovered.Dispose();
    }

    static void CheckFullControllerPhysicalBorrows()
    {
        using var f = new ControllerFixture();
        ulong pc = ControllerFixture.Base;
        f.Map(pc); f.Fill(pc);
        // The first code registration deliberately follows the data exposure.
        f.DataScope(0, () =>
        {
            f.Expose(pc, 0);
            Check(f.Jit.Authority.ActiveBorrows == 1, "first physical borrower enrolled before code registration");
            f.DataScope(0, () =>
            {
                f.Expose(pc, 0);
                Check(f.Jit.Authority.ActiveBorrows == 1, "recursive same-worker exposure deduplicates physical page");
            });
            Check(f.Jit.Authority.ActiveBorrows == 1, "inner data scope preserves outer physical borrow");
            f.DataScope(1, () =>
            {
                f.Expose(pc, 1);
                Check(f.Jit.Authority.ActiveBorrows == 2, "two workers retain independent nodes for one physical page");
                Check(f.Acquire(pc, 1, out var lease, out bool multiple, out _) && !multiple,
                    "first code registration during overlapping data use fails closed");
                lease.Dispose();
            });
            Check(f.Jit.Authority.ActiveBorrows == 1, "releasing one worker cannot release another's physical borrow");
            Check(f.Acquire(pc, 0, out var borrowed, out bool stillMultiple, out _) && !stillMultiple,
                "last remaining physical borrower still prevents multiple authority");
            borrowed.Dispose();
        });
        Check(f.Jit.Authority.ActiveBorrows == 0 && f.Jit.Authority.PeakBorrows == 2,
            "outermost data scope finally releases all physical borrows");
        Check(f.Acquire(pc, 1, out var reusable, out bool restored, out _) && restored,
            "private code regains multiple authority after final release");
        reusable.Dispose();
        try
        {
            f.DataScope(0, () => { f.Expose(pc, 0); throw new InvalidOperationException("borrow unwind"); });
        }
        catch (InvalidOperationException error) when (error.Message == "borrow unwind") { }
        Check(f.Jit.Authority.ActiveBorrows == 0, "throwing data consumer releases physical node in finally");
        nint physical = f.Physical(pc);
        for (int warmup = 0; warmup < 5; ++warmup) _ = CountFullAuthorityBorrows(f.Jit.Authority, physical, f.WorkerId(0), f.WorkerId(1), out _);
        for (int sample = 0; sample < 3; ++sample)
        {
            long allocated = CountFullAuthorityBorrows(f.Jit.Authority, physical, f.WorkerId(0), f.WorkerId(1), out bool correct);
            Check(correct && allocated == 0, "warmed overlap/dedup/release authority metadata allocates zero managed bytes");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static long CountFullAuthorityBorrows(FullCodeAuthority authority, nint page, int first, int second, out bool correct)
    {
        correct = true; long start = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10000; ++i)
        {
            lock (authority.Gate)
            {
                authority.Expose(first, page); authority.Expose(first, page); authority.Expose(second, page);
                correct &= authority.ActiveBorrows == 2;
            }
            authority.EndBorrow(first); correct &= authority.ActiveBorrows == 1;
            authority.EndBorrow(second); correct &= authority.ActiveBorrows == 0;
        }
        return GC.GetAllocatedBytesForCurrentThread() - start;
    }

    static void CheckFullControllerSharedAliases()
    {
        using var f = new ControllerFixture();
        var opened = f.Io.CreateMemoryFile("full-controller-alias", 0);
        Check(opened.Succeeded, "controller shared alias memfd");
        int fd = opened.Value;
        byte[] code = new byte[4096]; code.AsSpan().Fill(0xcc);
        code[0] = 0xb8; BinaryPrimitives.WriteUInt32LittleEndian(code.AsSpan(1), 0xabcdef);
        code[5] = 0xeb; code[6] = 0xf9;
        Check(f.Io.WriteAt(fd, code, 0).Value == code.Length, "controller alias code backing initialized");
        ulong first = ControllerFixture.Base, alias = first + 0x10000;
        f.MapShared(fd, first); f.MapShared(fd, alias);
        Check(f.Acquire(first, 0, out var initial, out bool initialMultiple, out _) && initialMultiple,
            "exact observer-enrolled local memfd permits multiple authority");
        initial.Dispose();
        Check(f.Acquire(alias, 1, out var other, out bool aliasMultiple, out _) && aliasMultiple,
            "shared second alias compiles through controller");
        other.Dispose();
        Check(f.Physical(first) == f.Physical(alias), "real virtual aliases resolve to same physical backing");
        f.DataScope(0, () =>
        {
            f.Expose(first, 0);
            Check(f.Acquire(alias, 1, out var borrowed, out bool multiple, out _) && !multiple,
                "data borrow through one alias denies multiple execution through another");
            borrowed.Dispose();
        });
        int callbacks = 0;
        f.MappingRevoked = (_, _) => f.DataScope(1, () =>
        {
            ++callbacks;
            Check(f.Acquire(alias, 1, out var lease, out bool multiple, out _) && !multiple,
                "mapping mutation on another alias denies multiple admission while pending");
            lease.Dispose();
        });
        Check(f.Mutate("mprotect", first) == 0 && callbacks == 1,
            "real alias mprotect completes outside authority/native epoch");
        Check(f.Step(alias, 1, signalActor: true) && FullRax(f, 1) == 0xabcdef,
            "unmodified executable alias still runs exact shared backing after mapping scope");
    }

    static void CheckFullControllerAuthoritySaturation()
    {
        using var f = new ControllerFixture(); f.Map(ControllerFixture.Base); f.Fill(ControllerFixture.Base);
        nint page = f.Physical(ControllerFixture.Base);
        // Synthetic aligned identities below are never dereferenced. Node
        // exhaustion uses one real page and 17 independently numbered workers.
        using (var nodes = new FullCodeAuthority(f.Io, 16384, 17, 16384))
        {
            lock (nodes.Gate)
            {
                for (int i = 0; i < 16; ++i) nodes.Expose(i, page);
                Check(!nodes.Saturated && nodes.ActiveBorrows == 16, "bounded node pool fills without early saturation");
                nodes.Expose(16, page);
                Check(nodes.Saturated && !nodes.CanRunMultiple(f.Program, f.Machine(), ControllerFixture.Base, 7),
                    "overlapping-worker node exhaustion permanently fails closed");
            }
            for (int i = 0; i < 17; ++i) nodes.EndBorrow(i);
            Check(nodes.ActiveBorrows == 0 && nodes.Saturated, "saturated node registry still releases recorded borrows");
        }
        using (var cells = new FullCodeAuthority(f.Io, 16384, 1, 16384))
        {
            lock (cells.Gate)
            {
                for (int i = 1; i <= 17; ++i) cells.Expose(0, (nint)(i * 4096));
                Check(cells.Saturated && !cells.CanRunMultiple(f.Program, f.Machine(), ControllerFixture.Base, 7),
                    "physical identity registry exhaustion fails closed");
            }
            cells.EndBorrow(0);
            Check(cells.ActiveBorrows == 0, "identity saturation releases all recorded nodes");
        }
    }

    static void CheckFullControllerHostWriterEnrollment()
    {
        using var f = new ControllerFixture();
        f.Map(ControllerFixture.Base); f.Fill(ControllerFixture.Base);
        Check(f.Acquire(ControllerFixture.Base, 0, out var resident, out bool multiple, out _) && multiple,
            "host writer witness has validated controller body");
        resident.Dispose();
        var opened = f.Io.CreateMemoryFile("full-native-enrollment", 0);
        Check(opened.Succeeded && f.Io.WriteAt(opened.Value, "a"u8, 0).Value == 1, "writer witness initial unexposed bytes");
        var acquired = f.Io.AcquireMemoryFilePage(opened.Value, 0);
        Check(acquired.Succeeded, "writer witness published backing lease");
        var page = acquired.Value;
        var authority = f.Jit.Authority;
        ulong validated = authority.Epoch;
        Check(authority.TryEnterNative(validated), "validated native reader enrolls before writer");
        bool enrolled = true;
        Exception? failure = null;
        using var finished = new ManualResetEventSlim();
        var writer = new Thread(() =>
        {
            try { if (f.Io.WriteAt(opened.Value, "b"u8, 0).Value != 1) throw new InvalidOperationException("host writer failed"); }
            catch (Exception error) { failure = error; }
            finally { finished.Set(); }
        }) { IsBackground = true };
        writer.Start();
        try
        {
            Check(SpinWait.SpinUntil(() => authority.Epoch != validated, TimeSpan.FromSeconds(10)),
                "actual observed host write announces authority change");
            Check(!finished.IsSet && ((byte*)page.Address)[0] == (byte)'a',
                "host writer waits for enrolled bounded native reader before changing bytes");
            ulong writerEpoch = authority.Epoch;
            bool oldAdmission = authority.TryEnterNative(validated);
            if (oldAdmission) authority.ExitNative();
            bool currentAdmission = authority.TryEnterNative(authority.Epoch);
            if (currentAdmission) authority.ExitNative();
            Check(!oldAdmission && !currentAdmission, "pending writer denies both old-epoch and new-epoch native enrollment");
            authority.ExitNative(); enrolled = false;
            Check(finished.Wait(TimeSpan.FromSeconds(10)) && writer.Join(TimeSpan.FromSeconds(10)), "writer completes after native-reader release");
            if (failure != null) throw new InvalidOperationException("observed writer thread", failure);
            Check(((byte*)page.Address)[0] == (byte)'b' && authority.HostMutations > 0 && authority.EnrollmentMisses >= 2,
                "writer byte publication and enrollment misses retained");
            Check(authority.Epoch > writerEpoch, "normal IO writer completion publishes a new epoch after its begin edge");
            Check(authority.TryEnterNative(authority.Epoch), "completed writer permits freshly validated native enrollment");
            authority.ExitNative();
        }
        finally
        {
            if (enrolled) authority.ExitNative();
            if (!writer.Join(TimeSpan.FromSeconds(10))) throw new TimeoutException("host mutation did not drain");
            InstanceIo.ReleaseMemoryFilePage(page.Lease);
        }
        // Native frame count is enrolled synthetically to hold the barrier;
        // no deliberately stalled native instruction or IO/native callback is
        // added. R19 separately covers observer byte ordering and IO cases.
    }

    sealed class FullControllerSyntheticWriterAdmission : IHostMemoryFileMutation
    {
        private readonly FullTranslationJit jit;
        private int state;
        internal int ForwardedBegins, BeginCallbacks, EndCallbacks;
        internal bool UnexpectedRange, UnbalancedEnd;
        internal nint ExpectedPage;
        internal ulong AdmissionEpoch;

        internal FullControllerSyntheticWriterAdmission(FullTranslationJit jit) => this.jit = jit;

        internal void Preadmit(nint page)
        {
            // SYNTHETIC protocol ordering: admit outside InstanceIo.sync so
            // real PAGE_MUG fetches can acquire that gate before snapshot.
            // The actual IO Begin callback consumes this single admission;
            // its actual End forwards the matching completion exactly once.
            ExpectedPage = page;
            jit.BeginMemoryFileMutation(page, 4096, false);
            ++ForwardedBegins; AdmissionEpoch = jit.Authority.Epoch;
            Volatile.Write(ref state, 1);
        }

        public void BeginMemoryFileMutation(nint page, long length, bool logical)
        {
            if (Interlocked.CompareExchange(ref state, 2, 1) != 1)
                throw new InvalidOperationException("Synthetic writer has no single preadmitted Begin.");
            ++BeginCallbacks;
            UnexpectedRange |= page != ExpectedPage || length != 4096 || logical;
        }

        public void EndMemoryFileMutation()
        {
            if (Interlocked.Exchange(ref state, 0) != 2) { UnbalancedEnd = true; return; }
            jit.EndMemoryFileMutation();
            ++EndCallbacks;
        }

        internal void CancelUnconsumedAdmission()
        {
            if (Interlocked.CompareExchange(ref state, 0, 1) == 1) jit.EndMemoryFileMutation();
        }
    }

    static void CheckFullControllerWriterCompletionProtocol()
    {
        using var f = new ControllerFixture();
        ulong pc = ControllerFixture.Base;
        var opened = f.Io.CreateMemoryFile("full-completion-protocol", 0);
        Check(opened.Succeeded, "completion protocol memfd creation");
        int fd = opened.Value;
        byte[] code = new byte[4096]; code.AsSpan().Fill(0xcc);
        // NOP; MOV EAX,1; JMP back. The approved first instruction is
        // unchanged; the retained suffix is stale after the actual IO write.
        code[0] = 0x90; code[1] = 0xb8; code[2] = 1;
        code[3] = code[4] = code[5] = 0;
        code[6] = 0xeb; code[7] = 0xf8;
        Check(f.Io.WriteAt(fd, code, 0).Value == code.Length, "completion protocol initial shared bytes");
        f.MapShared(fd, pc);
        f.DataScope(0, () => f.Expose(pc, 0));
        nint physical = f.Physical(pc);
        Check(BinaryPrimitives.ReadUInt32LittleEndian(new((byte*)physical + 2, 4)) == 1 &&
            f.Jit.Store.GetStatistics().ResidentBodies == 0, "completion witness begins before the first real compilation");

        // This is NOT the production IO-gate race schedule: Compile resets
        // TLB/icache, then PAGE_MUG fetch calls MemoryFilePageAvailable, which
        // takes IO.sync. A real writer paused after Begin under that gate
        // blocks fetch BEFORE FullTranslationSnapshotTaken. We do not bypass
        // that lock. Instead split admission from the actual IO callback to
        // discriminate the completion epoch at the controller protocol level.
        var proxy = new FullControllerSyntheticWriterAdmission(f.Jit);
        using var writeRequested = new ManualResetEventSlim();
        using var finished = new ManualResetEventSlim();
        Exception? writerFailure = null;
        Thread? writer = null;
        bool proxyBound = false;
        f.Io.UnbindMemoryFileMutation(f.Jit);
        try
        {
            f.Io.BindMemoryFileMutation(proxy); proxyBound = true;
            proxy.Preadmit(physical);
            f.Counts(100);
            f.Machine()->attention = true; // stop after the same approved NOP on either attempt
            FullRax(f, 0xfeedfaceUL);
            byte[] replacement = [2, 0, 0, 0];
            writer = new Thread(() =>
            {
                try
                {
                    if (!writeRequested.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Snapshot did not request the IO write.");
                    if (f.Io.WriteAt(fd, replacement, 2).Value != replacement.Length) throw new InvalidOperationException("Suffix IO write failed.");
                }
                catch (Exception error) { writerFailure = error; }
                finally { finished.Set(); }
            }) { IsBackground = true };
            writer.Start();
            int snapshots = 0;
            f.Jit.TestSnapshotTaken = address =>
            {
                ++snapshots;
                ulong snapshotEpoch = f.Jit.Authority.Epoch;
                Check(address == pc && Monitor.IsEntered(f.Jit.Authority.Gate) && f.Issued == 1 && f.Completed == 0 &&
                    snapshotEpoch >= proxy.AdmissionEpoch && f.Jit.Store.GetStatistics().ActiveNativeEpochs == 0,
                    "synthetic writer admission precedes actual controller snapshot with no native frame yet");
                writeRequested.Set();
                Check(finished.Wait(TimeSpan.FromSeconds(10)) && writer.Join(TimeSpan.FromSeconds(10)),
                    "actual IO writer and forwarded End finish while snapshot hook holds authority gate");
                if (writerFailure != null) throw new InvalidOperationException("completion protocol writer", writerFailure);
                Check(proxy.ForwardedBegins == 1 && proxy.BeginCallbacks == 1 && proxy.EndCallbacks == 1 &&
                    !proxy.UnexpectedRange && !proxy.UnbalancedEnd && f.Jit.Authority.Epoch > snapshotEpoch &&
                    BinaryPrimitives.ReadUInt32LittleEndian(new((byte*)physical + 2, 4)) == 2,
                    "single synthetic admission plus actual IO End publishes changed suffix and completion epoch");
            };
            Check(f.Step(pc) && snapshots == 1 && f.Issued == 1 && f.Completed == 1 &&
                f.Machine()->ip == pc + 1 && FullRax(f) == 0xfeedfaceUL,
                "completion rejects the old snapshot, retries its same approved NOP, then observes attention");
            var original = f.ResumeToken();
            Check(f.Jit.Store.TryAcquireToken(original, out var currentSnapshot), "inspect freshly checked completion snapshot");
            using (currentSnapshot)
            {
                var plans = FullTranslationJit.Plans(f.Jit.Store, in currentSnapshot);
                var header = f.Jit.Store.GetHeaderCopy(in currentSnapshot);
                Check(plans.Length == 3 && plans[0].Decoded.length == 1 && plans[1].Decoded.op.uimm0 == 2 &&
                    header.ValidatedGlobalEpoch == f.Jit.Authority.CodeEpoch && f.NativePreparedCalls == 2,
                    "old MOV1 emission is rejected before issue and replaced by current MOV2 at completion freshness");
            }
            f.Machine()->attention = true;
            Check(f.Continue() && f.Issued == 2 && f.Completed == 2 &&
                f.Machine()->ip == pc + 6 && FullRax(f) == 2 && f.ResumeToken() == original,
                "next real controller resume executes freshly retained MOV2 once without replacing its body");
            var statistics = f.Statistics;
            Check(statistics.InlineNativeInstructions == 2 && statistics.SemanticHelperInstructions == 0 &&
                statistics.ContentInvalidations > 0 && statistics.InterpreterAttempts == 0,
                "synthetic completion protocol witness uses actual native controller and byte retirement without fallback");
        }
        finally
        {
            f.Jit.TestSnapshotTaken = null;
            writeRequested.Set();
            if (writer != null && !writer.Join(TimeSpan.FromSeconds(10)))
                throw new TimeoutException("Synthetic admission IO writer did not quiesce.");
            proxy.CancelUnconsumedAdmission();
            if (proxyBound) f.Io.UnbindMemoryFileMutation(proxy);
            f.Io.BindMemoryFileMutation(f.Jit);
        }
    }

    static void CheckFullControllerSegmentBaseRevalidation()
    {
        using var f = new ControllerFixture();
        ulong first = ControllerFixture.Base, second = first + 0x10000, logicalIp = 0x100;
        f.Map(first); f.Map(second); f.Fill(first); f.Fill(second);
        f.WriteMov(first + logicalIp, 0x12345678); f.WriteMov(second + logicalIp, 0x76543210);
        // Enroll both physical code pages before the measured base change.
        // First code registration itself advances the epoch in the native MMU
        // protocol; this test isolates same-epoch CS-base revalidation.
        nint secondPhysical = f.Physical(second);
        lock (f.Jit.Authority.Gate) f.Jit.Authority.PrepareCodeSnapshot(secondPhysical);
        var machine = f.Machine(); var savedMode = machine->mode;
        ulong savedBase = machine->cs.@base, savedIp = machine->ip;
        try
        {
            // Verified generated APIs: Machine.cs is a ref DescriptorCache,
            // its @base is ulong, and GetPc is cs.@base + MaskAddress(IP).
            machine->mode.omode = Blink.XED_MODE_LEGACY;
            machine->mode.genmode = Blink.XED_GEN_MODE_PROTECTED;
            machine->cs.@base = first;
            f.Counts(10); f.At(logicalIp);
            ulong context = FullTranslationJit.Context(f.Program, machine, 1);
            Check(f.Program.GetPc(machine) == (long)(first + logicalIp), "legacy code fetch uses actual CS base plus logical IP");
            Check(f.Step(logicalIp, signalActor: true) && FullRax(f) == 0x12345678 && machine->ip == logicalIp + 5,
                "legacy first CS base executes a real retained helper plan");
            var firstToken = f.ResumeToken();
            Check(f.Acquire(logicalIp, 0, out var lease, out bool multiple, out int error, requestedCount: 1) && error == 0 && multiple,
                "legacy resident has physical multiple eligibility independently of native scalar mode support");
            using (lease)
            {
                var header = f.Jit.Store.GetHeaderCopy(in lease);
                Check(lease.Token == firstToken && header.Reserved136 == 0 && header.Live == 1 &&
                    header.CodeEpoch == header.ValidatedCodeEpoch && header.ValidatedGlobalEpoch == f.Jit.Authority.CodeEpoch,
                    "legacy resident stays untrusted despite stable physical multiple eligibility");
                Check((header.Context & (1UL << 63)) == 0 && header.Link0TargetHeader == 0 && header.Link1TargetHeader == 0,
                    "legacy validated key cannot mint flat-long trust or native links");
            }
            ulong epoch = f.Jit.Authority.Epoch;
            machine->cs.@base = second;
            Check(FullTranslationJit.Context(f.Program, machine, 1) == context && machine->ip == logicalIp && f.Jit.Authority.Epoch == epoch &&
                f.Program.GetPc(machine) == (long)(second + logicalIp),
                "CS base change leaves exact logical key, mode context and PTE epoch unchanged");
            Check(f.Step(logicalIp, signalActor: true) && FullRax(f) == 0x76543210 && machine->ip == logicalIp + 5 &&
                f.ResumeToken() != firstToken && f.Jit.Authority.Epoch == epoch,
                "nonlong CS base change forces byte revalidation and executes the new page's MOV helper");
            var statistics = f.Statistics;
            Check(statistics.SemanticHelperInstructions == 2 && statistics.InlineNativeInstructions == 0 &&
                statistics.ContentInvalidations > 0 && statistics.InterpreterAttempts == 0 && f.Issued == 2 && f.Completed == 2,
                "segment-base regression replaces changed retained content without an epoch or interpreter escape");
        }
        finally
        {
            machine->mode = savedMode; machine->cs.@base = savedBase;
            f.At(savedIp);
        }
    }

    static void CheckFullControllerLongContextProvenance()
    {
        using var f = new ControllerFixture();
        ulong ip = ControllerFixture.Base, nonflatBase = 0x10000;
        f.Map(ip); f.Map(ip + nonflatBase); f.Fill(ip); f.Fill(ip + nonflatBase);
        f.WriteMov(ip, 2); f.WriteMov(ip + nonflatBase, 1);
        // Context provenance is tested without first-code-registration epochs.
        // Pre-enroll both mapped bodies before capturing the transition epoch.
        nint flatPhysical = f.Physical(ip), nonflatPhysical = f.Physical(ip + nonflatBase);
        lock (f.Jit.Authority.Gate)
        {
            f.Jit.Authority.PrepareCodeSnapshot(flatPhysical);
            f.Jit.Authority.PrepareCodeSnapshot(nonflatPhysical);
        }
        var machine = f.Machine();
        ulong savedBase = machine->cs.@base, savedIp = machine->ip;
        try
        {
            f.Counts(10);
            machine->cs.@base = nonflatBase; f.At(ip);
            ulong epoch = f.Jit.Authority.Epoch;
            ulong nonflatContext = FullTranslationJit.Context(f.Program, machine, 1);
            Check(machine->mode.omode == Blink.XED_MODE_LONG && machine->mode.genmode == Blink.XED_GEN_MODE_PROTECTED &&
                (nonflatContext & (1UL << 63)) == 0 && f.Program.GetPc(machine) == (long)(ip + nonflatBase),
                "nonflat long-mode witness uses verified Blink CS-based linear fetch and an untrusted context key");
            Check(f.Step(ip, signalActor: true) && FullRax(f) == 1 && machine->ip == ip + 5,
                "nonflat long mode first executes MOV1 through the actual retained helper controller");
            var nonflat = f.ResumeToken();
            Check(f.Jit.Store.TryAcquireToken(nonflat, out var nonflatLease), "nonflat long-mode resident lease");
            using (nonflatLease)
            {
                var header = f.Jit.Store.GetHeaderCopy(in nonflatLease);
                Check(header.Context == nonflatContext && header.Reserved136 == 0 &&
                    header.Link0TargetHeader == 0 && header.Link1TargetHeader == 0,
                    "actual nonflat controller authorization never mints flat trust or native links");
            }

            machine->cs.@base = 0; f.At(ip);
            ulong flatContext = FullTranslationJit.Context(f.Program, machine, 1);
            Check((flatContext ^ nonflatContext) == (1UL << 63) && f.Jit.Authority.Epoch == epoch &&
                f.Program.GetPc(machine) == (long)ip,
                "nonflat-to-flat long transition changes only context bit63 with identical logical IP and mapping epoch");
            Check(f.Step(ip, signalActor: true) && FullRax(f) == 2 && machine->ip == ip + 5 && f.ResumeToken() != nonflat,
                "nonflat-to-flat long transition cannot reuse stale MOV1 helper content for the MOV2 page");
            var flat = f.ResumeToken();
            Check(f.Jit.Store.TryAcquireToken(flat, out var flatLease), "flat long-mode resident lease");
            Check(f.Jit.Store.TryAcquireToken(nonflat, out var oldLease), "nonflat key remains independently resident");
            using (flatLease)
            using (oldLease)
            {
                var flatHeader = f.Jit.Store.GetHeaderCopy(in flatLease);
                var nonflatHeader = f.Jit.Store.GetHeaderCopy(in oldLease);
                Check(flatHeader.Context == flatContext && flatHeader.Reserved136 == 1 && nonflatHeader.Reserved136 == 0 &&
                    flatHeader.GuestPc == nonflatHeader.GuestPc && flatHeader.ValidatedGlobalEpoch == nonflatHeader.ValidatedGlobalEpoch,
                    "actual flat Step positively mints trusted provenance while same-PC nonflat key stays untrusted");
                Check(!f.Jit.Store.TryLink(flat, 0, nonflat) && !f.Jit.Store.TryLink(nonflat, 0, flat) &&
                    !f.Jit.Store.TryLink(flat, 1, nonflat) && !f.Jit.Store.TryLink(nonflat, 1, flat),
                    "native links reject same-PC targets from the wrong flat/nonflat context in both directions");
            }
            machine->cs.@base = nonflatBase;
            Check(f.Step(ip, signalActor: true) && FullRax(f) == 1 && f.ResumeToken() == nonflat && f.Jit.Authority.Epoch == epoch,
                "reverse flat-to-nonflat transition reuses its correct helper key without executing old flat inline IR");
            machine->cs.@base = 0;
            Check(f.Step(ip, signalActor: true) && FullRax(f) == 2 && f.ResumeToken() == flat && f.Jit.Authority.Epoch == epoch,
                "return to flat context reuses the correct trusted native body after nonflat helper execution");
            var statistics = f.Statistics;
            Check(statistics.ActualBlockCompilations == 2 && statistics.SemanticHelperInstructions == 2 &&
                statistics.InlineNativeInstructions == 2 && statistics.ContentInvalidations == 0 &&
                statistics.InterpreterAttempts == 0 && f.Issued == 4 && f.Completed == 4,
                "flat/nonflat round trips preserve two fitting exact keys and four exact helper/native completions");
        }
        finally { machine->cs.@base = savedBase; f.At(savedIp); }
    }

    static void CheckFullControllerAllocationCategories()
    {
        object[] escaped = new object[7];
        for (int warmup = 0; warmup < 5; ++warmup)
            _ = MeasureFullNestedAllocation(new FullAllocationMeter(1), escaped, out _, out _, out _);
        for (int sample = 0; sample < 3; ++sample)
        {
            var meter = new FullAllocationMeter(1);
            long allocated = MeasureFullNestedAllocation(meter, escaped, out long helper, out long compilation, out long publication);
            Check(helper > 0 && compilation > 0 && publication > 0 &&
                meter.Total(FullAllocationMeter.Category.Helper) == helper &&
                meter.Total(FullAllocationMeter.Category.Compilation) == compilation &&
                meter.Total(FullAllocationMeter.Category.Publication) == publication,
                "nested Helper-Compilation-Publication-Helper allocates exclusively into the active production category");
            Check(allocated == helper + compilation + publication && meter.Total(FullAllocationMeter.Category.None) == 0,
                "escaped known CLR allocations equal summed exclusive totals without double counting");
        }
        GC.KeepAlive(escaped);
        // This tests the production meter API with escaped host allocations.
        // Setup, diagnostics, native reservation and guest malloc are separate
        // scopes/resources; these values are not a whole-process allocation sum.
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static long MeasureFullNestedAllocation(FullAllocationMeter meter, object[] escaped,
        out long helper, out long compilation, out long publication)
    {
        helper = compilation = publication = 0;
        long start = GC.GetAllocatedBytesForCurrentThread();
        using (meter.Enter(0, FullAllocationMeter.Category.Helper))
        {
            helper += EscapeFullMeterAllocation(escaped, 0, 101);
            using (meter.Enter(0, FullAllocationMeter.Category.Compilation))
            {
                compilation += EscapeFullMeterAllocation(escaped, 1, 203);
                using (meter.Enter(0, FullAllocationMeter.Category.Publication))
                {
                    publication += EscapeFullMeterAllocation(escaped, 2, 307);
                    using (meter.Enter(0, FullAllocationMeter.Category.Helper))
                        helper += EscapeFullMeterAllocation(escaped, 3, 409);
                    publication += EscapeFullMeterAllocation(escaped, 4, 503);
                }
                compilation += EscapeFullMeterAllocation(escaped, 5, 601);
            }
            helper += EscapeFullMeterAllocation(escaped, 6, 701);
        }
        return GC.GetAllocatedBytesForCurrentThread() - start;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static long EscapeFullMeterAllocation(object[] escaped, int index, int bytes)
    {
        long start = GC.GetAllocatedBytesForCurrentThread();
        escaped[index] = new byte[bytes];
        return GC.GetAllocatedBytesForCurrentThread() - start;
    }

    static void CheckFullControllerCollidersAndPressure()
    {
        var f = new ControllerFixture();
        try
        {
            ulong first = ControllerFixture.Base;
            const int bytes = 256 << 10;
            f.Map(first, bytes); f.Fill(first, bytes);
            ulong context = FullTranslationJit.Context(f.Program, f.Machine());
            var keys = new List<ulong>();
            ulong bucket = RetainedTranslationStore.Hash(new(first, context)) & 255;
            for (ulong pc = first; pc < first + bytes && keys.Count < 12; pc += 16)
                if ((RetainedTranslationStore.Hash(new(pc, context)) & 255) == bucket) keys.Add(pc);
            Check(keys.Count == 12, "controller fixture supplies twelve exact colliders in default 256 worker hints");
            var tokens = new TranslationToken[keys.Count];
            for (int i = 0; i < keys.Count; ++i)
            {
                Check(f.Acquire(keys[i], 0, out var lease, out _, out int error) && error == 0, "controller collider first compilation");
                tokens[i] = lease.Token; lease.Dispose();
            }
            long compiles = f.Statistics.ActualBlockCompilations;
            Check(compiles == keys.Count, "actual controller compiles one body for each collider");
            for (int phase = 0; phase < 12; ++phase)
            for (int pass = 0; pass < 8; ++pass)
            for (int i = 0; i < keys.Count; ++i)
            {
                int index = (i + phase) % keys.Count;
                Check(f.Acquire(keys[index], pass & 1, out var lease, out _, out _) && lease.Token == tokens[index],
                    "owner-wide exact collider key survives worker and phase changes");
                lease.Dispose();
            }
            Check(f.Statistics.ActualBlockCompilations == compiles && f.Statistics.BudgetEvictions == 0,
                "fitting controller colliders cause no unchanged retranslation");

            int pressureKeys = Math.Min(bytes / 16, Math.Max(512, f.BodyCapacity * 3));
            for (int i = 0; i < pressureKeys; ++i)
            {
                Check(f.Acquire(first + (uint)i * 16, i & 1, out var lease, out _, out int error) && error == 0,
                    "small fitting controller continues compiling under retention pressure");
                lease.Dispose();
            }
            var pressured = f.Statistics;
            var store = f.Jit.Store.GetStatistics();
            Check(store.TotalFlushes > 0 && store.FlushRetirements > 0 && store.CodePageReuses > 0 && pressured.CacheBytes > 0 &&
                pressured.CacheBytes <= ControllerFixture.Budget && pressured.PeakCacheBytes <= ControllerFixture.Budget,
                "controller flushes and reuses arenas inside explicit 2 MiB charged quota");
            // Model another worker's already-entered bounded frame by holding
            // its real store epoch. Do no guest helper execution in this epoch.
            // A compiling worker must report retention denial when reclamation
            // needs that frame to finish; no old-engine fallback is acceptable.
            ulong exhaustionStart = first + bytes;
            f.Map(exhaustionStart, bytes); f.Fill(exhaustionStart, bytes);
            Check(f.Jit.Store.TryBeginNativeEpoch(out var heldEpoch), "controller pressure witness native epoch");
            bool denied = false; ulong deniedPc = 0;
            ulong busyEpoch = f.Jit.Authority.Epoch;
            long attemptsBeforeBusy = f.Statistics.CompileAttempts;
            try
            {
                int limit = Math.Min(bytes / 16, f.BodyCapacity + 1);
                for (int i = 0; i < limit; ++i)
                {
                    deniedPc = exhaustionStart + (uint)i * 16;
                    if (!f.Acquire(deniedPc, 1, out var lease, out _, out int pendingError))
                    {
                        Check(pendingError == 0, "arena writer contention is a retry without guest fault");
                        denied = true; break;
                    }
                    lease.Dispose();
                }
                Check(denied && f.Completed == 0 && f.Issued == 0 && f.Statistics.InterpreterAttempts == 0 &&
                    f.Jit.Authority.Epoch > busyEpoch && f.Statistics.CompileAttempts == attemptsBeforeBusy &&
                    f.Statistics.CacheBytes <= ControllerFixture.Budget,
                    "controller requests epoch quiescence before repeated compilation without guest issue, fallback, or quota growth");
            }
            finally { heldEpoch.Dispose(); }
            Check(f.Acquire(deniedPc, 1, out var recovered, out _, out int recoveryError) && recoveryError == 0 &&
                f.Jit.Store.GetStatistics().ActiveNativeEpochs == 0,
                "controller recompilation recovers after blocking native epoch releases");
            recovered.Dispose();
            ulong phaseStart = first + (uint)(pressureKeys - 12) * 16;
            for (int pass = 0; pass < 64; ++pass)
            for (int i = 0; i < 12; ++i)
            {
                Check(f.Acquire(phaseStart + (uint)i * 16, pass & 1, out var lease, out _, out _), "controller new fitting phase converges");
                lease.Dispose();
            }
            var settled = f.Statistics;
            for (int pass = 0; pass < 16; ++pass)
            for (int i = 0; i < 12; ++i)
            {
                Check(f.Acquire(phaseStart + (uint)i * 16, pass & 1, out var lease, out _, out _), "controller stable pressure phase hits");
                lease.Dispose();
            }
            Check(f.Statistics.ActualBlockCompilations == settled.ActualBlockCompilations &&
                f.Statistics.BudgetEvictions == settled.BudgetEvictions,
                "stable fitting controller phase stops capacity retranslation");
            Check(f.Step(phaseStart, signalActor: true) && f.Completed == 1 && FullRax(f) == (uint)(phaseStart - first + 1),
                "retained pressure survivor executes through actual controller");
            var before = f.Statistics;
            Check(before.PeakCacheBytes == before.CacheBytes && before.PeakCacheBytes > f.Jit.Authority.Charge + f.Jit.Memory.Charge,
                "live controller peak includes retained store and fixed resources");
            f.Dispose();
            var after = f.Statistics;
            Check(after.CacheBytes == 0 && after.PeakCacheBytes == before.PeakCacheBytes && after.PeakCacheBytes > 0 &&
                after.ResidentTranslationBlocks == 0 && after.ActualBlockCompilations == before.ActualBlockCompilations,
                "disposed controller clears current charge and residency while preserving full positive peak and compile history");
        }
        finally { f.Dispose(); }
    }
}

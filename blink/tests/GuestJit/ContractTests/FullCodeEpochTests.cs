using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using ControllerFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // Authored, UNRUN by R80. Real emitted guards/MMU paths are separate from
    // explicitly synthetic pending-writer schedules. No timing assertion.
    static void CheckFullCodeEpochs(byte* machine)
    {
        CheckFullCodeEpochRetention();
        CheckFullCodeCopyCompletion();
        CheckFullFirstCodeCopySnapshot();
        CheckFullCodePendingCompletion();
        CheckFullCodeEpochBounds();
        CheckFullSeparateDataEpoch(machine);
        CheckFullSeparateTransferEpochs(machine);
        CheckFullCodeEpochEncoding();
    }

    static long AuthorityCount(GuestJitStatistics s, string key) => s.FullAuthorityCounts![key];

    static void CheckFullCodeEpochRetention()
    {
        using var f = new ControllerFixture();
        ulong source = ControllerFixture.Base, target = source + 0x10000, data = target + 0x10000, firstCode = data + 0x10000;
        foreach (ulong pc in new[] { source, target, data, firstCode }) { f.Map(pc); f.Fill(pc); }
        byte[] jump = [0x90, 0xe9, 0, 0, 0, 0];
        BinaryPrimitives.WriteInt32LittleEndian(jump.AsSpan(2), checked((int)(target - source - 6)));
        WriteControllerCode(f, source, jump);
        nint sourcePage = f.Physical(source), targetPage = f.Physical(target), dataPage = f.Physical(data), newPage = f.Physical(firstCode);
        lock (f.Jit.Authority.Gate)
        {
            f.Jit.Authority.PrepareCodeSnapshot(sourcePage);
            f.Jit.Authority.PrepareCodeSnapshot(targetPage);
        }
        Check(f.Acquire(target, 0, out var destination, out bool targetMultiple, out _) && targetMultiple,
            "separate epochs warm a fitting target");
        using (destination) f.Jit.AuthorizeLinks(in destination, targetMultiple);
        Check(f.Acquire(source, 0, out var body, out bool multiple, out _) && multiple, "separate epochs warm a fitting source");
        using (body)
        {
            f.Jit.AuthorizeLinks(in body, multiple);
            var header = f.Jit.Store.GetHeaderCopy(in body);
            Check(header.Link1TargetHeader != 0 && header.Reserved136 == 1, "positive trusted retained link prerequisite");
            ulong codeEpoch = f.Jit.Authority.CodeEpoch, runtime = f.Jit.Authority.Epoch;
            var before = f.Statistics;
            f.DataScope(0, () => f.Expose(data, 0));
            Check(f.Jit.Authority.Epoch > runtime && f.Jit.Authority.CodeEpoch == codeEpoch,
                "known non-code C pointer still pulses runtime while retaining code freshness");
            var copies = (IHostGuestMemoryCopies)f.Owner;
            runtime = f.Jit.Authority.Epoch;
            f.DataScope(0, () =>
            {
                copies.BeginGuestWrite(dataPage, 1);
                try { ((byte*)dataPage)[0] = 0x5a; }
                finally { copies.EndGuestWrite(); }
            });
            Check(f.Jit.Authority.Epoch > runtime && f.Jit.Authority.CodeEpoch == codeEpoch,
                "known non-code actual copy retains both runtime start/completion pulses without staling TBs");
            runtime = f.Jit.Authority.Epoch;
            lock (f.Jit.Authority.Gate) Check(f.Jit.Authority.PrepareCodeSnapshot(newPage), "first code registration");
            Check(f.Jit.Authority.Epoch > runtime && f.Jit.Authority.CodeEpoch == codeEpoch,
                "first code registration drains cached data writes without changing unrelated code freshness");
            Check(f.Acquire(source, 0, out var reused, out multiple, out _) && multiple, "resident source survives runtime-only changes");
            using (reused)
            {
                var after = f.Statistics;
                var live = f.Jit.Store.GetHeaderCopy(in reused);
                Check(reused.Token == body.Token && live.Entry == header.Entry && live.Live == 1 &&
                    live.ValidatedGlobalEpoch == codeEpoch && live.Link1TargetHeader == header.Link1TargetHeader,
                    "runtime changes retain exact code entry, generation, trust stamp and armed data link");
                Check(AuthorityCount(after, "TrustedAcquisitions") == AuthorityCount(before, "TrustedAcquisitions") + 1 &&
                    AuthorityCount(after, "ByteValidations") == AuthorityCount(before, "ByteValidations") &&
                    AuthorityCount(after, "Restamps") == AuthorityCount(before, "Restamps") &&
                    after.ActualBlockCompilations == before.ActualBlockCompilations && after.EmittedCodeBytes == before.EmittedCodeBytes,
                    "measured counters discriminate trusted acquisition from byte validation, restamping and compilation");
                f.At(source, 1); f.Counts(4, issued: 1);
                var frame = f.InvokeRetained(in reused, codeEpoch);
                Check(frame.Exit == FullBlockExit.Budget && frame.Executed == 4 && frame.TailJumps == 1 &&
                    f.Issued == 5 && f.Completed == 4 && frame.Header->GuestPc == target,
                    "existing native link continues after data pulses with exact rejected budget issuance");
            }
        }
    }

    static void CheckFullCodeCopyCompletion()
    {
        using var f = new ControllerFixture();
        ulong pc = ControllerFixture.Base;
        f.Map(pc); f.Fill(pc);
        Check(f.Acquire(pc, 0, out var warm, out bool multiple, out _) && multiple, "code-copy completion warm body");
        TranslationToken token = warm.Token;
        using (warm) f.Jit.AuthorizeLinks(in warm, multiple);
        nint physical = f.Physical(pc);
        var copies = (IHostGuestMemoryCopies)f.Owner;
        TranslationLease pending = default;
        ulong snapshotEpoch = 0, beforeCode = f.Jit.Authority.CodeEpoch;
        f.DataScope(0, () =>
        {
            copies.BeginGuestWrite(physical + 1, 4);
            try
            {
                Check(f.Jit.Authority.CodeEpoch > beforeCode, "known physical code copy announces freshness before bytes");
                Check(f.Acquire(pc, 1, out pending, out bool pendingMultiple, out _) && !pendingMultiple && pending.Token == token,
                    "old bytes may be revalidated during a pending actual copy, without multiple authority");
                snapshotEpoch = f.Jit.Store.GetHeaderCopy(in pending).ValidatedGlobalEpoch;
                BinaryPrimitives.WriteUInt32LittleEndian(new((byte*)physical + 1, 4), 2);
            }
            finally { f.At(pc, 0); copies.EndGuestWrite(); }
        });
        using (pending)
        {
            Check(f.Jit.Authority.CodeEpoch > snapshotEpoch && f.Jit.Authority.KnownCodeWriteScopes > 0,
                "known code copy completion stales revalidation of its old bytes");
            f.At(pc, 1); f.Counts(1, issued: 1); FullRax(f, 0xfeedface, 1);
            var frame = f.InvokeRetained(in pending, snapshotEpoch);
            Check(frame.Exit == FullBlockExit.Invalidated && frame.Executed == 0 && frame.FirstPreissued == 1 &&
                f.Completed == 0 && f.Issued == 1 && FullRax(f, 1) == 0xfeedface,
                "actual native entry rejects old code-copy body before issue or architectural effects");
        }
        Check(f.Acquire(pc, 0, out var current, out _, out _), "changed code acquires after actual copy completion");
        using (current) Check(current.Token != token && f.Statistics.ContentInvalidations == 1 &&
            FullTranslationJit.Plans(f.Jit.Store, in current)[0].Decoded.op.uimm0 == 2,
            "changed bytes retire old content and preserve the current decoded value");
    }

    static void CheckFullCodePendingCompletion()
    {
        foreach (string kind in new[] { "mapping", "logical-host", "unknown-copy" })
        {
            using var f = new ControllerFixture();
            ulong pc = ControllerFixture.Base;
            f.Map(pc); f.Fill(pc);
            Check(f.Acquire(pc, 0, out var warm, out bool multiple, out _) && multiple, "pending freshness warm body");
            using (warm) f.Jit.AuthorizeLinks(in warm, multiple);
            var a = f.Jit.Authority;
            ulong before = a.CodeEpoch;
            if (kind == "mapping") { a.BeginMappingMutation(); f.Jit.RevokeMapping((long)pc, 4096); }
            else if (kind == "logical-host") f.Jit.BeginMemoryFileMutation(0, 0, logical: true);
            else
            {
                a.BeginGuestWrite(0);
                lock (a.Gate) a.NoteGuestWritePages(0, (nint)0x12345000);
            }
            TranslationLease pending = default;
            ulong snapshot = 0;
            try
            {
                Check(a.CodeEpoch > before, "pending code/mapping mutation has a begin edge: " + kind);
                Check(f.Acquire(pc, 1, out pending, out multiple, out _) && !multiple,
                    "pending mutation validates old body without trusted multiple execution: " + kind);
                snapshot = f.Jit.Store.GetHeaderCopy(in pending).ValidatedGlobalEpoch;
            }
            finally
            {
                if (kind == "mapping") a.EndMappingMutation();
                else if (kind == "logical-host") f.Jit.EndMemoryFileMutation();
                else a.EndGuestWrite(0);
            }
            using (pending)
            {
                // Protocol-only pause: no PTE/host bytes are actually changed.
                // Actual PTE/alias/IO mutation coverage remains in the existing
                // controller/provenance tests, with their original limits.
                Check(a.CodeEpoch > snapshot, "completion invalidates old pending revalidation: " + kind);
                f.At(pc, 1); f.Counts(1, issued: 1);
                var frame = f.InvokeRetained(in pending, snapshot);
                Check(frame.Exit == FullBlockExit.Invalidated && frame.Executed == 0 && f.Completed == 0 &&
                    f.Issued == 1 && frame.FirstPreissued == 1, "completion rejects native saved body: " + kind);
            }
            Check(f.Acquire(pc, 0, out var refreshed, out multiple, out _) && multiple,
                "completed unchanged mutation restamps without compilation: " + kind);
            using (refreshed) Check(f.Statistics.ActualBlockCompilations == 1 && f.Statistics.ContentInvalidations == 0,
                "completion freshness preserves unchanged resident emission: " + kind);
        }
    }

    static void CheckFullFirstCodeCopySnapshot()
    {
        using var f = new ControllerFixture();
        ulong pc = ControllerFixture.Base;
        f.Map(pc); f.Fill(pc); // registered writable data, no code snapshot yet
        nint physical = f.Physical(pc);
        ulong codeEpoch = f.Jit.Authority.CodeEpoch;
        var copies = (IHostGuestMemoryCopies)f.Owner;
        copies.BeginGuestWrite(physical + 1, 4);
        TranslationLease old = default;
        try
        {
            Check(f.Acquire(pc, 1, out old, out bool multiple, out _) && !multiple &&
                f.Jit.Store.GetHeaderCopy(in old).ValidatedGlobalEpoch == 0,
                "first code registration during an older non-code copy cannot publish executable freshness");
            BinaryPrimitives.WriteUInt32LittleEndian(new((byte*)physical + 1, 4), 2);
        }
        finally { f.At(pc, 0); copies.EndGuestWrite(); }
        using (old)
        {
            Check(f.Jit.Authority.CodeEpoch == codeEpoch, "first registration/copy completion does not stale unrelated code");
            f.At(pc, 1); f.Counts(1, issued: 1);
            var rejected = f.InvokeRetained(in old, 0);
            Check(rejected.Exit == FullBlockExit.Invalidated && rejected.Executed == 0 && f.Issued == 1 && f.Completed == 0,
                "post-copy current runtime observation cannot authorize first-registration old bytes");
        }
        Check(f.Acquire(pc, 0, out var current, out bool currentMultiple, out _) && currentMultiple,
            "stable first-code bytes regain native authority after the bounded copy");
        using (current) Check(FullTranslationJit.Plans(f.Jit.Store, in current)[0].Decoded.op.uimm0 == 2 &&
            f.Jit.Store.GetHeaderCopy(in current).ValidatedGlobalEpoch == codeEpoch,
            "fresh validation observes the actual completed copy value");
    }

    static void CheckFullCodeEpochBounds()
    {
        using var ioCleanup = new SemanticTestIo();
        using (var a = new FullCodeAuthority(ioCleanup.Value, 4096, 1, 16384))
        {
            ulong code = a.CodeEpoch;
            lock (a.Gate)
            {
                for (int i = 1; i <= 17; ++i) a.Expose(0, (nint)(4096 * i));
                Check(a.Saturated && a.CodeEpoch > code, "unknown registry saturation fails closed for code freshness");
            }
            a.EndBorrow(0);
        }
        using (var a = new FullCodeAuthority(ioCleanup.Value, 4096, 1, 16384))
        {
            // Independent code-word exhaustion never wraps or reopens native
            // admission; host completion remains nonthrowing and balanced.
            a.BeginHostWriter();
            a.CodeEpochPointer[0] = ulong.MaxValue;
            a.EndHostWriter();
            Check(a.CodeEpoch == ulong.MaxValue && a.SemanticQuarantined && a.HostWriters == 0 && !a.TryEnterNative(a.Epoch),
                "code epoch completion exhaustion quarantines without wrap or writer leak");
        }
    }

    static void CheckFullSeparateDataEpoch(byte* machine)
    {
        byte* data = (byte*)NativeMemory.AllocZeroed(4096);
        try
        {
            Write64(data, 0, 0x123456789abcdef0);
            FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
            new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
            SetFullMemoryEntry(tlb, Page, data, epoch: 71);
            using var code = CreateFullBlock([new(new(Ip, 3, JitOperation.LoadMov, 64, new(0, 0),
                new(0, 0, IsMemory: true), SourceWidth: 64, Address: new(3, -1, 0, 0)))]);
            ulong runtime = 71, codeEpoch = 19;
            var header = FullHeader(Ip, code.Entry); header.ValidatedGlobalEpoch = codeEpoch;
            long issued = 0, completed = 0; int stop = 0;
            ResetFull(machine); Write64(machine, 48, Page); Write64(machine, 24, 0);
            var frame = FullFrame(machine, &header, &runtime, &issued, &completed, &stop, limit: 1, codeEpoch: &codeEpoch);
            frame.MemoryTlb = tlb; frame.MemoryTlbMask = 63;
            Check(RunFull(code, &frame) == FullBlockExit.Continue && completed == 1 && issued == 1,
                "native code and data epochs may have unrelated values");
            ++runtime; issued = completed = 0;
            ResetFull(machine); Write64(machine, 48, Page); Write64(machine, 24, 0);
            frame = FullFrame(machine, &header, &runtime, &issued, &completed, &stop, limit: 1, codeEpoch: &codeEpoch);
            frame.MemoryTlb = tlb; frame.MemoryTlbMask = 63;
            Check(RunFull(code, &frame) == FullBlockExit.NeedMemory && completed == 0 && issued == 1 && ((ulong*)machine)[3] == 0,
                "runtime pulse preserves code guard but rejects stale data TLB before load effects");
            SetFullMemoryEntry(tlb, Page, data, epoch: runtime);
            FullNativeMemory.ResumeSameIssue(&frame);
            Check(RunFull(code, &frame) == FullBlockExit.Continue && issued == 1 && completed == 1 && ((ulong*)machine)[3] == ((ulong*)data)[0],
                "fresh data mapping resumes the exact reserved issue with unchanged code stamp");
        }
        finally { NativeMemory.Free(data); }
    }

    static void CheckFullSeparateTransferEpochs(byte* machine)
    {
        const ulong target = Ip + 0x100;
        using var direct = CreateFullBlock([new(new(Ip, 5, JitOperation.Jump, 64, default, default, Target: target))]);
        using var indirect = CreateFullBlock([new(new(Ip, 2, JitOperation.IndirectJump, 64, default, new(3, 0)))]);
        using var destination = CreateFullBlock([
            new(new(target, 1, JitOperation.Inc, 64, new(0, 0), default)),
            new(new(target + 1, 2, JitOperation.Jump, 64, default, default, Target: target))]);
        FullNativeJumpEntry* jumps = stackalloc FullNativeJumpEntry[256];
        foreach (bool cache in new[] { false, true })
        foreach (string failure in new[] { "none", "target-code", "runtime", "code", "null-code", "abi3", "stop" })
        {
            ResetFull(machine); Write64(machine, 48, target); Write64(machine, 24, 0);
            ulong runtime = 72, codeEpoch = 19, generation = 31;
            var a = FullHeader(Ip, cache ? indirect.Entry : direct.Entry);
            var b = FullHeader(target, destination.Entry, 18);
            a.ValidatedGlobalEpoch = b.ValidatedGlobalEpoch = codeEpoch; b.Reserved136 = 1;
            a.Link1TargetHeader = (nint)(&b); a.Link1TargetGeneration = b.BodyGeneration; a.Link1Pc = target;
            new Span<FullNativeJumpEntry>(jumps, 256).Clear();
            FullNativeMemory.PublishJump(jumps, 255, generation, &b, in b);
            long issued = 0, completed = 0; int stop = failure == "stop" ? 1 : 0;
            var frame = FullFrame(machine, &a, &runtime, &issued, &completed, &stop, budget: 4, codeEpoch: &codeEpoch);
            frame.JumpCache = jumps; frame.JumpCacheMask = 255;
            frame.StoreGenerationPointer = &generation; frame.ObservedStoreGeneration = generation;
            if (failure == "target-code") --b.ValidatedGlobalEpoch;
            if (failure == "runtime") ++runtime;
            if (failure == "code") ++codeEpoch;
            if (failure == "null-code") frame.CodeAuthorityEpochPointer = null;
            if (failure == "abi3") frame.Version = 0x46420003;
            var exit = RunFull(cache ? indirect : direct, &frame);
            if (failure == "none") Check(exit == FullBlockExit.Budget && issued == 5 && completed == 4 && frame.TailJumps == 1 &&
                frame.Executed == 4 && ((ulong*)machine)[3] == 2, "fresh independent epochs chain with exact budget for direct/cache transfer");
            else if (failure == "target-code") Check(exit == FullBlockExit.Continue && issued == 1 && completed == 1 &&
                frame.Executed == 1 && frame.TailJumps == 0 && frame.Header == &a && ((ulong*)machine)[3] == 0,
                "code-stale target is rejected before direct/cache adoption");
            else Check(exit == (failure == "stop" ? FullBlockExit.Stop : FullBlockExit.Invalidated) && issued == 0 && completed == 0 &&
                frame.Executed == 0 && frame.TailJumps == 0, "source runtime/code/ABI/stop guard rejects before either transfer");
        }
    }

    static void CheckFullCodeEpochEncoding()
    {
        Check(FullBlockFrame.AbiVersion == 0x46420007 && sizeof(FullBlockFrame) == 416 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.CodeAuthorityEpochPointer)).ToInt32() == 368 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.ObservedCodeAuthorityEpoch)).ToInt32() == 376,
            "ABI4 uses only the two formerly unused words and preserves header/frame sizes");
        byte[] staging = new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach (bool windows in new[] { false, true })
        {
            int length = FullBlockEmitter.EmitScaffold(staging, out _, windows);
            Check(CountFullPattern(staging.AsSpan(0, length), [0x49, 0x8b, 0x82, 0x70, 1, 0, 0]) >= 1 &&
                CountFullPattern(staging.AsSpan(0, length), [0x49, 0x8b, 0x8a, 0x78, 1, 0, 0]) >= 1 &&
                CountFullPattern(staging.AsSpan(0, length), [0x49, 0x8b, 0x82, 0x80, 0, 0, 0]) >= 1,
                "both ABI scaffolds encode separate code pointer/observation and retained runtime pointer");
            for (int i = 0; i < 4; ++i) FullBlockEmitter.EmitScaffold(staging, out _, windows);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 20; ++i) FullBlockEmitter.EmitScaffold(staging, out _, windows);
            Check(GC.GetAllocatedBytesForCurrentThread() == before, "dual-epoch span emission is warm zero allocation on both ABIs");
        }
    }
}

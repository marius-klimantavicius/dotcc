using System.Buffers.Binary;
using Managed.Emulation.Execution;
using ControllerFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    static void CheckQemuBlockPolicy()
    {
        CheckQemuSharedCompilerBudget();
        CheckQemuLargeDecodedBlocks();
        CheckQemuNativeGateProgress();
        CheckQemuRecursiveCompilerScratch();
    }

    static void CheckQemuSharedCompilerBudget()
    {
        Check(FullCompilerScratch.ChargeFor(1) == FullCompilerScratch.ChargeFor(64),
            "serialized translation scratch is an owner resource independent of worker count");
        var io = new Managed.Emulation.Host.InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
        try
        {
        using var jit = FullTranslationJit.Create(io, new() { Mode = GuestJitMode.Full,
            CacheLimitBytes = 8L << 20, AllowUnverifiedWindows = true }, 64, 64UL << 20);
        var stats = jit.Snapshot(new());
        Check(stats.CacheBytes > 0 && stats.CacheBytes <= 8L << 20 && stats.NativeEntries == 0,
            "64-worker owner admits bounded512 compiler resources within unchanged8MiB quota");
        }
        finally { io.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
        using var scratch = new FullCompilerScratch(64);
        Span<int> slots = stackalloc int[FullCompilerScratch.SlotCount];
        for (int i = 0; i < slots.Length; ++i) slots[i] = scratch.Acquire();
        bool denied = false;
        try { scratch.Acquire(); } catch (InvalidOperationException) { denied = true; }
        Check(denied, "compiler recursive scratch exhaustion is explicit and bounded");
        for (int i = slots.Length - 1; i >= 0; --i) scratch.Release(slots[i]);
        int recovered = scratch.Acquire(); scratch.Release(recovered);
        Check(recovered == 0, "compiler scratch recovers after recursive scopes unwind");
    }

    static void CheckQemuLargeDecodedBlocks()
    {
        using var f = new ControllerFixture();
        ulong pc = ControllerFixture.Base;
        f.Map(pc, 8192);
        f.DataScope(0, () =>
        {
            byte* page = f.Program.LookupAddress2(f.Machine(), (long)pc, 0, 0);
            new Span<byte>(page, 4096).Fill(0x90);
        });
        Check(f.Acquire(pc, 0, out var large, out bool multiple, out int error) && error == 0 && multiple,
            "normal512 decoded translation");
        using (large)
        {
            var plans = FullTranslationJit.Plans(f.Jit.Store, in large);
            Check(plans.Length == 512 && plans[^1].Ip == pc + 511 && plans[511].Helper == null,
                "normal requested count512 with scalable native-site metadata");
            Check(f.Jit.Store.GetHeaderCopy(in large).GuestLength == 512,
                "normal512 guest length comes from exact decoded prefix");
        }
        Check(f.Statistics.BlockLengthCounts[512] == 1 && f.Statistics.BlockStopCounts["Count"] == 1,
            "512 length and count stopping reason exported");
        f.Counts(10);
        Check(f.Step(pc, signalActor: true) && f.Issued == 1 && f.Completed == 1 && f.Machine()->ip == pc + 1,
            "signal actor selects genuine one-instruction request");
        var actorKey = new TranslationKey(pc, FullTranslationJit.Context(f.Program, f.Machine(), 1));
        Check(f.Jit.Store.TryAcquireKey(actorKey, out var actor), "actor request has distinct retained key");
        using (actor) Check(FullTranslationJit.Plans(f.Jit.Store, in actor).Length == 1,
            "one-instruction special TB does not replace normal512 body");
        Check(f.Acquire(pc, 0, out var reused, out _, out _), "normal512 resident after actor");
        using (reused) Check(FullTranslationJit.Plans(f.Jit.Store, in reused).Length == 512,
            "ordinary lookup retains normal512 after single-step request");

        // Indirect JMP is decoded through its group extension, not only opcode.
        f.DataScope(0, () =>
        {
            byte* page = f.Program.LookupAddress2(f.Machine(), (long)(pc + 1024), 0, 0);
            page[0] = 0xff; page[1] = 0xe0; // jmp rax
        });
        Check(f.Acquire(pc + 1024, 0, out var indirect, out _, out _), "indirect branch TB");
        using (indirect) Check(FullTranslationJit.Plans(f.Jit.Store, in indirect).Length == 1,
            "indirect control flow stops translation before following bytes");
        f.DataScope(0, () =>
        {
            byte* page = f.Program.LookupAddress2(f.Machine(), (long)(pc + 2048), 0, 0);
            page[0] = 0xfb; // sti: inhibition/state boundary
        });
        Check(f.Acquire(pc + 2048, 0, out var state, out _, out _), "state boundary TB");
        using (state) Check(FullTranslationJit.Plans(f.Jit.Store, in state).Length == 1,
            "state boundary stops translation");
        Check(f.Acquire(pc + 4090, 0, out var tail, out _, out _), "page-tail TB");
        using (tail) Check(FullTranslationJit.Plans(f.Jit.Store, in tail).Length == 6 &&
            f.Jit.Store.GetHeaderCopy(in tail).GuestLength == 6,
            "same-page translation stops before next page");

        // A helper past the old uint bitmap ceiling must defer native links.
        f.DataScope(0, () =>
        {
            byte* page = f.Program.LookupAddress2(f.Machine(), (long)(pc + 512), 0, 0);
            new Span<byte>(page, 300).Fill(0x90);
            page[256] = 0x0f; page[257] = 0xa2; // CPUID helper; MOV memory now lowers natively.
            page[258] = 0xeb; page[259] = 0xfe;
        });
        Check(f.Acquire(pc + 512, 0, out var mixed, out _, out _), "mixed TB beyond bitmap limit");
        using (mixed)
        {
            var plans = FullTranslationJit.Plans(f.Jit.Store, in mixed);
            Check(plans.Length == 258 && plans[256].Helper != null && plans[255].Helper == null,
                "typed helper site256 retains selected helper without bitmap truncation");
            long lookups = f.Statistics.LinkTargetLookups;
            f.Jit.AuthorizeLinks(in mixed, true, 0, 0);
            Check(f.Statistics.LinkTargetLookups == lookups, "unreachable edge after helper256 stays deferred");
        }
        ulong wide = pc + 0x20000;
        f.Map(wide);
        f.DataScope(0, () =>
        {
            byte* page = f.Program.LookupAddress2(f.Machine(), (long)wide, 0, 0);
            new Span<byte>(page, 4096).Fill(0x90);
            for (int i = 0; i < 512; ++i)
            { page[i * 4] = 0x49; page[i * 4 + 1] = 0x83; page[i * 4 + 2] = 0xc7; page[i * 4 + 3] = 0xff; }
        });
        long retryBefore = f.Statistics.EmissionSizeRetries;
        Check(f.Acquire(wide, 0, out var shrunk, out _, out _), "actual compiler oversized scalar block");
        using (shrunk)
        {
            int count = FullTranslationJit.Plans(f.Jit.Store, in shrunk).Length;
            Check(count is > 1 and < 512 && f.Jit.Store.GetHeaderCopy(in shrunk).GuestLength == (uint)(count * 4) &&
                f.Statistics.EmissionSizeRetries > retryBefore && f.Statistics.BlockStopCounts["CodeLimit"] > 0,
                $"oversized512 scalar block halves prefix and publishes exact shorter guest range (count={count}, length={f.Jit.Store.GetHeaderCopy(in shrunk).GuestLength}, retries={f.Statistics.EmissionSizeRetries}, before={retryBefore}, native={f.Statistics.CompiledInlineSites}, helper={f.Statistics.CompiledHelperSites})");
        }
        Check(f.Statistics.InterpreterAttempts == 0, "large-block compiler never dispatches interpreter");
    }

    static void CheckQemuRecursiveCompilerScratch()
    {
        using var f = new ControllerFixture();
        ulong first = ControllerFixture.Base, second = first + 0x10000;
        f.Map(first); f.Map(second); f.Fill(first); f.Fill(second);
        int snapshots = 0;
        f.Jit.TestSnapshotTaken = pc =>
        {
            ++snapshots;
            Check(pc == first && Monitor.IsEntered(f.Jit.Authority.Gate), "recursive scratch begins under compiler gate");
            Check(f.Acquire(second, 1, out var nested, out _, out _), "recursive translation has an independent pooled slot");
            using (nested) Check(FullTranslationJit.Plans(f.Jit.Store, in nested)[0].Ip == second,
                "nested scratch payload has its own guest PC");
            f.At(first);
        };
        Check(f.Acquire(first, 0, out var outer, out _, out _), "outer translation survives nested compilation");
        using (outer) Check(FullTranslationJit.Plans(f.Jit.Store, in outer)[0].Ip == first &&
            FullTranslationJit.Plans(f.Jit.Store, in outer)[0].Decoded.op.uimm0 == 1,
            "nested compilation does not overwrite outer plans or emission scratch");
        Check(snapshots == 1 && f.Statistics.ActualBlockCompilations == 2 && f.Issued == 0 && f.Completed == 0,
            "bounded recursive compiler setup performs no guest execution");
    }

    static void CheckQemuNativeGateProgress()
    {
        using var f = new ControllerFixture();
        ulong pc = ControllerFixture.Base;
        f.Map(pc); f.Fill(pc);
        Check(f.Acquire(pc, 0, out var body, out bool multiple, out _) && multiple,
            "unbounded authority witness resident");
        body.Dispose();
        f.Counts(1_000_000_000);
        Exception? failure = null;
        bool gateEntered = false, writerDrained = false;
        using var writerDone = new ManualResetEventSlim();
        using var watchdog = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
        var writer = new Thread(() =>
        {
            try
            {
                if (!SpinWait.SpinUntil(() => f.Issued > 1024, TimeSpan.FromSeconds(5)))
                    throw new TimeoutException("Native chain did not pass1024 issues in one invocation.");
                lock (f.Jit.Authority.Gate)
                {
                    gateEntered = true;
                    f.Jit.RevokeMapping((long)pc, 4096);
                    writerDrained = true;
                }
            }
            catch (Exception error) { failure = error; f.Stop.RequestStop(); }
            finally { writerDone.Set(); }
        }) { IsBackground = true };
        writer.Start();
        try
        {
            Check(f.Step(pc), "unbounded native invocation returns after concurrent revocation");
            Check(writerDone.Wait(TimeSpan.FromSeconds(5)) && writer.Join(TimeSpan.FromSeconds(5)),
                "authority writer drains native outside gate");
            if (failure != null) throw new InvalidOperationException("native writer witness", failure);
            var stats = f.Statistics;
            Check(gateEntered && writerDrained && stats.NativeEntries == 1 && stats.InlineNativeInstructions > 1024 &&
                stats.NativeExitCounts[(int)FullBlockExit.Invalidated] == 1 && f.Stop.Reason == Managed.Emulation.Host.HostExecutionStopReason.None,
                "no routine32/512 return; writer acquires gate and epoch-polls actual native loop to completion");
            Check(f.Issued == f.Completed && stats.InterpreterAttempts == 0,
                "revocation after native progress preserves exact completed issuance");
        }
        finally
        {
            f.Stop.RequestStop();
            if (!writer.Join(TimeSpan.FromSeconds(5))) throw new TimeoutException("Native authority writer did not terminate.");
        }
    }
}

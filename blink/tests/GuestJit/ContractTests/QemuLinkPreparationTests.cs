using System.Buffers.Binary;
using Managed.Emulation.Execution;
using ControllerFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    static void CheckQemuLinkPreparation()
    {
        CheckQemuStaleGlobalLinkReuse();
        Check(new GuestJitOptions { Mode = GuestJitMode.Full }.CacheLimitBytes == 1L << 30 &&
            new GuestJitOptions { Mode = GuestJitMode.Hybrid }.CacheLimitBytes == 8L << 20 &&
            new GuestJitOptions { Mode = GuestJitMode.Full, CacheLimitBytes = 128L << 20 }.CacheLimitBytes == 128L << 20,
            "Full measured total-budget default and explicit retention limits remain distinct");
        using var f = new ControllerFixture();
        ulong source = ControllerFixture.Base, target = source + 0x10000;
        f.Map(source); f.Map(target); f.Fill(source); f.Fill(target);
        f.DataScope(0, () =>
        {
            byte* code = f.Program.LookupAddress2(f.Machine(), (long)source, 0, 0);
            code[0] = 0x0f; code[1] = 0xa2; // CPUID: typed runtime helper; MOV memory is now native.
            code[2] = 0xe9;
            BinaryPrimitives.WriteInt32LittleEndian(new(code + 3, 4), checked((int)(target - source - 7)));
        });
        nint sourcePhysical = f.Physical(source), targetPhysical = f.Physical(target);
        lock (f.Jit.Authority.Gate)
        {
            f.Jit.Authority.PrepareCodeSnapshot(sourcePhysical);
            f.Jit.Authority.PrepareCodeSnapshot(targetPhysical);
        }
        Check(f.Acquire(target, 0, out var destination, out bool targetMultiple, out _), "link target compilation");
        TranslationToken destinationToken = destination.Token;
        f.Jit.AuthorizeLinks(in destination, targetMultiple);
        destination.Dispose();
        Check(f.Acquire(source, 0, out var body, out bool multiple, out _) && multiple, "mixed link source compilation");
        using (body)
        {
            var h = f.Jit.Store.GetHeaderCopy(in body);
            Check(FullTranslationJit.Plans(f.Jit.Store, in body)[0].Helper != null && FullTranslationJit.Plans(f.Jit.Store, in body)[1].Helper == null, "retained per-site helper metadata comes from translated IR");
            var before = f.Statistics;
            f.Jit.AuthorizeLinks(in body, true, index: 0, nativeLimit: 32);
            var deferred = f.Statistics;
            Check(deferred.LinkTargetLookups == before.LinkTargetLookups &&
                deferred.SharedIndexLookups == before.SharedIndexLookups &&
                deferred.DeferredLinkPreparations == before.DeferredLinkPreparations + 1 &&
                f.Jit.Store.GetHeaderCopy(in body).Reserved136 == 1,
                "helper RET defers successor queries without losing validated flat provenance");
            f.Jit.AuthorizeLinks(in body, true, index: 1, nativeLimit: 1);
            Check(f.Statistics.LinkTargetLookups == before.LinkTargetLookups,
                "postcommit cap prevents unnecessary link preparation at the last permitted instruction");
            f.Jit.AuthorizeLinks(in body, true, index: 1, nativeLimit: 32);
            var linked = f.Jit.Store.GetHeaderCopy(in body);
            var prepared = f.Statistics;
            Check(prepared.LinkTargetLookups == before.LinkTargetLookups + 1 && linked.Link0TargetHeader == 0 &&
                linked.Link1TargetHeader != 0,
                "reachable unconditional exit prepares its sole actual successor");
            for (int i = 0; i < 100; ++i) f.Jit.AuthorizeLinks(in body, true, index: 1, nativeLimit: 32);
            var reused = f.Statistics;
            Check(reused.LinkTargetLookups == prepared.LinkTargetLookups &&
                reused.SharedIndexLookups == prepared.SharedIndexLookups && reused.LinkReuseHits == prepared.LinkReuseHits + 100,
                "stable authenticated data link reuses its publication without repeated shared-index queries");
            Check(f.Jit.Store.RetireContentChanged(destinationToken) &&
                f.Jit.Store.GetHeaderCopy(in body).Link1TargetHeader == 0,
                "target retirement reverse-unlinks the reused source cell before reclamation");
            f.Jit.AuthorizeLinks(in body, true, index: 1, nativeLimit: 32);
            Check(f.Statistics.LinkTargetLookups == reused.LinkTargetLookups + 1 &&
                f.Jit.Store.GetHeaderCopy(in body).Link1TargetHeader == 0,
                "unlinked target becomes a real shared-index miss again");
        }
        Check(f.Statistics.InterpreterAttempts == 0, "link preparation uses no interpreter dispatch");
    }

    static void CheckQemuStaleGlobalLinkReuse()
    {
        using var f = new ControllerFixture();
        ulong source = ControllerFixture.Base, target = source + 0x10000;
        f.Map(source); f.Map(target); f.Fill(source); f.Fill(target);
        f.DataScope(0, () =>
        {
            byte* code = f.Program.LookupAddress2(f.Machine(), (long)source, 0, 0);
            code[0] = 0x90; code[1] = 0xe9;
            BinaryPrimitives.WriteInt32LittleEndian(new(code + 2, 4), checked((int)(target - source - 6)));
        });
        // Isolate the later unrelated PTE mutation from first code-page
        // registration, which now invalidates data write-TLB authority.
        nint sourcePhysical = f.Physical(source), targetPhysical = f.Physical(target);
        lock (f.Jit.Authority.Gate)
        {
            f.Jit.Authority.PrepareCodeSnapshot(sourcePhysical);
            f.Jit.Authority.PrepareCodeSnapshot(targetPhysical);
        }
        Check(f.Acquire(target, 0, out var destination, out bool targetMultiple, out _), "global-link target compiled");
        TranslationToken targetToken = destination.Token;
        f.Jit.AuthorizeLinks(in destination, targetMultiple); destination.Dispose();
        Check(f.Acquire(source, 0, out var initial, out bool multiple, out _), "global-link source compiled");
        TranslationToken sourceToken = initial.Token;
        f.Jit.AuthorizeLinks(in initial, multiple);
        ulong oldGlobal = f.Jit.Store.GetHeaderCopy(in initial).ValidatedGlobalEpoch;
        initial.Dispose();
        long compiles = f.Statistics.ActualBlockCompilations, emitted = f.Statistics.EmittedCodeBytes;
        // Real unrelated PTE mutation changes invocation freshness without
        // revoking either body or its reverse-linked source edge.
        f.Map(target + 0x10000);
        Check(f.Jit.Authority.CodeEpoch > oldGlobal, "unrelated mapping advances global version");
        Check(f.Acquire(source, 0, out var body, out bool currentMultiple, out _),
            "source revalidates after unrelated mapping");
        using (body)
        {
            Check(f.Jit.Store.TryAcquireToken(targetToken, out var stale), "global-stale target remains resident");
            using (stale)
            {
                var h = f.Jit.Store.GetHeaderCopy(in stale);
                Check(h.Live == 1 && h.ValidatedGlobalEpoch == oldGlobal &&
                    f.Jit.Store.GetHeaderCopy(in body).Link1TargetHeader != 0,
                    "source edge remains armed while only target's global stamp is stale");
            }
            long lookups = f.Statistics.LinkTargetLookups, hits = f.Statistics.LinkReuseHits;
            f.Jit.AuthorizeLinks(in body, currentMultiple);
            Check(f.Statistics.LinkTargetLookups == lookups && f.Statistics.LinkReuseHits == hits + 1,
                "existing data edge is reused without certifying its target's stale global version");
            f.At(source, 1); FullRax(f, 0xabcdef, 1); f.Counts(32, issued: 1);
            var rejected = f.InvokeRetained(in body, f.Jit.Authority.CodeEpoch);
            Check(rejected.Exit == FullBlockExit.Continue && rejected.Executed == 2 && rejected.TailJumps == 0 &&
                rejected.Header->BodyGeneration == sourceToken.Generation && rejected.NextIp == target &&
                f.Issued == 2 && f.Completed == 2 && FullRax(f, 1) == 0xabcdef,
                "native link guard exits before adopting or executing the stale-global target");
            Check(f.Acquire(target, 0, out var refreshed, out _, out _), "actual checked target refresh");
            using (refreshed)
            {
                Check(refreshed.Token == targetToken && f.Jit.Store.GetHeaderCopy(in refreshed).ValidatedGlobalEpoch == f.Jit.Authority.CodeEpoch,
                    "target refresh retains original generation and emission");
                f.At(source, 1); FullRax(f, 0xabcdef, 1); f.Counts(4, issued: 1);
                var chained = f.InvokeRetained(in body, f.Jit.Authority.CodeEpoch);
                Check(chained.Exit == FullBlockExit.Budget && chained.Executed == 4 && chained.TailJumps == 1 &&
                    chained.Header->BodyGeneration == targetToken.Generation && FullRax(f, 1) == 1 &&
                    f.Issued == 5 && f.Completed == 4 && f.Statistics.ActualBlockCompilations == compiles &&
                    f.Statistics.EmittedCodeBytes == emitted,
                    "same armed edge chains after checked refresh with precise aggregate budget and no compilation");
            }
        }
    }
}

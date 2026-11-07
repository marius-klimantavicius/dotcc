using System.Buffers.Binary;
using Managed.Emulation.Execution;

namespace Managed.Emulation.Execution
{
    internal unsafe sealed partial class TranslationCodeRegions
    {
        internal bool TestFailSeal, TestFailMetadataRelease;
        internal int TestFailRegionRelease = -1;
        partial void ForceSealFailure(ref bool fail)
        { fail = TestFailSeal; TestFailSeal = false; }
        partial void ForceRegionReleaseFailure(int region, ref bool fail)
        { if (region == TestFailRegionRelease) { fail = true; TestFailRegionRelease = -1; } }
        partial void ForceMetadataReleaseFailure(ref bool fail)
        { fail = TestFailMetadataRelease; TestFailMetadataRelease = false; }
    }
    internal unsafe sealed partial class FullRepKernelStore
    {
        internal void TestFailNextSeal() => code.TestFailSeal = true;
        internal void TestFailNextRegionRelease(int region) => code.TestFailRegionRelease = region;
        internal void TestFailNextMetadataRelease() => code.TestFailMetadataRelease = true;
    }
}

unsafe partial class Program
{
    static void CheckRepKernelFailurePaths()
    {
        using (var io = new SemanticTestIo())
        using (var authority = new FullCodeAuthority(io.Value, 4096, 2, 4L << 20))
        using (var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 }))
        using (var pool = new FullRepKernelStore(authority, store, 2, true))
        {
            byte[] staging = new byte[FullBlockEmitter.RepKernelCapacity];
            var first = new FullRepInstruction(Ip, 2, FullRepOperation.Move, 8, 64, -1, FullRepRepeat.Count);
            var second = first with { Width = 16 };
            lock (authority.Gate)
            {
                Check(pool.TryBind(in first, staging, out var binding, out _), "R105 older kernel published before seal failure");
                var issued = pool.Acquire(in binding);
                try
                {
                    Check(pool.IsValid(in issued), "R105 older issued capability initially valid");
                    pool.TestFailNextSeal(); bool failed = false;
                    try { pool.TryBind(in second, staging, out _, out _); } catch (InvalidOperationException) { failed = true; }
                    Check(failed && !pool.AdmissionOpen && !pool.IsValid(in issued),
                        "R105 real quarantine protection invalidates every paused kernel capability before reentry");
                    CheckRepClosedAdmission(pool, first, binding, staging);
                }
                finally { issued.Dispose(); }
            }
        }
        foreach (bool metadataFailure in new[] { false, true })
        foreach (bool storeDisposedFirst in new[] { false, true })
        {
            using var io = new SemanticTestIo();
            using var authority = new FullCodeAuthority(io.Value, 4096, 2, 4L << 20);
            using var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 });
            using var pool = new FullRepKernelStore(authority, store, 2, true);
            byte[] staging = new byte[FullBlockEmitter.RepKernelCapacity];
            lock (authority.Gate)
            {
                var first = new FullRepInstruction(Ip, 2, FullRepOperation.Move, 8, 32, -1, FullRepRepeat.Count);
                FullRepKernelBinding binding = default;
                foreach (var op in Enum.GetValues<FullRepOperation>())
                foreach (int width in new[] { 8, 16, 32, 64 })
                foreach (int address in new[] { 32, 64 })
                foreach (int segment in new[] { -1, 4, 5 })
                foreach (var repeat in new[] { FullRepRepeat.Count, FullRepRepeat.WhileEqual, FullRepRepeat.WhileNotEqual })
                {
                    var i = new FullRepInstruction(Ip, 2, op, width, address, segment, repeat);
                    if (!FullRepLowering.IsCanonical(in i) || pool.ReservedBytes >= 131072) continue;
                    Check(pool.TryBind(in i, staging, out var found, out _), "R105 two-region partial-release prerequisite");
                    if (i == first) binding = found;
                }
                Check(pool.ReservedBytes == 131072 && store.ExternalReservedBytes == 131072 && binding.Cookie != 0,
                    "R105 exact two charged regions before failed disposal");
                if (storeDisposedFirst) store.Dispose();
                if (metadataFailure) pool.TestFailNextMetadataRelease();
                else pool.TestFailNextRegionRelease(1);
                bool failed = false;
                try { pool.Dispose(); } catch (InvalidOperationException) { failed = true; }
                long remaining = metadataFailure ? 0 : 65536;
                Check(failed && !pool.AdmissionOpen && pool.ReservedBytes == remaining && store.ExternalReservedBytes == remaining,
                    "R105 exceptional disposal refunds only actual successful unmaps, including after store disposal");
                Check(store.GetStatistics().ExternalReservedBytes == remaining &&
                    (!storeDisposedFirst || store.ChargedBytes == remaining && store.GetStatistics().ChargedBytes == remaining),
                    "R105 partially disposed owner still reports every actually retained external mapping byte");
                CheckRepClosedAdmission(pool, first, binding, staging);
                pool.Dispose();
                Check(store.ExternalReservedBytes == 0 && pool.ReservedBytes == 0 && !pool.AdmissionOpen,
                    "R105 retry reclaims every remaining region/metadata with zero stranded code quota");
            }
        }
    }

    static void CheckRepClosedAdmission(FullRepKernelStore pool, FullRepInstruction instruction,
        FullRepKernelBinding binding, byte[] staging)
    {
        bool find = false, acquire = false, bind = false;
        try { pool.Find(in instruction); } catch (InvalidOperationException) { find = true; }
        try { pool.Acquire(in binding); } catch (InvalidOperationException) { acquire = true; }
        try { pool.TryBind(in instruction, staging, out _, out _); } catch (InvalidOperationException) { bind = true; }
        Check(find && acquire && bind, "R105 closed pool refuses existing-key lookup, acquisition and warm binding");
    }

    static void CheckRepSharedYieldCleanup(byte* machine)
    {
        ResetFull(machine);
        using var common = CreateFullScaffold(out var scaffold);
        // Test-only host-ABI shim deliberately changes host MXCSR, then enters
        // the actual shared ninth typed exit. FloatHostProbe restores the true
        // host environment before CLR resumes, including on a failed assertion.
        var bytes = new List<byte> { 0xf3, 0x0f, 0x1e, 0xfa,
            0x49, 0x89, OperatingSystem.IsWindows() ? (byte)0xca : (byte)0xfa,
            0x4d, 0x8b, 0x5a, 8,
            0x41, 0x0f, 0xae, 0x9a, 0x60, 1, 0, 0,
            0x41, 0xc7, 0x82, 0x64, 1, 0, 0, 0x80, 0x1f, 0, 0,
            0x41, 0xc7, 0x82, 0x68, 1, 0, 0, 1, 0, 0, 0,
            0x41, 0x0f, 0xae, 0x92, 0x64, 1, 0, 0, 0x48, 0xb8 };
        byte[] target = new byte[8]; BinaryPrimitives.WriteInt64LittleEndian(target, scaffold.Exit(FullBlockExit.RepYield));
        bytes.AddRange(target); bytes.AddRange([0xff, 0xe0]);
        Check(NativeCodeAllocation.TryCreate(bytes.ToArray(), out var code, out string reason, requireNativeFlags: false), reason);
        using (code)
        {
            ulong epoch = 11; long issued = 1, completed = 0; int stop = 0;
            var header = FullHeader(Ip, code!.Entry);
            var frame = FullFrame(machine, &header, &epoch, &issued, &completed, &stop);
            Check(FloatRunProbe(code, &frame) == FullBlockExit.RepYield && issued == 1 && completed == 0 &&
                frame.HostMxcsrDirty == 0 && frame.HostMxcsrSaved == 0x5fa1,
                "R105 shared RepYield exit restores exact host FP environment and does not complete an instruction");
        }
        ResetFull(machine);
    }

    static void CheckRepLatchRestoreAfterFloat(byte* machine)
    {
        FloatSeed(machine);
        FloatWrite(machine, 0, 0, 32, 0x3f800000); FloatWrite(machine, 1, 0, 32, 0x33800000);
        var instruction = FloatClassify([0xf3, 0x0f, 0x58, 0xc1]);
        ulong next = Ip + (uint)instruction.Length;
        var rep = new FullRepInstruction(next, 2, FullRepOperation.Move, 8, 64, -1, FullRepRepeat.Count);
        byte[] staging = new byte[FullBlockEmitter.RepKernelCapacity];
        int length = FullBlockEmitter.EmitRepSite(rep, [0xf3, 0xa4], 73, 91, (nint)1, staging, in fullOwnerScaffold);
        Check(NativeCodeAllocation.TryCreate(staging.AsSpan(0, length), out var target, out string reason, requireNativeFlags: false), reason);
        using (target)
        using (var first = CreateFullBlock([new(instruction)]))
        {
            ulong epoch = 11; long issued = 0, completed = 0; int stop = 0;
            var source = FullHeader(Ip, first.Entry);
            var destination = FullHeader(next, target!.Entry, 18);
            source.Link0TargetHeader = (nint)(&destination);
            source.Link0TargetGeneration = destination.BodyGeneration; source.Link0Pc = next;
            var frame = FullFrame(machine, &source, &epoch, &issued, &completed, &stop);
            var state = new FullRepState { Version = FullRepState.CurrentVersion, Size = FullRepState.ByteSize,
                OwnerCookie = 73, WorkerCookie = 43 };
            frame.OwnerHandle = 43; frame.Rep = &state;
            Check(FloatRunProbe(first, &frame) == FullBlockExit.RepYield && frame.TailJumps == 1 &&
                issued == 2 && completed == 1 && frame.Executed == 1 && state.Phase == FullRepPhase.Active &&
                state.Reason == FullRepReason.Latched && state.CompletedIterations == 0 &&
                frame.HostMxcsrSaved == 0x5fa1 && frame.HostMxcsrDirty == 0,
                "R100 actual FP-to-REP native chain restores host environment at pre-data handoff without completing REP");
        }
        ResetFull(machine);
    }
}

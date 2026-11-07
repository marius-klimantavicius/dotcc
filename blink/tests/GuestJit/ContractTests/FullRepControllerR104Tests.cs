using Managed.Emulation.Execution;
using RepR104Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

namespace Managed.Emulation.Execution
{
    public sealed unsafe partial class ThreadedGuestExecution
    {
        // Contract assembly only: inject a bounded identity edge without an
        // alternative executor or changing authority-generation teardown.
        internal ulong RepAttemptForTests
        {
            get => current.Value!.FullRepAttempt;
            set => current.Value!.FullRepAttempt = value;
        }
    }
}

unsafe partial class Program
{
    // SOURCE-AUTHORED / UNRUN. Parent runner may call after CheckFullRepContracts.
    // These use the actual demand compiler/controller and existing fixture hooks.
    static void CheckFullRepControllerR104()
    {
        CheckRepR104DeniedInitialAdmission();
        CheckRepR104ExceptionalLatch();
        CheckRepR104AttemptExhaustion();
        CheckRepR104FlushContinuation();
    }

    static void CheckRepR104DeniedInitialAdmission()
    {
        using var f = new RepR104Fixture();
        const ulong pc = RepR104Fixture.Base, data = pc + 0x10000;
        f.Map(pc); f.Fill(pc); f.Map(data); f.Fill(data);
        WriteControllerCode(f, pc, [0xf3, 0xaa, 0xeb, 0xfe]);
        RepSeed(f, 0x800000000000UL, data + 128, 7, 1, f.Machine()->flags & ~0x10500u); f.Counts(1);
        int prepares = 0, returns = 0, latches = 0;
        Action<nint>? prepare = null;
        prepare = address =>
        {
            var frame = (FullBlockFrame*)address;
            Check(frame->Rep != null && frame->Rep->Phase == FullRepPhase.Idle && frame->Rep->AttemptGeneration == 0,
                "R104 denied initial enrollment cannot consume a REP latch/attempt");
            if (++prepares == 1)
            {
                Check(f.Jit.Authority.TryReserveNativeTicket(f.WorkerId(1)), "R104 foreign pending enrollment prerequisite");
                f.NativePrepared = prepare;
            }
            else
            {
                Check(returns == 0 && f.Completed == 0 && f.Issued == 1 && RepRegister(f, 32) == 1 &&
                    f.Machine()->ip == pc, "R104 no unenrolled native REP call or guest effects before retry");
                f.Jit.Authority.AbortNativeReservation(f.WorkerId(1));
            }
        };
        f.NativePrepared = prepare;
        f.NativeReturned = _ => ++returns;
        f.Owner.RepPausedForTests = (_, reason) => { if (reason == FullRepReason.Latched) ++latches; };
        using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        try
        {
            Check(f.Step(pc, signalActor: true) && prepares == 2 && latches == 1 && f.Issued == 1 && f.Completed == 1,
                "R104 initial admission retry retains one approved issue and one monotonic attempt");
            RepClean(f);
            lock (f.Jit.Authority.Gate) f.Jit.RepKernels.AssertQuiescent();
        }
        finally
        {
            f.NativePrepared = null; f.NativeReturned = null; f.Owner.RepPausedForTests = null;
            f.Jit.Authority.AbortNativeReservation(f.WorkerId(1));
        }
    }

    static void CheckRepR104ExceptionalLatch()
    {
        using var f = new RepR104Fixture();
        const ulong pc = RepR104Fixture.Base, data = pc + 0x10000;
        f.Map(pc); f.Fill(pc); f.Map(data); f.Fill(data);
        WriteControllerCode(f, pc, [0xf3, 0xaa, 0xeb, 0xfe]);
        RepSeed(f, 0x800000000000UL, data + 128, 7, 1, f.Machine()->flags & ~0x10500u); f.Counts(2);
        ulong first = 0, second = 0;
        var marker = new InvalidOperationException("R104 injected post-cleanup latch exception");
        f.Owner.RepPausedForTests = (address, reason) =>
        {
            if (reason != FullRepReason.Latched) return;
            ulong attempt = ((FullRepState*)address)->AttemptGeneration;
            if (first == 0) { first = attempt; throw marker; }
            second = attempt;
        };
        using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        try
        {
            bool caught = false;
            try { f.Step(pc, signalActor: true); }
            catch (InvalidOperationException error) when (ReferenceEquals(error, marker)) { caught = true; }
            Check(caught && first != 0 && f.Issued == 1 && f.Completed == 0 && RepRegister(f, 32) == 1,
                "R104 thrown pause abandons an issued/uncompleted latch without guest computation");
            RepClean(f);
            lock (f.Jit.Authority.Gate) f.Jit.RepKernels.AssertQuiescent();
            Check(f.Continue(signalActor: true) && second > first && f.Issued == 2 && f.Completed == 1,
                "R104 exceptional cleanup burns attempt identity before a fresh synchronous call");
            RepClean(f);
            lock (f.Jit.Authority.Gate) f.Jit.RepKernels.AssertQuiescent();
        }
        finally { f.Owner.RepPausedForTests = null; }
    }

    static void CheckRepR104FlushContinuation()
    {
        using var f = new RepR104Fixture();
        const ulong pc = RepR104Fixture.Base, src = pc + 0x10000, dst = pc + 0x20000;
        f.Map(pc); f.Fill(pc); f.Map(src, 8192); f.Fill(src, 8192); f.Map(dst); f.Fill(dst);
        WriteControllerCode(f, pc, [0xf3, 0xa4, 0xeb, 0xfe]);
        f.DataScope(0, () =>
        {
            f.Program.LookupAddress2(f.Machine(), (long)src + 4095, 0, 0)[0] = 0x11;
            byte* p = f.Program.LookupAddress2(f.Machine(), (long)src + 4096, 0, 0); p[0] = 0x22; p[1] = 0x33;
        });
        RepSeed(f, src + 4095, dst + 64, 0xee, 3, f.Machine()->flags & ~0x10500u); f.Counts(1);
        bool flushed = false;
        f.Owner.RepPausedForTests = (address, reason) =>
        {
            var state = (FullRepState*)address;
            if (flushed || reason != FullRepReason.NeedMemory || state->CompletedIterations != 1) return;
            var resources = f.Jit.Store.GetStatistics();
            Check(resources.ActiveManagedLeases == 0 && resources.ActiveNativeEpochs == 0 &&
                f.Statistics.ActiveDataBorrows == 0 && f.Jit.Authority.PendingAtomicWorker == -1,
                "R104 main store flush begins after complete old-capability cleanup");
            lock (f.Jit.Authority.Gate)
            {
                Check(f.Jit.Store.TryFlush(out _), "R104 actual main store flush during partial issued REP");
                bool active = false;
                try { f.Jit.RepKernels.AssertQuiescent(); } catch (InvalidOperationException) { active = true; }
                Check(active && f.Jit.RepKernels.Count != 0, "R104 issued independent kernel lease survives flush");
            }
            flushed = true;
            WriteControllerCode(f, pc, [0xf3, 0xaa, 0xeb, 0xfe]); // STOS must await a fresh issue/decode.
        };
        using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        try
        {
            Check(f.Step(pc, signalActor: true) && flushed && f.Issued == 1 && f.Completed == 1 &&
                RepRegister(f, 32) == 0 && RepRegister(f, 72) == src + 4098 && RepRegister(f, 80) == dst + 67,
                "R104 partial MOVS completes original issue after body flush and changed opcode");
            f.DataScope(0, () =>
            {
                byte* p = f.Program.LookupAddress2(f.Machine(), (long)dst + 64, 0, 0);
                Check(p[0] == 0x11 && p[1] == 0x22 && p[2] == 0x33, "R104 continuation uses retained MOVS semantics after flush");
            });
            RepClean(f);
            lock (f.Jit.Authority.Gate) f.Jit.RepKernels.AssertQuiescent();
        }
        finally { f.Owner.RepPausedForTests = null; }
    }

    static void CheckRepR104AttemptExhaustion()
    {
        using var f = new RepR104Fixture();
        const ulong pc = RepR104Fixture.Base, data = pc + 0x10000;
        f.Map(pc); f.Fill(pc); f.Map(data); f.Fill(data);
        WriteControllerCode(f, pc, [0xf3, 0xaa, 0xeb, 0xfe]);
        RepSeed(f, 0x800000000000UL, data + 128, 7, 1, f.Machine()->flags & ~0x10500u); f.Counts(2);
        f.Owner.RepAttemptForTests = ulong.MaxValue - 1;
        using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        Check(f.Step(pc, signalActor: true) && f.Owner.RepAttemptForTests == ulong.MaxValue && f.Completed == 1,
            "R104 final nonwrapping attempt generation may complete normally");
        RepClean(f);
        f.At(pc); Write64((byte*)f.Machine(), 32, 1);
        ulong instructions = f.Completed, destination = RepRegister(f, 80);
        int prepared = f.NativePreparedCalls;
        bool refused = false;
        try { f.Continue(signalActor: true); }
        catch (InvalidOperationException error) when (error.Message.Contains("attempt namespace exhausted")) { refused = true; }
        Check(refused && f.Owner.RepAttemptForTests == ulong.MaxValue && f.Completed == instructions &&
            RepRegister(f, 32) == 1 && RepRegister(f, 80) == destination && f.NativePreparedCalls == prepared,
            "R104 exhausted seed refuses before native admission without reuse, wrap, effects or completion");
        RepClean(f);
        lock (f.Jit.Authority.Gate) f.Jit.RepKernels.AssertQuiescent();
    }
}

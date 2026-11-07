using Managed.Emulation.Execution;
using OrdinaryRepFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

namespace Managed.Emulation.Execution
{
    public sealed unsafe partial class ThreadedGuestExecution
    {
        internal int RepStateInitializationsForTests { get; private set; }
        partial void FullControllerRepStateInitialized() => ++RepStateInitializationsForTests;
    }
}

unsafe partial class Program
{
    static void CheckFullRepOrdinaryState()
    {
        using var f = new OrdinaryRepFixture();
        const ulong pc = OrdinaryRepFixture.Base, data = pc + 0x10000;
        f.Map(pc); f.Fill(pc); f.Map(data);
        WriteControllerCode(f, pc, [0x48, 0x8b, 0x03, 0xeb, 0xfe]);
        Write64((byte*)f.Machine(), 48, data); f.Counts(1);
        int preparations = 0, returns = 0;
        Action<nint>? prepare = null;
        prepare = address =>
        {
            var frame = (FullBlockFrame*)address;
            Check(frame->Rep != null && frame->Rep->Version == FullRepState.CurrentVersion &&
                frame->Rep->Phase == FullRepPhase.Idle && frame->Rep->AttemptGeneration == (preparations == 0 ? 0UL : 7UL),
                "ordinary native frame reuses initialized Idle REP state and refreshes its attempt namespace");
            ++preparations; f.NativePrepared = prepare;
        };
        Action<nint>? returned = null;
        returned = address =>
        {
            var frame = (FullBlockFrame*)address; ++returns;
            if (returns == 1)
            {
                Check(frame->Exit == FullBlockExit.NeedMemory, "ordinary lazy load supplies a real mapping-return boundary");
                // Explicit protocol-level witness of a nested call consuming
                // attempt IDs; this does not claim an actual signal schedule.
                f.Owner.RepAttemptForTests = 7;
            }
            f.NativeReturned = returned;
        };
        f.NativePrepared = prepare; f.NativeReturned = returned;
        try
        {
            Check(f.Step(pc, signalActor: true) && f.Issued == 1 && f.Completed == 1 && preparations >= 2 &&
                returns >= 2 && f.Owner.RepStateInitializationsForTests == 1 && f.Owner.RepAttemptForTests == 7,
                "ordinary same-issue MMU retry initializes REP scope once without minting a REP attempt");
            RepClean(f);
            lock (f.Jit.Authority.Gate) f.Jit.RepKernels.AssertQuiescent();
        }
        finally { f.NativePrepared = null; f.NativeReturned = null; }
    }
}

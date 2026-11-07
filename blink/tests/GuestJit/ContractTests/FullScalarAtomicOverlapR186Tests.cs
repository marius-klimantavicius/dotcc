using Managed.Emulation.Execution;

unsafe partial class Program
{
    // Strengthen R186 V1: the concurrent interval MUST contain a real aligned
    // fast operation. An extra standalone operation cannot supply that witness.
    static void CheckR186LiveFastVsTicket()
    {
        using var f = new ThreadedGuestExecution.FullControllerFixture();
        using var deadline = f.AtomicDeadline();
        ulong pc = ThreadedGuestExecution.FullControllerFixture.Base, data = pc + 8192;
        AtomicFamilySetup(f, pc, data);
        WriteControllerCode(f, pc, [0xf0, 0x48, 0x01, 0x0b]);
        byte* host = (byte*)f.Physical(data) + 64;
        new Span<byte>(host, 16).Clear();
        AtomicFamilySeed(f, data + 64); Write64((byte*)f.Machine(), 32, 1);
        nint mainMachine = (nint)f.Machine();
        int foreignWorker = f.WorkerId(1), returned = 0, witnessed = 0;
        using var start = new ManualResetEventSlim();
        using var other = new AtomicFamilyThread(f, 1, () =>
        {
            AtomicFamilyWait(start, "R186 foreign ticket starts inside live fast enrollment");
            AtomicFamilySeed(f, data + 65, 1); Write64((byte*)f.Machine(1), 32, 1);
            Check(f.Step(pc, 1, true), "R186 unaligned overlapping ticket RMW completes");
            Volatile.Write(ref returned, 1);
        });
        f.NativeReturned = pointer =>
        {
            var frame = (FullBlockFrame*)pointer;
            if ((nint)frame->Machine != mainMachine || frame->Executed == 0 || frame->NativeMemoryComputations == 0) return;
            Check(frame->MemoryFlags == FullMemoryFlags.None && frame->MemoryTicket == 0,
                "R186 actual aligned fast route is required before concurrent reservation");
            Check(Interlocked.CompareExchange(ref witnessed, 1, 0) == 0,
                "R186 one fast native completion owns the live overlap interval");
            start.Set();
            // No Gate acquisition here: the foreign reservation closes
            // admission under Gate and drains this still-enrolled RET scope.
            AtomicFamilyWaitUntil(() => f.Jit.Authority.PendingAtomicWorker == foreignWorker,
                "R186 actual foreign reservation publishes while fast enrollment is live");
            Check(f.Jit.Authority.FullTestR185NativeFrames > 0 && Volatile.Read(ref returned) == 0 && *(ulong*)host == 1,
                "R186 ticket scatter cannot pass the fast operation's RET enrollment");
        };
        try
        {
            Check(f.Step(pc, signalActor: true), "R186 real aligned fast operation returns");
            other.Join();
            Check(witnessed == 1 && returned == 1 && *(ulong*)host == 257 && f.Issued == 2 && f.Completed == 2,
                "R186 forced fast/ticket overlap commits both actual RMWs exactly once");
            AtomicFamilyAssertClean(f);
        }
        finally { start.Set(); f.NativeReturned = null; }
    }
}

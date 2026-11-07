using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    // R86-01 actual controller witness. A foreign reservation is established
    // before the refill frame observes its runtime epoch. No foreign native
    // store is required: executing even one memory effect without enrollment
    // already violates the exclusion contract. All buffers belong to this test.
    static void CheckFullStackMemoryAdmission()
    {
        using var f = new ThreadedGuestExecution.FullControllerFixture();
        const ulong code = ThreadedGuestExecution.FullControllerFixture.Base;
        const ulong source = code + 0x10000, destination = code + 0x20000;
        f.Map(code); f.Fill(code); f.Map(source); f.Fill(source); f.Map(destination); f.Fill(destination);
        WriteControllerCode(f, code, [0xff, 0x33, 0xeb, 0xfe]); // PUSH qword [RBX]
        var m = f.Machine();
        Write64((byte*)m, 48, source + 128); Write64((byte*)m, 56, destination + 136);
        f.DataScope(0, () =>
        {
            *(ulong*)f.Program.LookupAddress2(m, (long)source + 128, 0, 0) = 0x1122334455667788;
            *(ulong*)f.Program.LookupAddress2(m, (long)destination + 128, 0, 0) = 0xaabbccddeeff0011;
        });
        byte* dest = (byte*)f.Physical(destination) + 128;
        bool reserved = false, readyAtPrepare = false, blockedReadyObserved = false;
        int preissues = 0;
        f.Preissued = (_, _) => ++preissues;
        Action<nint>? prepare = null;
        prepare = pointer =>
        {
            var frame = (FullBlockFrame*)pointer;
            frame->NativeLimit = 1;
            if (!reserved)
            {
                lock (f.Jit.Authority.Gate)
                    Check(f.Jit.Authority.TryReserveNativeTicket(1), "R86 foreign reservation precedes a fresh refill frame");
                reserved = true;
            }
            readyAtPrepare = frame->StackMemory != null && frame->StackMemory->State == FullStackMemoryState.Ready;
            f.NativePrepared = prepare;
        };
        f.NativePrepared = prepare;
        f.NativeReturned = pointer =>
        {
            if (!readyAtPrepare || blockedReadyObserved) return;
            var frame = (FullBlockFrame*)pointer;
            Check(f.Jit.Authority.PendingAtomicWorker == 1 && frame->Exit == FullBlockExit.NeedMemory &&
                frame->Executed == 0 && f.Completed == 0 && m->ip == code && m->oplen == 0 &&
                *(ulong*)((byte*)m + 56) == destination + 136 && *dest == 0x11 &&
                *(ulong*)dest == 0xaabbccddeeff0011,
                "R86 denied native enrollment discards Ready direct pointers before ANY memory or CPU effect");
            blockedReadyObserved = true;
            f.Jit.Authority.AbortNativeReservation(1);
        };
        f.Counts(1);
        using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        try
        {
            Check(f.Step(code) && blockedReadyObserved && f.Completed == 1 && f.Issued == 1 && preissues == 1 &&
                m->ip == code + 2 && *(ulong*)((byte*)m + 56) == destination + 128 && *(ulong*)dest == 0x1122334455667788,
                "R86 same approved issue recovers and commits once after foreign reservation ends");
            var s = f.Jit.Store.GetStatistics();
            Check(s.ActiveManagedLeases == 0 && s.ActiveNativeEpochs == 0 && f.Statistics.ActiveDataBorrows == 0 &&
                f.Jit.Authority.PendingAtomicWorker == -1,
                "R86 controller drains every request/store/native/data capability");
        }
        finally
        {
            f.NativePrepared = null; f.NativeReturned = null;
            f.Jit.Authority.AbortNativeReservation(1);
        }
    }
}

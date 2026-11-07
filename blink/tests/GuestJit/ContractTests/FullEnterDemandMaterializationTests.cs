using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution
{
    // Only compiled into ContractTests. Both callbacks disappear from product
    // builds, including argument evaluation; no native instrumentation occurs.
    public sealed unsafe partial class ThreadedGuestExecution
    {
        private Action<nint, bool, bool>? testFullMaterialization;
        internal void TestObserveMaterialization(Action<nint, bool, bool>? callback)
            => testFullMaterialization = callback;
        partial void FullControllerMemoryMaterializing(FullBlockFrame* frame, bool finalCheckOnly)
            => testFullMaterialization?.Invoke((nint)frame, finalCheckOnly, false);
        partial void FullControllerMemoryMaterialized(FullBlockFrame* frame, bool finalCheckOnly)
            => testFullMaterialization?.Invoke((nint)frame, finalCheckOnly, true);
    }
}

unsafe partial class Program
{
    static void CheckFullEnterDemandMaterialization()
    {
        using var f = new ThreadedGuestExecution.FullControllerFixture();
        const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
        const ulong push = pc + 0x2fff, final = pc + 0xfff;
        f.Map(pc, 4 * 4096);
        f.Fill(pc); f.Fill(pc + 0x2000); f.Fill(pc + 0x3000);
        WriteControllerCode(f, pc, [0x90, 0xc8, 0x00, 0x20, 0x00, 0xeb, 0xfe]);
        f.DataScope(0, () =>
        {
            byte* at = f.Program.LookupAddress2(f.Machine(), (long)final, 0, 0);
            Check(at != null, "R130 final first page materialized");
            at[0] = 0x5a;
        });
        nint push0 = f.Physical(pc + 0x2000), push1 = f.Physical(pc + 0x3000);
        lock (f.Jit.Authority.Gate)
        {
            // Register actual destinations before the witness. This removes
            // first-write enrollment pulses from the materialization interval.
            f.Jit.Authority.TryGetNativeDataPermission(push0, true, out _);
            f.Jit.Authority.TryGetNativeDataPermission(push1, true, out _);
            Check(f.Jit.Authority.TryGetNativeDataPermission(push0, true, out _) &&
                f.Jit.Authority.TryGetNativeDataPermission(push1, true, out _),
                "R130 actual push mappings have stable native write permission");
        }
        Check(f.Acquire(pc, 0, out var lease, out _, out _), "R130 compile code alias of final-only page");
        using (lease)
            Check(f.Jit.Store.GetHeaderCopy(in lease).Live == 1, "R130 final first physical page is watched code");
        const ulong oldBp = 0x8877665544332211;
        Write64((byte*)f.Machine(), 56, push + 8); Write64((byte*)f.Machine(), 64, oldBp);
        int beforeBorrows = -1, callbacks = 0;
        long beforeRevocations = -1;
        ulong beforeRuntime = 0, beforeCode = 0;
        f.Owner.TestObserveMaterialization((pointer, finalOnly, after) =>
        {
            var frame = (FullBlockFrame*)pointer;
            Check(finalOnly && frame->MemoryAddress == final && frame->MemoryWidth == 64 &&
                frame->MemoryAccess == FullMemoryAccess.Write && FullNativeMemory.IsEnterRequest(frame),
                "R130 exact final-only crossing request reaches demand materialization");
            if (!after)
            {
                beforeBorrows = f.Jit.Authority.ActiveBorrows;
                beforeRuntime = f.Jit.Authority.Epoch; beforeCode = f.Jit.Authority.CodeEpoch;
                beforeRevocations = f.Jit.Store.GetStatistics().AuthorityRevocations;
            }
            else
            {
                ++callbacks;
                Check(f.Jit.Authority.ActiveBorrows == beforeBorrows,
                    "R130 mapping-only final commitment enrolls no physical writer");
                Check(f.Jit.Store.GetStatistics().AuthorityRevocations == beforeRevocations &&
                    f.Jit.Authority.CodeEpoch == beforeCode && f.Jit.Authority.Epoch == beforeRuntime,
                    "R130 materializing reserved final neighbor does not pulse writer epochs or revoke watched alias");
            }
        });
        Action<nint> cap = pointer => ((FullBlockFrame*)pointer)->NativeLimit = 1;
        f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap;
        try
        {
            f.Counts(2); Check(f.Step(pc), "R130 initial controller issue");
            for (int retry = 0; f.Completed < 2 && retry < 12; ++retry)
                Check(f.Continue(), "R130 bounded same-issue continuation");
            Check(callbacks == 1 && f.Issued == 2 && f.Completed == 2 &&
                f.Statistics.SemanticHelperInstructions == 0 && f.Statistics.InterpreterAttempts == 0,
                "R130 lazy final-only page completes one native ENTER without reissuing or semantic helper");
            Check(((byte*)f.Physical(pc))[4095] == 0x5a, "R130 watched final byte unchanged");
            byte* neighbor = (byte*)f.Physical(pc + 4096);
            for (int b = 0; b < 7; ++b) Check(neighbor[b] == 0, "R130 newly committed final bytes remain zero");
            byte* pushed = (byte*)f.Physical(pc + 0x2000) + 4095;
            Check(pushed[0] == unchecked((byte)oldBp), "R130 actual crossing push first byte committed");
            byte* pushedNext = (byte*)f.Physical(pc + 0x3000);
            for (int b = 1; b < 8; ++b) Check(pushedNext[b - 1] == (byte)(oldBp >> (8 * b)),
                "R130 actual crossing push remaining bytes committed");
            Check(((ulong*)((byte*)f.Machine() + 56))[0] == final && ((ulong*)((byte*)f.Machine() + 64))[0] == push,
                "R130 BP and full SP commit after final-only materialization");
            var stats = f.Jit.Store.GetStatistics();
            Check(stats.ActiveManagedLeases == 0 && stats.ActiveNativeEpochs == 0 &&
                f.Statistics.ActiveDataBorrows == 0 && f.Jit.Authority.ActiveBorrows == 0,
                "R130 demand materialization releases every checked mapping and capability");
        }
        finally { f.Owner.TestObserveMaterialization(null); f.NativePrepared = null; f.NativeReturned = null; }
    }
}

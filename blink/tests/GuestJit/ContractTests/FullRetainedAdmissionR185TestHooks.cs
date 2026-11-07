using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullCodeAuthority
{
    internal int FullTestR185NativeFrames => Volatile.Read(ref nativeFrames);
}

public sealed unsafe partial class ThreadedGuestExecution
{
    internal sealed partial class FullControllerFixture
    {
        internal readonly record struct R185Result(bool Acquired, TranslationToken Token,
            uint Index, bool Multiple, bool Retained, bool FetchBorrow, int Error, ulong Immediate);

        // Same acquisition API as the actual patched controller. Values only
        // escape; every view/lease/read is released before returning to tests.
        internal R185Result R185Acquire(ulong pc, TranslationToken resume = default,
            uint resumeIndex = 0, bool forceChecked = false, uint requestedCount = 0)
        {
            At(pc); var worker = testWorkers[0];
            bool previous = worker.FullFetchActive, borrowed = false;
            TranslationLease lease = default;
            try
            {
                worker.FullFetchActive = true;
                lock (Jit.Authority.Gate)
                {
                    Jit.Authority.DrainNativeForFiniteGateRead();
                    bool acquired = Jit.AcquireNativeEntry(Program, worker.Machine, ref worker.FullCache,
                        resume, resumeIndex, forceChecked, ref borrowed, out lease, out var view,
                        out uint index, out bool multiple, out bool retained, out int error, requestedCount);
                    ulong immediate = acquired ? ((FullTranslationJit.Plan*)view.Header->Payload)[index].Decoded.op.uimm0 : 0;
                    return new(acquired, lease.Token, index, multiple, retained, borrowed, error, immediate);
                }
            }
            finally
            {
                try { lease.Dispose(); }
                finally { worker.FullFetchActive = previous; if (borrowed) Jit.EndRead(Program); }
            }
        }

        internal void R185Resume(TranslationToken token, uint index)
        { testWorkers[0].FullResume = token; testWorkers[0].FullResumeIndex = index; }
    }
}

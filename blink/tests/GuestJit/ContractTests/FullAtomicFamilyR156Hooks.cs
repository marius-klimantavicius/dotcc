using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Test-only, erased in production. Real bindings are used on the foreign host
// thread; no Machine, TLS context, worker cache or x87 owner is shared concurrently.
public sealed unsafe partial class ThreadedGuestExecution
{
    private Action<nint>? atomicCopyReadStarted, atomicCopyReadEnded;
    private Action<nint, nint, int>? atomicCopyWriteStarting;
    partial void FullAtomicCopyReadStarted(nint machine) => atomicCopyReadStarted?.Invoke(machine);
    partial void FullAtomicCopyReadEnded(nint machine) => atomicCopyReadEnded?.Invoke(machine);
    partial void FullAtomicCopyWriteStarting(nint machine, nint address, int count) => atomicCopyWriteStarting?.Invoke(machine, address, count);

    internal sealed partial class FullControllerFixture
    {
        private int atomicBoundFrames;
        internal IDisposable AtomicDeadline() => new Timer(_=>Stop.RequestStop(),null,TimeSpan.FromSeconds(12),Timeout.InfiniteTimeSpan);
        internal Action<nint>? AtomicCopyReadStarted { set => Owner.atomicCopyReadStarted = value; }
        internal Action<nint>? AtomicCopyReadEnded { set => Owner.atomicCopyReadEnded = value; }
        internal Action<nint, nint, int>? AtomicCopyWriteStarting { set => Owner.atomicCopyWriteStarting = value; }
        internal void AtomicMapSharedPage(ulong address,int fd,long fileOffset)
        {
            Select(0);using var data=new FullDataScope(Owner,testWorkers[0]);
            Require(Program.ReserveVirtual(Owner.system,(long)address,4096,Program.SetProtection(7),fd,fileOffset,true,true)==(long)address,
                "R156 exact shared file-page offset mapping");
        }
        internal void AtomicPrepareBoundWorker(int index)
        {
            if (index <= 0 || index >= testWorkers.Length) throw new ArgumentOutOfRangeException(nameof(index));
            var w=testWorkers[index];
            DetachX87Worker(w); // original fixture thread owns this binding
            w.FullCache.Dispose(); w.Bound=false;
        }
        // Prepare on the fixture thread, run once, join before disposal.
        internal void AtomicBoundWorker(int index, Action body)
        {
            if (index <= 0 || index >= testWorkers.Length) throw new ArgumentOutOfRangeException(nameof(index));
            var w = testWorkers[index];
            Interlocked.Increment(ref atomicBoundFrames);
            if (w.Bound || !w.X87Detached) throw new InvalidOperationException("Prepare fixture worker before host-thread transfer.");
            try
            {
                Owner.Bind(w);
                w.Machine->thread = w.HostIdentity;
                Program.SetHostGuestCurrentMachine(w.Machine);
                body();
            }
            finally
            {
                try { DetachX87Worker(w); Owner.Unbind(w); }
                finally
                {
                    // Fixture teardown runs on its original thread and uses
                    // the same synthetic clone bookkeeping as its constructor.
                    Require(Jit.Store.TryCreateWorkerCache(out w.FullCache), "restore quiescent fixture worker slot");
                    w.Bound = true;
                    Interlocked.Decrement(ref atomicBoundFrames);
                }
            }
        }
        internal void AtomicSyscall(ulong code, int worker, ulong number, ulong a0=0, ulong a1=0, ulong a2=0, ulong a3=0, ulong a4=0, ulong a5=0)
        {
            var m = Machine(worker); ulong* r = (ulong*)((byte*)m+24);
            r[0]=number; r[7]=a0; r[6]=a1; r[2]=a2; r[10]=a3; r[8]=a4; r[9]=a5;
            if (!Step(code, worker, signalActor:true)) throw new InvalidOperationException("Atomic-family syscall refused.");
        }
        internal void AtomicCheckSignals(int worker)
        { Select(worker); var m=Machine(worker);m->signals=1UL<<16;m->sigmask=0;m->attention=true;Owner.CheckSignals(testWorkers[worker]); }
        internal void AtomicResumeActor(ulong code)
        {
            At(code); Machine()->restored = false; Machine()->attention = false;
            // One real atomic commits. The existing returned hook requests a
            // restored signal boundary before Actor can issue another opcode.
            NativeReturned = pointer =>
            {
                var f=(FullBlockFrame*)pointer;
                if (f->NativeMemoryComputations == 0 || f->Executed == 0) return;
                Machine()->restored=true; Machine()->attention=true;
            };
            try { ((IHostGuestThreads)Owner).RunSignalActor((nint)Machine()); }
            finally { NativeReturned=null; }
        }
        internal void AtomicAssertSnapshot(ulong code, Action mutate)
        {
            At(code); var w=testWorkers[0];
            if (!Acquire(code,0,out var lease,out _,out int error,requestedCount:1) || error!=0)
                throw new InvalidOperationException("Atomic snapshot test acquire.");
            using (lease)
            {
                var plan=FullTranslationJit.Plans(Jit.Store,in lease)[0];
                if (!NativeAtomic(in plan,out var i)) throw new InvalidOperationException("Atomic classifier inactive in qualification candidate.");
                var frame=new FullBlockFrame { Machine=w.Machine, MemoryAccess=FullNativeMemory.AtomicReadWrite,
                    MemoryWidth=(uint)i.Width,MemoryAddress=FullNativeMemory.AtomicEffectiveAddress(in i,w.Machine),
                    MemoryResume=1,FsBase=w.Machine->fs.@base,GsBase=w.Machine->gs.@base };
                var snapshot=CaptureAtomicRequest(in plan,&frame,w.Machine);
                mutate(); bool rejected=false;
                try { AuthenticateAtomicSnapshot(in plan,&frame,w.Machine,in snapshot); }
                catch(InvalidOperationException) { rejected=true; }
                Require(rejected,"atomic snapshot mutation must fail before effects");
            }
        }
    }
}

internal sealed unsafe partial class FullCodeAuthority
{
    internal Action? TestAtomicHostPending;
    partial void AtomicHostReaderPending() => TestAtomicHostPending?.Invoke();
}

using System.Buffers.Binary;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Compiled ONLY into ContractTests, alongside the authored threaded sources.
// No reflection, public product API, alternate dispatch, or historical engine.
public sealed unsafe partial class ThreadedGuestExecution
{
    private Action<nint, bool>? testFullPreissued;
    private Action<long, long>? testFullMappingRevoked;
    private Action<nint>? testFullNativePrepared;
    private int testFullNativePreparedCalls;
    private Action<nint>? testFullNativeReturned;
    partial void FullControllerNativeReturned(FullBlockFrame* frame)
        => testFullNativeReturned?.Invoke((nint)frame);

    partial void FullControllerPreissued(Blink.Machine* machine, bool signalActor)
        => testFullPreissued?.Invoke((nint)machine, signalActor);

    partial void FullControllerMappingRevoked(long address, long size)
    {
        var callback = testFullMappingRevoked;
        testFullMappingRevoked = null; // one callback, including recursive maps
        callback?.Invoke(address, size);
    }

    partial void FullControllerNativePrepared(FullBlockFrame* frame)
    {
        Interlocked.Increment(ref testFullNativePreparedCalls);
        var callback = testFullNativePrepared;
        testFullNativePrepared = null;
        callback?.Invoke((nint)frame);
    }

    internal sealed partial class FullControllerFixture : IDisposable
    {
        internal const ulong Base = 0x400000;
        internal const ulong MemoryLimit = 8UL << 20;
        internal const long Budget = 3L << 20;
        internal readonly InstanceIo Io;
        internal readonly HostExecutionStop Stop;
        internal readonly ThreadedGuestExecution Owner;
        private Blink.Libc.RuntimeBinding binding;
        private readonly Worker[] testWorkers;
        private bool mutationBound, signalsBound, exitBound, mainBound, disposed;
        internal FullTranslationJit Jit => Owner.fullJit!;
        internal Blink Program => Owner.program!;
        internal GuestJitStatistics Statistics => Owner.JitStatistics;
        internal ulong Issued => Owner.InstructionsIssued;
        internal ulong Completed => Owner.InstructionsCompleted;
        internal int BodyCapacity => Jit.Store.GetStatistics().BodyCapacity;
        internal Action<nint, bool>? Preissued { set => Owner.testFullPreissued = value; }
        internal Action<long, long>? MappingRevoked { set => Owner.testFullMappingRevoked = value; }
        internal Action<nint>? NativePrepared { set => Owner.testFullNativePrepared = value; }
        internal int NativePreparedCalls => Volatile.Read(ref Owner.testFullNativePreparedCalls);
        internal Action<nint>? NativeReturned { set => Owner.testFullNativeReturned = value; }

        internal FullControllerFixture(bool helpersOnly = false, int workers = 2, long retentionBudget = Budget, bool interpreterOracle = false)
        {
            Io = new(new Dictionary<string, ReadOnlyMemory<byte>>());
            Stop = new();
            Owner = new(Io, Stop, MemoryLimit, workers, jitOptions: new()
            {
                Mode = interpreterOracle ? GuestJitMode.Interpreter : GuestJitMode.Full, CacheLimitBytes = retentionBudget,
                HostFeatures = helpersOnly ? GuestJitHostFeatures.BaselineOnly : GuestJitHostFeatures.Detected,
                AllowUnverifiedWindows = true
            });
            testWorkers = new Worker[workers];
            try
            {
                Owner.program = new Blink(); binding = Program.__DotCcEnter();
                Owner.instructionBudget = 1000000;
                Owner.variables = new(); Owner.directories = new(Io); Owner.barrier = new();
                Owner.fullJit = FullTranslationJit.Create(Io, Owner.jitOptions, workers, MemoryLimit);
                if (helpersOnly) Jit.FullTestDisableNative();
                Jit.CheckProducer(Program);
#if BLINK_FULL_X87_STATE_OWNER
                CheckX87StateProducer(Program);
#endif
                Io.BindMemoryFileMutation(Jit); mutationBound = true;
                Io.BindMemoryFileRead(Owner);
                Owner.memoryToken = Program.BlinkHostMemoryCreateShared(MemoryLimit);
                Require(Owner.memoryToken != 0, "controller fixture memory owner");
                var main = testWorkers[0] = Owner.NewWorker(true, null);
                Owner.workers.Add(main); Owner.Bind(main); mainBound = true;
                Require(Program.BlinkHostSignalActionsBegin() == 0, "controller signal actions"); signalsBound = true;
                Require(Program.BlinkHostExitCallbacksBegin() == 0, "controller exit callbacks"); exitBound = true;
                Require(Program.BlinkHostMemoryEnablePrivateFiles() == 0, "controller private files");
                Program.InitMap(); Program.InitBus();
                Owner.system = Program.NewSystem(new() { omode = 2, genmode = 1 });
                Require(Owner.system != null, "controller real system");
                Require(Program.BlinkHostInitializeBoundResourceLimits(Owner.system) == 0, "controller resource limits");
                Require(main.FullNativeState != null && main.X87Binding == null && !main.X87Initialized,
                    "controller main storage precedes NewMachine without a binding");
                main.Machine = Program.NewMachine(Owner.system, null);
                Require(main.Machine != null, "controller real main machine");
                Owner.InitializeX87Worker(main);
                main.GuestId = main.Machine->tid; main.Machine->thread = main.HostIdentity;
                Owner.system->trapexit = true;
                Select(0);
                using (new FullDataScope(Owner, main))
                {
                    // NewSystem leaves cr3 zero. LoadProgram normally creates
                    // the protected-mode root before its first ReserveVirtual;
                    // this fixture bypasses that loader, so do its CPU/root
                    // initialization explicitly before mapping or cloning.
                    main.Machine->flags = Program.SetFlag(main.Machine->flags, 9, true);
                    Owner.system->cr0 = 0x80000013u;
                    ulong root = Program.AllocatePageTable(Owner.system);
                    Require(root != ulong.MaxValue && (root & (ulong)Blink.PAGE_V) != 0,
                        "controller root page-table allocation");
                    Owner.system->cr3 = root;
                    byte* table = Program.GetPageAddress(Owner.system, root, true);
                    Require(table != null && Program.BlinkHostMemoryContains(table, 4096) != 0,
                        "controller root page table belongs to physical owner");
                }
                for (int i = 1; i < workers; ++i)
                {
                    var worker = testWorkers[i] = Owner.NewWorker(false, Program.NewMachine(Owner.system, main.Machine));
                    Require(worker.Machine != null && Jit.Store.TryCreateWorkerCache(out worker.FullCache), "controller second worker");
                    // Real NewMachine already copied metadata. Reuse the
                    // production fresh-identity clone before synthetic binding.
                    Owner.CloneX87Worker(main, worker);
                    worker.ThreadId = Environment.CurrentManagedThreadId;
                    worker.Bound = true;
                    Owner.BindX87Worker(worker);
                    worker.GuestId = worker.Machine->tid; worker.HostIdentity = main.HostIdentity;
                    Owner.workers.Add(worker);
                }
                Select(0);
            }
            catch { Dispose(); throw; }
        }

        internal Blink.Machine* Machine(int worker = 0) => testWorkers[worker].Machine;
        internal int WorkerId(int worker) => testWorkers[worker].FullCache.Worker;
        private void Select(int worker)
        {
            Owner.current.Value = testWorkers[worker];
            Program.SetHostGuestCurrentMachine(testWorkers[worker].Machine);
        }
        internal void At(ulong pc, int worker = 0)
        {
            Select(worker); var w = testWorkers[worker];
            w.Machine->ip = pc; w.Machine->oplen = 0;
            w.FullResume = default; w.FullResumeIndex = 0;
            Program.ResetTlb(w.Machine); Program.ResetInstructionCache(w.Machine);
        }
        internal void Counts(ulong budget, long issued = 0, long completed = 0)
        { Owner.instructionBudget = budget; Owner.issued = issued; Owner.completed = completed; }
        internal void Map(ulong address, int bytes = 4096)
        {
            Select(0); using var data = new FullDataScope(Owner, testWorkers[0]);
            Require(Program.ReserveVirtual(Owner.system, (long)address, bytes, Program.SetProtection(7), -1, 0, false, true) == (long)address,
                "controller anonymous mapping");
        }
        internal void MapShared(int fd, ulong address)
        {
            Select(0); using var data = new FullDataScope(Owner, testWorkers[0]);
            Require(Program.ReserveVirtual(Owner.system, (long)address, 4096, Program.SetProtection(7), fd, 0, true, true) == (long)address,
                "controller shared alias mapping");
        }
        internal void Fill(ulong address, int bytes = 4096)
        {
            Select(0); using var data = new FullDataScope(Owner, testWorkers[0]);
            for (int offset = 0; offset < bytes; offset += 4096)
            {
                byte* page = Program.LookupAddress2(Machine(), (long)(address + (uint)offset), 0, 0);
                Require(page != null, "controller committed backing");
                new Span<byte>(page, 4096).Fill(0xcc);
                for (int at = 0; at <= 4096 - 16; at += 16)
                {
                    // mov eax, immediate; jmp back to mov. Every candidate has
                    // a real boundary and fits one retained native code page.
                    page[at] = 0xb8;
                    BinaryPrimitives.WriteUInt32LittleEndian(new(page + at + 1, 4), (uint)(offset + at + 1));
                    page[at + 5] = 0xeb; page[at + 6] = 0xf9;
                }
            }
        }
        internal void WriteMov(ulong address, uint value)
        {
            Select(0); using var data = new FullDataScope(Owner, testWorkers[0]);
            byte* code = Program.LookupAddress2(Machine(), (long)address, 0, 0);
            Require(code != null, "controller replacement backing");
            code[0] = 0xb8; BinaryPrimitives.WriteUInt32LittleEndian(new(code + 1, 4), value);
            code[5] = 0xeb; code[6] = 0xf9;
        }
        internal nint Physical(ulong address, int worker = 0)
        {
            Select(worker); Jit.BeginRead(Program, false);
            bool previous = testWorkers[worker].FullFetchActive;
            testWorkers[worker].FullFetchActive = true;
            try
            {
                Require(JitPageIdentity.TryRead(Program, Machine(worker), address, out ulong entry), "controller page identity");
                return (nint)Program.FindHostPage(entry);
            }
            finally { testWorkers[worker].FullFetchActive = previous; Jit.EndRead(Program); }
        }
        internal bool Acquire(ulong pc, int worker, out TranslationLease lease, out bool multiple, out int error, uint requestedCount = 0)
        {
            At(pc, worker); var w = testWorkers[worker];
            bool previous = w.FullFetchActive; w.FullFetchActive = true;
            Jit.BeginRead(Program, false);
            try
            {
                lock (Jit.Authority.Gate)
                    return Jit.Acquire(Program, w.Machine, ref w.FullCache, default, 0, out lease, out _, out multiple, out error, requestedCount);
            }
            finally { w.FullFetchActive = previous; Jit.EndRead(Program); }
        }
        internal bool Step(ulong pc, int worker = 0, bool signalActor = false)
        { At(pc, worker); return Owner.ExecuteFullInstruction(testWorkers[worker], signalActor); }
        internal bool Continue(int worker = 0, bool signalActor = false)
        { Select(worker); return Owner.ExecuteFullInstruction(testWorkers[worker], signalActor); }
        internal (int Signal, string? Detail) StepSynchronousFault(ulong pc)
        {
            At(pc);
            for (int attempt = 0; attempt < 8; ++attempt)
            {
                try { Owner.ExecuteFullInstruction(testWorkers[0]); }
                catch (GuestSignal) { return (testWorkers[0].Signal, testWorkers[0].FirstTrap); }
            }
            throw new InvalidOperationException("Expected a synchronous guest signal from the actual controller.");
        }
        internal TranslationToken ResumeToken(int worker = 0) => testWorkers[worker].FullResume;
        internal void SignalActor(ulong pc, bool attentionBefore = true)
        {
            At(pc); Machine()->signals = 0; Machine()->restored = false;
            Machine()->attention = attentionBefore;
            ((IHostGuestThreads)Owner).RunSignalActor((nint)Machine());
        }
        internal void DataScope(int worker, Action body)
        {
            int previous = Array.IndexOf(testWorkers, Owner.current.Value);
            Select(worker);
            try
            {
                using var semantic = Jit.Authority.EnterCpuSemanticRead(testWorkers[worker].FullCache.Worker);
                using var scope = new FullDataScope(Owner, testWorkers[worker]); body();
            }
            finally { if (previous >= 0) Select(previous); }
        }
        internal void Expose(ulong pc, int worker)
        {
            Select(worker);
            byte* page = Program.LookupAddress2(Machine(worker), (long)pc, 0, 0);
            Require(page != null, "controller data exposure");
        }
        internal bool MappingBorrow(int worker) => testWorkers[worker].FullMappingBorrow;
        internal void NotifyMapping(ulong pc, int worker)
        { Select(worker); ((IHostGuestCodeRevocation)Owner).RevokeMapping((long)pc, 4096); }
        internal int Mutate(string operation, ulong pc)
        {
            Select(0); using var data = new FullDataScope(Owner, testWorkers[0]);
            return operation switch
            {
                "mprotect" => Program.ProtectVirtual(Owner.system, (long)pc, 4096, 0, false),
                "munmap" => Program.FreeVirtual(Owner.system, (long)pc, 4096),
                "MAP_FIXED" => Program.ReserveVirtual(Owner.system, (long)pc, 4096, Program.SetProtection(7), -1, 0, false, true) == (long)pc ? 0 : -1,
                _ => throw new ArgumentException(operation)
            };
        }
        internal FullBlockFrame InvokeRetained(in TranslationLease lease, ulong observedCodeEpoch)
        {
            Select(1); var w = testWorkers[1]; var header = Jit.Store.GetHeaderCopy(in lease);
            w.NativePoll ??= w.Stop.AcquireNativePoll();
            FullBlockFrame frame = new()
            {
                Version = FullBlockFrame.AbiVersion, Size = (uint)sizeof(FullBlockFrame), Machine = w.Machine,
                Stop = w.NativePoll.Pointer, Budget = Owner.instructionBudget, NativeLimit = 32,
                Header = (TranslationBlockHeader*)Jit.Store.GetHeader(in lease), OwnerCookie = header.OwnerCookie,
                BodyGeneration = header.BodyGeneration, Context = header.Context, GlobalEpochPointer = Jit.Authority.EpochPointer,
                ObservedGlobalEpoch = Jit.Authority.Epoch, CodeAuthorityEpochPointer = Jit.Authority.CodeEpochPointer,
                ObservedCodeAuthorityEpoch = observedCodeEpoch, HelperIndex = FullBlockFrame.NoHelperIndex, FirstPreissued = 1
            };
            Require(Jit.Store.TryBeginNativeEpoch(out var epoch), "controller stale-entry epoch");
            // Actual owner bodies require invocation-private completion, even
            // when a test bypasses the controller to exercise a retained entry.
            long nativeCompleted = 0;
            using (epoch)
            fixed (long* issued = &Owner.issued)
            {
                frame.Issued = issued; frame.Completed = &nativeCompleted;
                EnterX87Native(w);
                ((delegate* unmanaged<FullBlockFrame*, void>)header.Entry)(&frame);
                try { X87NativeReturned(w); }
                finally
                {
                    if (nativeCompleted != 0) Interlocked.Add(ref Owner.completed, nativeCompleted);
                }
            }
            frame.Completed = null; // returned test evidence cannot retain this stack counter
            return frame;
        }

        public void Dispose()
        {
            if (disposed) return;
            var qualificationStatistics = Statistics;
            if (Volatile.Read(ref atomicBoundFrames) != 0) throw new InvalidOperationException("R156 fixture retains live bound worker frames.");
            foreach (var worker in testWorkers)
                if (worker != null) worker.X87TerminalQuiescent = !worker.X87NativeInFlight;
            Owner.testFullMappingRevoked = null; Owner.testFullPreissued = null;
            Owner.testFullNativePrepared = null;
            Owner.testFullNativeReturned = null;
            if (Owner.fullJit != null) Jit.TestSnapshotTaken = null;
            if (Owner.program != null)
            {
                bool hadMachine = false;
                for (int i = testWorkers.Length - 1; i >= 0; --i)
                    if (testWorkers[i] != null && testWorkers[i].Machine != null)
                    { hadMachine = true; Select(i); Owner.ClearExecution(testWorkers[i]); Owner.ReleaseMachine(testWorkers[i]); }
                if (!hadMachine && Owner.system != null)
                {
                    using var data = new FullDataScope(Owner, testWorkers[0]);
                    Program.FreeSystem(Owner.system);
                }
                Owner.system = null;
                if (exitBound) Program.BlinkHostExitCallbacksEnd();
                if (signalsBound) Program.BlinkHostSignalActionsEnd();
                if (mainBound) Owner.Unbind(testWorkers[0]);
                if (Owner.memoryToken != 0)
                    Require(Program.BlinkHostMemoryDestroyShared(Owner.memoryToken) == 0, "controller memory owner cleanup");
                if (mutationBound) { Io.UnbindMemoryFileRead(Owner); Io.UnbindMemoryFileMutation(Jit); }
                foreach (var worker in testWorkers) if (worker != null) Owner.RetireX87Worker(worker);
                Owner.RetirePendingX87Rollbacks();
                foreach (var worker in testWorkers) if (worker != null) worker.FullCache.Dispose();
                foreach (var worker in testWorkers) if (worker != null) DisposeWorkerResources(worker);
                Owner.fullJit?.Dispose();
                Owner.current.Dispose();
                Owner.directories?.Dispose(); Owner.variables?.Dispose(); Owner.barrier?.Dispose();
                binding.Dispose(); Program.Dispose();
            }
            Stop.Dispose(); Io.DisposeAsync().AsTask().GetAwaiter().GetResult();
            global::Program.RecordQualificationController(qualificationStatistics);
            disposed = true;
        }
    }
}

// Test-only implementation of the parent's erasable compiler snapshot hook.
// The callback runs after plan/IR capture, before emission/publication/admission.
internal sealed unsafe partial class FullTranslationJit
{
    internal void FullTestDisableNative() => inlineEnabled = false;
    internal Action<ulong>? TestSnapshotTaken;
    partial void FullTranslationSnapshotTaken(ulong pc)
    {
        var callback = TestSnapshotTaken;
        TestSnapshotTaken = null;
        callback?.Invoke(pc);
    }
}

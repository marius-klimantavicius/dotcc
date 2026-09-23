using System.Buffers.Binary;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

public sealed record GuestThreadResult(int GuestThreadId, ulong Instructions, ulong InstructionPointer,
    string Termination, int Status, int Halt, int Signal, bool MachineReleased)
{
    public string? FirstTrap { get; init; }
}
public sealed record ThreadedGuestExecutionResult(bool Exited, int ExitStatus, ulong Instructions,
    HostExecutionStopReason StopReason, bool AllWorkersJoined, bool MemoryReleased,
    ulong RetainedBytesBeforeRelease, ulong RetainedMappingsBeforeRelease,
    IReadOnlyList<GuestThreadResult> Threads);
public sealed record ThreadedSyscallObservation(int GuestThreadId, ulong InstructionPointer, ulong Number,
    ulong Argument1, ulong Argument2, ulong Argument3, ulong Argument4,
    ulong Argument5, ulong Argument6, ulong? ReturnValue);

/// <summary>One guest process, one dedicated C# worker per upstream Machine.
/// Run is blocking and must itself run on a dedicated caller thread. The caller
/// owns IO/stop until IsQuiescent; failure to quiesce retains the owner and backing for quarantine.</summary>
public sealed unsafe class ThreadedGuestExecution : IHostGuestThreads
{
    private readonly InstanceIo io;
    private readonly HostExecutionStop stop;
    private readonly ulong memoryLimitBytes;
    private readonly object gate = new();
    private readonly List<Worker> workers = new();
    private readonly ThreadLocal<Worker?> current = new();
    private HostVariables? variables;
    private HostDirectories? directories;
    private HostProcessMemoryBarrier? barrier;
    private readonly HostEnvironment environment = new();
    private readonly HostIdentity identity = new();
    private Blink.System* system;
    private ulong memoryToken, instructionBudget;
    private long issued, completed, exitSequence;
    private bool closing, groupExited;
    private int groupStatus;
    private HostExecutionStopReason? executionOutcome;
    private Action<ThreadedSyscallObservation>? trace;
    private readonly int maximumWorkers;
    private Blink? program;
    private int runStarted;
    public bool IsQuiescent { get; private set; }
    public ulong InstructionsCompleted => (ulong)Volatile.Read(ref completed);

    private sealed class Worker
    {
        public required ThreadedGuestExecution Owner;
        public required HostExecutionStop Stop;
        public required HostSleep Sleep;
        public HostSignalWake Wake = new();
        public Blink.Machine* Machine;
        public Thread? Thread;
        public long HostIdentity;
        public int ThreadId, GuestId, Signal, Status, Halt;
        public string Termination = "None";
        public ulong Instructions, Ip;
        public bool Main, Attached, Bound, Released, Finished, Joined, TidCleared;
        public long ExitSequence;
        public Exception? Error;
        public string? FirstTrap;
        public Blink.Libc.RuntimeBinding ContextBinding;
        public Stack<Action> Unbind = new();
        public Dictionary<int, HostGuestSignalInfo> PendingSignals = new();
        public (int Signal, HostGuestSignalInfo Sender)? ImmediateSignal;
    }
    private sealed class ThreadExit : Exception { }
    private sealed class GuestSignal : Exception { }

    public ThreadedGuestExecution(InstanceIo io, HostExecutionStop stop,
        ulong memoryLimitBytes = 64UL * 1024 * 1024, int maximumWorkers = 16)
    {
        ArgumentNullException.ThrowIfNull(io); ArgumentNullException.ThrowIfNull(stop);
        if (memoryLimitBytes < 8192 || memoryLimitBytes > 256UL * 1024 * 1024 || memoryLimitBytes % 4096 != 0)
            throw new ArgumentOutOfRangeException(nameof(memoryLimitBytes), "Guest backing requires a page-aligned limit from 8 KiB through 256 MiB.");
        if (maximumWorkers is < 1 or > 256) throw new ArgumentOutOfRangeException(nameof(maximumWorkers));
        this.io = io; this.stop = stop; this.memoryLimitBytes = memoryLimitBytes;
        this.maximumWorkers = maximumWorkers;
    }

    public ThreadedGuestExecutionResult Run(string imagePath, IReadOnlyList<string> argv,
        IReadOnlyList<string> env, ulong instructionBudget,
        Action<ThreadedSyscallObservation>? syscallTrace = null, string? workingDirectory = null, Action? started = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(imagePath);
        ArgumentNullException.ThrowIfNull(argv); ArgumentNullException.ThrowIfNull(env);
        if (instructionBudget == 0 || instructionBudget > long.MaxValue) throw new ArgumentOutOfRangeException(nameof(instructionBudget));
        if (argv.Count == 0) throw new ArgumentException("argv includes argv[0].", nameof(argv));
        foreach (string value in argv.Prepend(imagePath).Concat(env))
            if (value == null || value.Contains('\0')) throw new ArgumentException("Guest strings must not contain NUL.");
        if (Interlocked.Exchange(ref runStarted, 1) != 0)
            throw new InvalidOperationException("Each execution owner runs once; create a fresh owner for the next machine run.");
        program = new Blink();
        var programBinding = program.__DotCcEnter();
        this.instructionBudget = instructionBudget; trace = syscallTrace;
        variables = new HostVariables(); directories = new HostDirectories(io); barrier = new HostProcessMemoryBarrier();
        var main = NewWorker(true, null);
        workers.Add(main);
        List<nint> strings = new();
        bool processState = false, exitState = false, memoryReleased = false, joined = false;
        ulong retainedBytes = 0, retainedMappings = 0;
        HostExecutionStopReason outcome = HostExecutionStopReason.None;
        Exception? failure = null;
        byte* Text(string value)
        {
            nint p = Marshal.StringToCoTaskMemUTF8(value); strings.Add(p); return (byte*)p;
        }
        byte** Vector(IReadOnlyList<string> values)
        {
            nint p = Marshal.AllocCoTaskMem(checked((values.Count + 1) * sizeof(nint))); strings.Add(p);
            byte** result = (byte**)p;
            for (int i = 0; i < values.Count; ++i) result[i] = Text(values[i]);
            result[values.Count] = null; return result;
        }
        void Cleanup(Action action)
        {
            try { action(); } catch (Exception error) { failure = failure == null ? error : new AggregateException(failure, error); }
        }
        try
        {
            memoryToken = program!.BlinkHostMemoryCreateShared(memoryLimitBytes);
            Require(memoryToken != 0, "Create shared memory");
            Bind(main);
            if (workingDirectory != null) Require(io.ChangeDirectory(workingDirectory).Succeeded, "Resolve guest working directory");
            Require(program!.BlinkHostSignalActionsBegin() == 0, "Begin shared signal actions"); processState = true;
            Require(program!.BlinkHostExitCallbacksBegin() == 0, "Begin shared exit callbacks"); exitState = true;
            Require(program!.BlinkHostMemoryEnablePrivateFiles() == 0, "Bind private mapped files");
            program!.InitMap(); program!.InitBus();
            Require(program!.SetOverlays(Text(""), false) == 0, "Initialize overlays");
            system = program!.NewSystem(new Blink.XedMachineMode { omode = 2, genmode = 1 });
            Require(system != null, "NewSystem");
            Require(program!.BlinkHostInitializeBoundResourceLimits(system) == 0, "Initialize guest limits");
            main.Machine = program!.NewMachine(system, null);
            Require(main.Machine != null, "NewMachine");
            main.GuestId = main.Machine->tid;
            program!.SetHostGuestCurrentMachine(main.Machine);
            main.Machine->thread = main.HostIdentity;
            system->trapexit = true;
            byte* path = Text(imagePath);
            program!.LoadProgram(main.Machine, path, path, Vector(argv), Vector(env), null);
            Require((int)system->loaded != 0 && system->elf.interpreter == null, "Load static image");
            for (int fd = 0; fd < 3; ++fd) program!.AddStdFd(&system->fds, fd);
            started?.Invoke();
            Execute(main, false);
            if (main.Error != null) throw main.Error;
        }
        catch (Exception error) { failure = error; StopGroup(); }
        finally
        {
            // Every main-loop unwind releases page/syscall locks before joining.
            if (main.Machine != null)
            {
                Cleanup(() => ClearExecution(main));
                Cleanup(() => ClearWorkerTid(main));
            }
            if (failure != null || stop.Reason != HostExecutionStopReason.None || groupExited) StopGroup();
            joined = JoinChildren();
            if (!joined)
                failure = new AggregateException(failure ?? new TimeoutException("Guest workers did not join."),
                    new InvalidOperationException("Borrowed resources remain live; retain this execution owner until its workers quiesce."));
            if (joined)
            {
                lock (gate) outcome = executionOutcome ?? stop.Reason;
                foreach (Worker child in workers.Where(w => !w.Main))
                    if (child.Error != null) failure = failure == null ? child.Error : new AggregateException(failure, child.Error);
                foreach (Worker worker in workers)
                    if (worker.Stop.NotificationFailure is { } notification)
                        failure = failure == null ? notification : new AggregateException(failure, notification);
                bool childMachinesReleased = workers.Where(w => !w.Main).All(w => w.Released);
                if (childMachinesReleased)
                {
                    bool upstreamReleased = false;
                    if (main.Machine != null)
                    {
                        Cleanup(() => ReleaseMachine(main));
                        upstreamReleased = main.Released;
                    }
                    else if (system != null && workers.Count == 1)
                        Cleanup(() => { program!.FreeSystem(system); upstreamReleased = true; });
                    else upstreamReleased = main.Released || system == null;
                    if (upstreamReleased)
                    {
                        system = null;
                        if (!groupExited && failure == null && outcome == HostExecutionStopReason.None &&
                            workers.All(w => w.Termination == "ThreadExit"))
                        {
                            groupExited = true;
                            groupStatus = workers.MaxBy(w => w.ExitSequence)!.Status;
                        }
                        if (failure == null && groupExited && outcome == HostExecutionStopReason.None && exitState)
                            Cleanup(() => Require(program!.BlinkHostExitCallbacksRun() == 0, "Run shared exit callbacks"));
                        if (exitState) Cleanup(program!.BlinkHostExitCallbacksEnd);
                        if (processState) Cleanup(program!.BlinkHostSignalActionsEnd);
                        if (main.Attached)
                            Cleanup(() => { retainedBytes = program!.BlinkHostMemoryBytes(); retainedMappings = program!.BlinkHostMemoryMappings(); });
                        Cleanup(() => Unbind(main));
                        bool bindingsReleased = workers.All(w => !w.Attached && !w.Bound && w.Unbind.Count == 0) && barrier!.Attachments == 0;
                        if (!bindingsReleased) failure ??= new InvalidOperationException("Worker bindings did not release; retain this execution owner.");
                        if (memoryToken != 0 && bindingsReleased)
                            Cleanup(() => { Require(program!.BlinkHostMemoryDestroyShared(memoryToken) == 0, "Destroy shared memory"); memoryReleased = true; });
                        else if (memoryToken == 0 && bindingsReleased) memoryReleased = true;
                    }
                    else failure ??= new InvalidOperationException("Upstream cleanup failed; shared memory remains live with this execution owner.");
                }
                else failure ??= new InvalidOperationException("A child Machine remains owned; retain this execution owner.");
                if (memoryReleased)
                {
                    foreach (nint pointer in strings) Marshal.FreeCoTaskMem(pointer);
                    Cleanup(() => directories.Dispose()); Cleanup(() => variables.Dispose()); Cleanup(() => barrier.Dispose());
                    foreach (Worker worker in workers)
                    {
                        Cleanup(() =>
                        {
                            worker.Wake.Dispose();
                            if (worker.Wake.NotificationFailure is { } error) throw error;
                        });
                        Cleanup(worker.Sleep.Dispose); Cleanup(worker.Stop.Dispose);
                    }
                    current.Dispose();
                    IsQuiescent = true;
                }
            }
        }
        // Children have left their contexts before shared backing is retired.
        // If cleanup could not quiesce, retain the context with this owner.
        // The main worker has stopped even if a child still owns resources.
        // Leave only this thread's bindings, in stack order; retain all shared
        // backing and owner objects until actual worker quiescence.
        if (!IsQuiescent) Cleanup(() => Unbind(main));
        try { programBinding.Dispose(); }
        catch (Exception error) { failure = failure == null ? error : new AggregateException(failure, error); }
        if (IsQuiescent) Cleanup(program.Dispose);
        if (stop.NotificationFailure != null) failure ??= stop.NotificationFailure;
        if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
        return new(groupExited, groupStatus, InstructionsCompleted, outcome, joined, memoryReleased,
            retainedBytes, retainedMappings, workers.Select(w => new GuestThreadResult(w.GuestId, w.Instructions,
                w.Ip, w.Termination, w.Status, w.Halt, w.Signal, w.Released) { FirstTrap = w.FirstTrap }).ToArray());
    }

    private Worker NewWorker(bool main, Blink.Machine* machine) => new()
    {
        Owner = this, Main = main, Machine = machine,
        Stop = new HostExecutionStop(cancellation: stop.Token), Sleep = new HostSleep()
    };
    private void Bind(Worker worker)
    {
        worker.ContextBinding = program!.__DotCcEnter();
        worker.ThreadId = Environment.CurrentManagedThreadId;
        worker.HostIdentity = Blink.Libc.pthread_self();
        current.Value = worker;
        void BindOne(Action bind, Action unbind) { bind(); worker.Unbind.Push(unbind); }
        barrier!.AttachCurrentThread(); worker.Unbind.Push(barrier.DetachCurrentThread);
        BindOne(() => Blink.BindHostMembarrier(barrier), Blink.UnbindHostMembarrier);
        BindOne(() => Blink.BindHostGuestThreads(this, program!), Blink.UnbindHostGuestThreads);
        BindOne(() => Blink.BindHostGuestSignals(OnSignal), Blink.UnbindHostGuestSignals);
        BindOne(() => Blink.BindHostExecutionStop(worker.Stop), Blink.UnbindHostExecutionStop);
        BindOne(() => Blink.BindHostIo(io, worker.Stop.Token, worker.Wake), Blink.UnbindHostIo);
        BindOne(() => Blink.BindHostSleep(worker.Sleep, worker.Stop.Token, worker.Wake), Blink.UnbindHostSleep);
        BindOne(() => Blink.BindHostDirectories(directories!), Blink.UnbindHostDirectories);
        BindOne(() => Blink.BindHostVariables(variables!), Blink.UnbindHostVariables);
        BindOne(() => Blink.BindHostEnvironment(environment), Blink.UnbindHostEnvironment);
        BindOne(() => Blink.BindHostIdentity(identity), Blink.UnbindHostIdentity);
        Require(program!.BlinkHostMemoryAttach(memoryToken) == 0, "Attach shared memory"); worker.Attached = true;
        worker.Bound = true;
    }
    private void Unbind(Worker worker)
    {
        List<Exception> errors = new();
        if (worker.Attached)
        {
            try { Require(program!.BlinkHostMemoryDetach() == 0, "Detach shared memory"); worker.Attached = false; }
            catch (Exception error) { errors.Add(error); }
        }
        while (worker.Unbind.TryPop(out Action? unbind))
        {
            try { unbind(); } catch (Exception error) { errors.Add(error); }
        }
        try { worker.ContextBinding.Dispose(); } catch (Exception error) { errors.Add(error); }
        current.Value = null;
        worker.Bound = errors.Count != 0;
        if (errors.Count != 0) throw new AggregateException("Worker binding cleanup failed.", errors);
    }
    private void Execute(Worker worker, bool child)
    {
        Blink.Machine* machine = worker.Machine;
        worker.GuestId = machine->tid; machine->thread = worker.HostIdentity;
        program!.SetHostGuestCurrentMachine(machine);
        ulong jumpIdentity = Blink.Libc.ArmJumpBuffer(program!.PrepareVirtualSignalJump(
            (Blink.blink_host_signal_jump_storage*)&machine->onhalt, 1));
        try
        {
            if (child) Require(program!.blink_host_sigprocmask(2, &machine->spawn_sigmask, null) == 0, "Restore child signal mask");
            while (worker.Stop.Reason == HostExecutionStopReason.None)
            {
                machine->canhalt = true;
                try
                {
                    while (worker.Stop.Reason == HostExecutionStopReason.None)
                    {
                        if ((int)Blink.Atomic.Load(ref machine->attention) != 0) { program!.CheckForSignals(machine); continue; }
                        if (!ExecuteOne(worker)) break;
                    }
                }
                catch (Blink.Libc.JumpBufferException jump) when (jump.Identity == jumpIdentity &&
                    jump.Value is -1 or -2 or -3 or -4 or -6 or -7 or -8 or -9 or 1 or 3 or 4)
                {
                    // Upstream HaltMachine has already installed the guest's
                    // signal frame. Blink() cleans up and resumes Actor at that
                    // handler; this is used by NativeAOT hardware exceptions.
                    // Unhandled signals instead unwind through OnSignal.
                    ClearExecution(worker);
                    jumpIdentity = Blink.Libc.ArmJumpBuffer(program!.PrepareVirtualSignalJump(
                        (Blink.blink_host_signal_jump_storage*)&machine->onhalt, 1));
                }
            }
            if (worker.Stop.Reason != HostExecutionStopReason.None)
                Require(machine->sysdepth == 0 && (int)machine->insyscall == 0 &&
                    machine->freelist.n == 0 && machine->pagelocks.i == 0,
                    "Stopped syscall retained live cleanup state before frontend cleanup");
            if (worker.Termination == "None") worker.Termination = "Stopped";
        }
        catch (ThreadExit) { }
        catch (GuestSignal) { worker.Termination = "GuestSignal"; StopGroup(); }
        catch (Blink.Libc.JumpBufferException jump) when (jump.Identity == jumpIdentity)
        { worker.Halt = jump.Value; worker.Termination = "Halt"; StopGroup(); }
        catch (Exception error) { worker.Error = error; worker.Termination = "Failure"; StopGroup(); }
        finally { worker.Ip = machine->ip; ClearExecution(worker); }
    }
    // Normal execution and recursive upstream signal delivery share this exact
    // instruction reservation, trace and completion path. Handler work cannot
    // escape the process-wide budget or the worker's monotonic stop token.
    private bool ExecuteOne(Worker worker)
    {
        if (worker.Stop.Reason != HostExecutionStopReason.None) return false;
        if ((ulong)Interlocked.Increment(ref issued) > instructionBudget)
        { stop.RequestBudgetStop(); return false; }
        ThreadedSyscallObservation? observation = trace == null ? null : Observe(worker);
        bool returned = false;
        ulong instructionPointer = worker.Machine->ip;
        try { program!.ExecuteInstruction(worker.Machine); returned = true; }
        catch (Blink.Libc.JumpBufferException jump)
        {
            worker.FirstTrap ??= $"ip=0x{instructionPointer:x} address=0x{worker.Machine->faultaddr:x} code={jump.Value}";
            throw;
        }
        finally
        {
            if (observation != null)
            {
                var item = observation with { ReturnValue = returned ? Register(worker.Machine, 0) : null };
                lock (gate) trace!(item);
            }
        }
        ++worker.Instructions; Interlocked.Increment(ref completed);
        return true;
    }
    void IHostGuestThreads.RunSignalActor(nint pointer)
    {
        Worker worker = RequireCurrent();
        Require((nint)worker.Machine == pointer, "Recursive signal execution owner");
        var machine = worker.Machine;
        // Preserve upstream SignalActor ordering: execute, then test restored
        // before consuming attention. On stop, return through its original
        // DeliverSignalRecursively/syscall cleanup rather than throw across it.
        while (ExecuteOne(worker))
        {
            if ((int)Blink.Atomic.Load(ref machine->attention) != 0)
            {
                if ((int)machine->restored != 0) break;
                program!.CheckForSignals(machine);
            }
        }
    }
    private void ClearExecution(Worker worker)
    {
        var machine = worker.Machine;
        if (machine == null) return;
        machine->sysdepth = 0; machine->sigdepth = 0;
        machine->canhalt = false; machine->nofault = false; machine->insyscall = false;
        program!.CollectPageLocks(machine); program!.CollectGarbage(machine, 0);
    }
    private void ClearWorkerTid(Worker worker)
    {
        if (worker.Machine == null || worker.TidCleared) return;
        program!.ClearChildTid(worker.Machine);
        worker.TidCleared = true;
    }
    private void ReleaseMachine(Worker worker)
    {
        var machine = worker.Machine;
        if (machine == null) return;
        ClearWorkerTid(worker);
        program!.SetHostGuestCurrentMachine(null);
        // FreeMachine removes the target under upstream machines_lock. Until
        // then, a concurrent SysTkill may still legitimately enumerate it and
        // needs its owning metadata/wake target. Never hold gate across free.
        program!.FreeMachine(machine);
        lock (worker.Owner.gate)
        {
            worker.PendingSignals.Clear(); worker.ImmediateSignal = null;
            worker.Machine = null; worker.Released = true;
        }
    }
    int IHostGuestThreads.Start(nint pointer)
    {
        _ = RequireCurrent();
        var child = (Blink.Machine*)pointer;
        if (child == null || child->blink_host_system != system) return 22;
        lock (gate)
        {
            if (closing || stop.Reason != HostExecutionStopReason.None || workers.Count == maximumWorkers) return 11;
            Worker? worker = null;
            try
            {
                worker = NewWorker(false, child);
                worker.Thread = new Thread(() => ChildMain(worker)) { IsBackground = true, Name = "translated-guest-child" };
                workers.Add(worker);
                worker.Thread.Start();
                return 0; // No throwing work may follow successful Start.
            }
            catch (Exception error) when (error is OutOfMemoryException or ThreadStateException)
            {
                if (worker != null) { workers.Remove(worker); worker.Wake.Dispose(); worker.Sleep.Dispose(); worker.Stop.Dispose(); }
                return 11;
            }
        }
    }
    private void ChildMain(Worker worker)
    {
        try { Bind(worker); Execute(worker, true); }
        catch (Exception error) { worker.Error = error; StopGroup(); }
        finally
        {
            try
            {
                if (worker.Attached) { ClearExecution(worker); ReleaseMachine(worker); }
                Unbind(worker);
            }
            catch (Exception error) { worker.Error = worker.Error == null ? error : new AggregateException(worker.Error, error); StopGroup(); }
            lock (gate) { worker.Finished = true; Monitor.PulseAll(gate); }
        }
    }
    void IHostGuestThreads.Exit(nint pointer, int status, bool group)
    {
        Worker worker = RequireCurrent();
        Require((nint)worker.Machine == pointer, "Exit from owning Machine");
        lock (gate)
        {
            worker.Status = status; worker.Termination = group ? "GroupExit" : "ThreadExit";
            worker.ExitSequence = Interlocked.Increment(ref exitSequence);
            if (group && !groupExited) { groupExited = true; groupStatus = status; }
            // Freeze the execution outcome at the actual group exit, or the
            // last ordinary thread exit. Later teardown time is not guest time.
            if (group || workers.All(w => w.Termination == "ThreadExit"))
                executionOutcome ??= stop.Reason;
        }
        if (group) StopGroup();
        throw new ThreadExit();
    }
    void IHostGuestThreads.StopOthers(nint pointer)
        => throw new NotSupportedException("Only the explicit managed group-exit owner coordinates stop/join in this profile.");
    int IHostGuestThreads.Signal(long thread, int signal)
    {
        lock (gate)
        {
            Worker? worker = workers.FirstOrDefault(w => !w.Finished && w.HostIdentity == thread);
            if (worker == null) return 3;
            return signal == 0 ? 0 : 95;
        }
    }
    int IHostGuestThreads.WakeSignal(nint pointer)
    {
        _ = RequireCurrent();
        Worker? worker;
        lock (gate)
            worker = workers.FirstOrDefault(w => !w.Finished && !w.Released && (nint)w.Machine == pointer);
        if (worker == null) return 3;
        // NewMachine initially stores its creator's pthread identity. Address
        // the actual owned Machine even before its child has bound or started.
        // Request only schedules callbacks; it never runs cancellation under
        // this caller's upstream machines_lock. No native signal is emitted.
        worker.Wake.Request();
        return 0;
    }
    void IHostGuestThreads.SignalCheckpoint(nint pointer)
    {
        Worker worker = RequireCurrent();
        Require((nint)worker.Machine == pointer, "Signal checkpoint owner");
        worker.Wake.Checkpoint();
    }
    void IHostGuestThreads.EnqueueSignalInfo(nint pointer, int signal, int processId, uint userId)
    {
        _ = RequireCurrent();
        Require(signal is >= 1 and <= 64 && processId > 0, "Queued signal sender");
        lock (gate)
        {
            Worker worker = workers.Single(w => (nint)w.Machine == pointer && !w.Released);
            // Called under the upstream sig_lock, before its pending bit is
            // set. Coalesced delivery preserves the first actual sender.
            if ((worker.Machine->signals & (1UL << (signal - 1))) == 0)
                worker.PendingSignals[signal] = new(processId, userId);
        }
    }
    void IHostGuestThreads.DeliverThreadSignal(nint pointer, int signal, int processId, uint userId)
    {
        Worker worker = RequireCurrent();
        Require((nint)worker.Machine == pointer && signal is >= 1 and <= 64 && processId > 0,
            "Immediate signal sender");
        var previous = worker.ImmediateSignal;
        worker.ImmediateSignal = (signal, new(processId, userId));
        try { program!.DeliverSignal(worker.Machine, signal, -6); } // SI_TKILL_LINUX
        finally { worker.ImmediateSignal = previous; }
    }
    HostGuestSignalInfo? IHostGuestThreads.TakeSignalInfo(nint pointer, int signal, bool discard)
    {
        Worker worker = RequireCurrent();
        Require((nint)worker.Machine == pointer && signal is >= 1 and <= 64, "Signal metadata owner");
        if (!discard && worker.ImmediateSignal is { } immediate && immediate.Signal == signal)
            return immediate.Sender;
        lock (gate)
        {
            // ConsumeSignalImpl clears the selected pending bit under sig_lock
            // before building its frame. An unrelated synchronous fault with
            // the same number must not steal still-pending sender metadata.
            if (!discard && (worker.Machine->signals & (1UL << (signal - 1))) != 0) return null;
            bool found = worker.PendingSignals.Remove(signal, out var sender);
            return !discard && found ? sender : null;
        }
    }
    private Worker RequireCurrent()
    {
        Worker? worker = current.Value;
        if (worker == null || worker.ThreadId != Environment.CurrentManagedThreadId || worker.Machine == null)
            throw new InvalidOperationException("Guest callback does not belong to this execution thread.");
        return worker;
    }
    private void OnSignal(nint pointer, int signal, int code)
    {
        Worker worker = RequireCurrent(); Require((nint)worker.Machine == pointer, "Signal owner");
        worker.Signal = signal; throw new GuestSignal();
    }
    private void StopGroup()
    {
        Worker[] snapshot;
        lock (gate) { closing = true; snapshot = workers.ToArray(); }
        foreach (Worker worker in snapshot) worker.Stop.RequestStop();
    }
    private bool JoinChildren()
    {
        while (true)
        {
            Worker[] snapshot;
            lock (gate) snapshot = workers.Where(w => !w.Main && !w.Joined).ToArray();
            if (snapshot.Length == 0) return true;
            foreach (Worker worker in snapshot)
            {
                if (stop.Reason != HostExecutionStopReason.None) StopGroup();
                if (worker.Thread!.Join(TimeSpan.FromMilliseconds(100))) { worker.Joined = true; continue; }
                if (closing)
                {
                    if (!worker.Thread.Join(TimeSpan.FromSeconds(5))) return false;
                    worker.Joined = true;
                }
            }
        }
    }
    private ulong Register(Blink.Machine* machine, int index)
    {
        byte* p = program!.GetModrmRegisterWordPointerRead8(machine, (3UL << 22) | ((ulong)index << 7), 0, 0);
        return BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(p, 8));
    }
    private ThreadedSyscallObservation? Observe(Worker worker)
    {
        int saved = Blink.Libc.errno;
        try
        {
            Blink.XedDecodedInst decoded = default;
            var machine = worker.Machine;
            if (program!.GetInstruction(machine, (long)machine->ip, &decoded) != 0 || ((decoded.op.rde >> 40) & 0x7ff) != 0x105) return null;
            return new(worker.GuestId, machine->ip, Register(machine, 0), Register(machine, 7), Register(machine, 6),
                Register(machine, 2), Register(machine, 10), Register(machine, 8), Register(machine, 9), null);
        }
        finally { Blink.Libc.errno = saved; }
    }
    private static void Require(bool value, string message)
    { if (!value) throw new InvalidOperationException(message + " failed."); }
}

using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Buffers.Binary;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

public sealed record GuestExecutionResult(ulong Instructions, ulong InstructionPointer,
    int Halt, int Signal, int SignalCode, bool Exited, int ExitStatus,
    HostExecutionStopReason StopReason, ulong RetainedBytesBeforeRelease,
    ulong RetainedMappingsBeforeRelease, bool MemoryReleased);

/// <summary>Scalar machine state captured on an execution exception, before
/// cleanup. Arguments are the current Linux x64 argument registers, not an
/// assertion that the failing operation was a syscall.</summary>
public sealed record GuestExecutionFailureState(ulong InstructionPointer, ulong Accumulator,
    ulong Argument1, ulong Argument2, ulong Argument3, ulong Argument4,
    ulong Argument5, ulong Argument6, int HostError);

/// <summary>One blocking execution on its calling thread. The caller owns IO and
/// cancellation and must keep them alive until Run returns. All translated calls,
/// including teardown, happen on this thread; discard the process afterward.</summary>
public sealed unsafe class GuestExecution
{
    private static int processUsed;
    private readonly InstanceIo io;
    private readonly HostExecutionStop stop;
    private Blink.Machine* machine;
    private Blink.System* pendingSystem;
    private int threadId, signal, signalCode;
    private ulong instructionsCompleted;
    private HostSleep? activeSleep;
    private sealed class UnhandledGuestSignalException : Exception { }
    public ulong InstructionsCompleted => Volatile.Read(ref instructionsCompleted);
    public bool IsSleeping => Volatile.Read(ref activeSleep)?.IsWaiting ?? false;
    public GuestExecutionFailureState? LastFailureState { get; private set; }

    public GuestExecution(InstanceIo io, HostExecutionStop stop)
    {
        ArgumentNullException.ThrowIfNull(io);
        ArgumentNullException.ThrowIfNull(stop);
        this.io = io; this.stop = stop;
    }

    public GuestExecutionResult Run(string imagePath, IReadOnlyList<string> argv,
        IReadOnlyList<string> env, ulong instructionBudget, bool allowInterpreter = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(imagePath);
        ArgumentNullException.ThrowIfNull(argv);
        ArgumentNullException.ThrowIfNull(env);
        if (instructionBudget == 0) throw new ArgumentOutOfRangeException(nameof(instructionBudget));
        if (argv.Count == 0) throw new ArgumentException("argv must include argv[0].", nameof(argv));
        foreach (string value in argv.Prepend(imagePath).Concat(env))
            if (value == null || value.Contains('\0')) throw new ArgumentException("Guest strings must be nonnull and contain no NUL.");
        if (Interlocked.Exchange(ref processUsed, 1) != 0)
            throw new InvalidOperationException("The translated core may execute only once in a process; its static caches are not reusable after disposal.");
        threadId = Environment.CurrentManagedThreadId;
        var strings = new List<nint>();
        var bindings = new Stack<Action>();
        bool memoryOwner = false, signalOwner = false, exitOwner = false;
        bool ordinaryExit = false;
        bool memoryReleased = false;
        ulong retainedBytes = 0, retainedMappings = 0;
        ulong instructions = 0, ip = 0;
        int halt = 0, status = 0;
        bool exited = false;
        HostExecutionStopReason completionReason = HostExecutionStopReason.None;
        Exception? failure = null;
        using var variables = new HostVariables();
        using var sleep = new HostSleep();
        Volatile.Write(ref activeSleep, sleep);
        using var directories = new HostDirectories(io);
        void Bind(Action bind, Action unbind) { bind(); bindings.Push(unbind); }
        void Cleanup(Action action)
        {
            try { action(); }
            catch (Exception error) { failure = failure == null ? error : new AggregateException(failure, error); }
        }
        byte* Text(string value)
        {
            nint pointer = Marshal.StringToCoTaskMemUTF8(value);
            strings.Add(pointer); return (byte*)pointer;
        }
        byte** Vector(IReadOnlyList<string> values)
        {
            nint pointer = Marshal.AllocCoTaskMem(checked((values.Count + 1) * sizeof(nint)));
            strings.Add(pointer);
            byte** result = (byte**)pointer;
            for (int i = 0; i < values.Count; ++i) result[i] = Text(values[i]);
            result[values.Count] = null;
            return result;
        }
        try
        {
            Bind(() => Blink.BindHostExecutionStop(stop), Blink.UnbindHostExecutionStop);
            Bind(() => Blink.BindHostGuestSignals(OnSignal), Blink.UnbindHostGuestSignals);
            Bind(() => Blink.BindHostIo(io, stop.Token), Blink.UnbindHostIo);
            Bind(() => Blink.BindHostDirectories(directories), Blink.UnbindHostDirectories);
            Bind(() => Blink.BindHostVariables(variables), Blink.UnbindHostVariables);
            Bind(() => Blink.BindHostSleep(sleep, stop.Token), Blink.UnbindHostSleep);
            Bind(() => Blink.BindHostEnvironment(new HostEnvironment()), Blink.UnbindHostEnvironment);
            Bind(() => Blink.BindHostIdentity(new HostIdentity()), Blink.UnbindHostIdentity);
            Require(Blink.BlinkHostMemoryBegin(64 * 1024 * 1024) == 0, "Begin memory"); memoryOwner = true;
            Require(Blink.BlinkHostSignalActionsBegin() == 0, "Begin signal actions"); signalOwner = true;
            Require(Blink.BlinkHostExitCallbacksBegin() == 0, "Begin exit callbacks"); exitOwner = true;
            Require(Blink.BlinkHostMemoryEnablePrivateFiles() == 0, "Enable private file mappings");
            Blink.InitMap(); Blink.InitBus();
            Require(Blink.SetOverlays(Text(""), false) == 0, "Initialize overlays");
            Blink.System* system = Blink.NewSystem(new Blink.XedMachineMode { omode = 2, genmode = 1 });
            Require(system != null, "NewSystem");
            pendingSystem = system;
            Require(Blink.BlinkHostInitializeBoundResourceLimits(system) == 0, "Initialize guest resource limits");
            machine = Blink.NewMachine(system, null);
            Require(machine != null, "NewMachine");
            pendingSystem = null;
            Blink.Globals.g_machine = machine;
            system->trapexit = true;
            byte* path = Text(imagePath);
            Blink.LoadProgram(machine, path, path, Vector(argv), Vector(env), null);
            Require((int)system->loaded != 0, "Load image");
            Require(allowInterpreter || system->elf.interpreter == null,
                "Static image required by the selected execution profile");
            for (int fd = 0; fd < 3; ++fd) Blink.AddStdFd(&system->fds, fd);

            // Same arm/identity-filter boundary emitted for upstream Blink's
            // sigsetjmp. The virtual signal adapter restores the saved mask
            // before the runtime exception arrives here.
            ulong identity = Blink.Libc.ArmJumpBuffer(Blink.PrepareVirtualSignalJump(
                (Blink.blink_host_signal_jump_storage*)&machine->onhalt, 1));
            try
            {
                machine->canhalt = true;
                while (signal == 0 && (int)system->exited == 0)
                {
                    if (stop.Reason != HostExecutionStopReason.None) break;
                    if (instructions == instructionBudget) { stop.RequestBudgetStop(); break; }
                    // Mirror Actor's actual attention branch; only this thread
                    // accesses Machine, including signal processing and GC.
                    if ((int)Blink.Atomic.Load(ref machine->attention) == 0)
                    {
                        Blink.ExecuteInstruction(machine);
                        ++instructions;
                        Volatile.Write(ref instructionsCompleted, instructions);
                    }
                    else Blink.CheckForSignals(machine);
                    if (stop.Reason != HostExecutionStopReason.None) break;
                }
                if (stop.Reason != HostExecutionStopReason.None)
                    Require(machine->sysdepth == 0 && (int)machine->insyscall == 0 &&
                        machine->freelist.n == 0 && machine->pagelocks.i == 0,
                        "Stopped syscall left live cleanup state before frontend cleanup");
            }
            catch (Blink.Libc.JumpBufferException jump) when (jump.Identity == identity) { halt = jump.Value; }
            catch (UnhandledGuestSignalException) { halt = machine->trapno; }
            catch
            {
                LastFailureState = CaptureFailureState();
                throw;
            }
            finally
            {
                completionReason = stop.Reason;
                // Qualified profile has DISABLE_JIT, so there is no path to
                // abandon. Host fatal signals are not software guest faults.
                machine->sysdepth = 0; machine->sigdepth = 0;
                machine->canhalt = false; machine->nofault = false; machine->insyscall = false;
                Blink.CollectPageLocks(machine); Blink.CollectGarbage(machine, 0);
            }
            ip = machine->ip; exited = (int)system->exited != 0; status = system->exitcode;
            ordinaryExit = exited && signal == 0 && completionReason == HostExecutionStopReason.None;
            if (stop.NotificationFailure != null) throw stop.NotificationFailure;
        }
        catch (Exception error)
        {
            LastFailureState ??= CaptureFailureState();
            failure = error;
        }
        finally
        {
            // Clear before freeing: a thrown cleanup callback can never cause
            // a second FreeMachine. Host bindings remain valid through atexit.
            if (machine != null)
            {
                nint value = (nint)machine; machine = null;
                Cleanup(() => Blink.FreeMachine((Blink.Machine*)value));
            }
            if (pendingSystem != null)
            {
                nint value = (nint)pendingSystem; pendingSystem = null;
                Cleanup(() => Blink.FreeSystem((Blink.System*)value));
            }
            if (ordinaryExit && failure == null && exitOwner)
                Cleanup(() => Require(Blink.BlinkHostExitCallbacksRun() == 0, "Run exit callbacks"));
            if (exitOwner) Cleanup(Blink.BlinkHostExitCallbacksEnd);
            if (signalOwner) Cleanup(Blink.BlinkHostSignalActionsEnd);
            if (memoryOwner)
            {
                Cleanup(() => { retainedBytes = Blink.BlinkHostMemoryBytes(); retainedMappings = Blink.BlinkHostMemoryMappings(); });
                Cleanup(() => { Blink.BlinkHostMemoryDisposeWorker(); memoryReleased = true; });
            }
            while (bindings.TryPop(out Action? unbind)) Cleanup(unbind);
            foreach (nint pointer in strings) Marshal.FreeCoTaskMem(pointer);
            Volatile.Write(ref activeSleep, null);
        }
        if (stop.NotificationFailure != null && failure == null) failure = stop.NotificationFailure;
        if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
        return new(instructions, ip, halt, signal, signalCode, exited, status, completionReason,
            retainedBytes, retainedMappings, memoryReleased);
    }

    private void OnSignal(nint pointer, int number, int code)
    {
        if (Environment.CurrentManagedThreadId != threadId || machine == null || (nint)machine != pointer)
            throw new InvalidOperationException("Guest signal reached a different execution owner.");
        signal = number; signalCode = code;
        throw new UnhandledGuestSignalException();
    }
    private GuestExecutionFailureState? CaptureFailureState()
    {
        if (machine == null) return null;
        int error = Blink.Libc.errno;
        ulong Register(int index)
        {
            // Upstream rde.h: ModrmMod=3 selects only a register; RexbRm
            // occupies bits 7..10. No guest memory access or layout offsets.
            ulong rde = (3UL << 22) | ((ulong)index << 7);
            byte* value = Blink.GetModrmRegisterWordPointerRead8(machine, rde, 0, 0);
            return BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(value, 8));
        }
        return new(machine->ip, Register(0), Register(7), Register(6), Register(2),
            Register(10), Register(8), Register(9), error);
    }
    private static void Require(bool condition, string operation)
    {
        if (!condition) throw new InvalidOperationException(operation + " failed.");
    }
}

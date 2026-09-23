using global::System;
using global::System.Buffers.Binary;
using Managed.Emulation.Host;

namespace Managed.Emulation;

public partial class BlinkCore
{
    [ThreadStatic] private static IHostGuestThreads? guestThreads;
    // The threaded upstream profile declares g_machine as TLS. Keep access to
    // generated storage inside this assembly; lifecycle remains in the C# API.
#if DOTCC_INSTANCE_FOR_HOST
    public unsafe void SetHostGuestCurrentMachine(Machine* machine)
#else
    public static unsafe void SetHostGuestCurrentMachine(Machine* machine)
#endif
        => ThreadGlobals.g_machine = machine;
#if DOTCC_INSTANCE_FOR_HOST
    [ThreadStatic] private static BlinkCore? guestProgram;
    private static IHostGuestThreads GuestOwner(BlinkCore program)
    {
        if (!ReferenceEquals(guestProgram, program)) throw new InvalidOperationException("Guest callback program is not bound to its owning worker.");
        return GuestThreadOwner;
    }
    public static void BindHostGuestThreads(IHostGuestThreads owner, BlinkCore program)
#else
    public static void BindHostGuestThreads(IHostGuestThreads owner)
#endif
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (guestThreads != null) throw new InvalidOperationException("Guest thread owner already bound.");
        guestThreads = owner;
#if DOTCC_INSTANCE_FOR_HOST
        guestProgram = program;
#endif
    }
    public static void UnbindHostGuestThreads()
    {
        guestThreads = null;
#if DOTCC_INSTANCE_FOR_HOST
        guestProgram = null;
#endif
    }
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe int blink_host_guest_thread_start(BlinkCore program, Machine* child)
#else
    public static unsafe int blink_host_guest_thread_start(Machine* child)
#endif
#if DOTCC_INSTANCE_FOR_HOST
        => GuestOwner(program).Start((nint)child);
#else
        => guestThreads?.Start((nint)child) ?? 19;
#endif
    [global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public static unsafe void blink_host_guest_thread_exit(Machine* machine, int status)
    {
        (guestThreads ?? throw new InvalidOperationException("Guest thread owner is unbound.")).Exit((nint)machine, status, false);
        throw new InvalidOperationException("Guest thread exit returned.");
    }
    [global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_group_exit(BlinkCore program, Machine* machine, int status)
#else
    public static unsafe void blink_host_guest_group_exit(Machine* machine, int status)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        GuestOwner(program).Exit((nint)machine, status, true);
#else
        (guestThreads ?? throw new InvalidOperationException("Guest thread owner is unbound.")).Exit((nint)machine, status, true);
#endif
        throw new InvalidOperationException("Guest group exit returned.");
    }
    [global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_exit(BlinkCore program, Machine* machine, int status)
#else
    public static unsafe void blink_host_guest_exit(Machine* machine, int status)
#endif
    {
        // Preserve the upstream HAVE_THREADS SysExit decision and its lock
        // semantics by calling the generated, unchanged IsOrphan exactly once.
#if DOTCC_INSTANCE_FOR_HOST
        _ = GuestOwner(program);
        if ((int)program.IsOrphan(machine) != 0) blink_host_guest_group_exit(program, machine, status);
#else
        if ((int)IsOrphan(machine) != 0) blink_host_guest_group_exit(machine, status);
#endif
        else blink_host_guest_thread_exit(machine, status);
        throw new InvalidOperationException("Guest exit returned.");
    }
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_stop_other_threads(BlinkCore program, System* system)
#else
    public static unsafe void blink_host_guest_stop_other_threads(System* system)
#endif
#if DOTCC_INSTANCE_FOR_HOST
        => GuestOwner(program).StopOthers((nint)system);
#else
        => (guestThreads ?? throw new InvalidOperationException("Guest thread owner is unbound.")).StopOthers((nint)system);
#endif
    public static unsafe int blink_host_guest_pthread_sigmask(int how, blink_host_sigset* mask, blink_host_sigset* previous)
    {
        int saved = Libc.errno;
        try
        {
#if DOTCC_INSTANCE_FOR_HOST
            int result = (guestProgram ?? throw new InvalidOperationException("Guest program is unbound."))
                .blink_host_sigprocmask(how, mask, previous);
#else
            int result = blink_host_sigprocmask(how, mask, previous);
#endif
            return result == 0 ? 0 : Libc.errno;
        }
        finally { Libc.errno = saved; }
    }
    public static int blink_host_guest_pthread_kill(long thread, int signal)
    {
        int saved = Libc.errno;
        try { return guestThreads?.Signal(thread, signal) ?? 19; }
        finally { Libc.errno = saved; }
    }
    private static IHostGuestThreads GuestThreadOwner => guestThreads
        ?? throw new InvalidOperationException("Guest thread owner is unbound.");
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_signal_actor(BlinkCore program, Machine* machine)
#else
    public static unsafe void blink_host_guest_signal_actor(Machine* machine)
#endif
#if DOTCC_INSTANCE_FOR_HOST
        => GuestOwner(program).RunSignalActor((nint)machine);
#else
        => GuestThreadOwner.RunSignalActor((nint)machine);
#endif
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_signal_checkpoint(BlinkCore program, Machine* machine)
#else
    public static unsafe void blink_host_guest_signal_checkpoint(Machine* machine)
#endif
#if DOTCC_INSTANCE_FOR_HOST
        => GuestOwner(program).SignalCheckpoint((nint)machine);
#else
        => GuestThreadOwner.SignalCheckpoint((nint)machine);
#endif
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe int blink_host_guest_signal_wake(BlinkCore program, Machine* machine)
#else
    public static unsafe int blink_host_guest_signal_wake(Machine* machine)
#endif
    {
        int saved = Libc.errno;
#if DOTCC_INSTANCE_FOR_HOST
        try { return GuestOwner(program).WakeSignal((nint)machine); }
#else
        try { return GuestThreadOwner.WakeSignal((nint)machine); }
#endif
        finally { Libc.errno = saved; }
    }
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_signal_enqueue_info(BlinkCore program, Machine* machine, int signal, int processId, uint userId)
#else
    public static unsafe void blink_host_guest_signal_enqueue_info(Machine* machine, int signal, int processId, uint userId)
#endif
#if DOTCC_INSTANCE_FOR_HOST
        => GuestOwner(program).EnqueueSignalInfo((nint)machine, signal, processId, userId);
#else
        => GuestThreadOwner.EnqueueSignalInfo((nint)machine, signal, processId, userId);
#endif
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_signal_deliver_tkill(BlinkCore program, Machine* machine, int signal, int processId, uint userId)
#else
    public static unsafe void blink_host_guest_signal_deliver_tkill(Machine* machine, int signal, int processId, uint userId)
#endif
#if DOTCC_INSTANCE_FOR_HOST
        => GuestOwner(program).DeliverThreadSignal((nint)machine, signal, processId, userId);
#else
        => GuestThreadOwner.DeliverThreadSignal((nint)machine, signal, processId, userId);
#endif
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe void blink_host_guest_signal_apply_info(BlinkCore program, Machine* machine, int signal, siginfo_linux* info)
#else
    public static unsafe void blink_host_guest_signal_apply_info(Machine* machine, int signal, siginfo_linux* info)
#endif
    {
#if DOTCC_INSTANCE_FOR_HOST
        var sender = GuestOwner(program).TakeSignalInfo((nint)machine, signal, info == null);
#else
        var sender = GuestThreadOwner.TakeSignalInfo((nint)machine, signal, info == null);
#endif
        if (info == null || sender is not { } value) return;
        // Pinned upstream Linux siginfo layout: code at 8, sender PID/UID at
        // 16/20. The reviewed signal staging checks these offsets in C.
        BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>((byte*)info + 8, 4), -6); // SI_TKILL
        BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>((byte*)info + 16, 4), value.ProcessId);
        BinaryPrimitives.WriteUInt32LittleEndian(new Span<byte>((byte*)info + 20, 4), value.UserId);
    }
    // raise addresses this private process only. Positive asynchronous signals
    // retain the explicit signal-policy error until delivery is qualified.
    public static int blink_host_raise(int signal) => blink_host_kill(0, signal);
#if DOTCC_INSTANCE_FOR_HOST
    public static unsafe int blink_host_guest_pthread_atfork(BlinkCore program, delegate*<BlinkCore, void> prepare, delegate*<BlinkCore, void> parent, delegate*<BlinkCore, void> child)
#else
    public static unsafe int blink_host_guest_pthread_atfork(delegate*<void> prepare, delegate*<void> parent, delegate*<void> child)
#endif
        => 95; // Explicit no-fork profile; do not pretend to register callbacks.
}

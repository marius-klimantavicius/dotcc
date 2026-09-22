using global::System;
using global::System.Buffers.Binary;
using Managed.Emulation.Host;

namespace Managed.Emulation;

public static partial class BlinkCore
{
    [ThreadStatic] private static IHostGuestThreads? guestThreads;
    // The threaded upstream profile declares g_machine as TLS. Keep access to
    // generated storage inside this assembly; lifecycle remains in the C# API.
    public static unsafe void SetHostGuestCurrentMachine(Machine* machine)
        => ThreadGlobals.g_machine = machine;
    public static void BindHostGuestThreads(IHostGuestThreads owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (guestThreads != null) throw new InvalidOperationException("Guest thread owner already bound.");
        guestThreads = owner;
    }
    public static void UnbindHostGuestThreads() => guestThreads = null;
    public static unsafe int blink_host_guest_thread_start(Machine* child)
        => guestThreads?.Start((nint)child) ?? 19;
    [global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public static unsafe void blink_host_guest_thread_exit(Machine* machine, int status)
    {
        (guestThreads ?? throw new InvalidOperationException("Guest thread owner is unbound.")).Exit((nint)machine, status, false);
        throw new InvalidOperationException("Guest thread exit returned.");
    }
    [global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public static unsafe void blink_host_guest_group_exit(Machine* machine, int status)
    {
        (guestThreads ?? throw new InvalidOperationException("Guest thread owner is unbound.")).Exit((nint)machine, status, true);
        throw new InvalidOperationException("Guest group exit returned.");
    }
    [global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public static unsafe void blink_host_guest_exit(Machine* machine, int status)
    {
        // Preserve the upstream HAVE_THREADS SysExit decision and its lock
        // semantics by calling the generated, unchanged IsOrphan exactly once.
        if ((int)IsOrphan(machine) != 0) blink_host_guest_group_exit(machine, status);
        else blink_host_guest_thread_exit(machine, status);
        throw new InvalidOperationException("Guest exit returned.");
    }
    public static unsafe void blink_host_guest_stop_other_threads(System* system)
        => (guestThreads ?? throw new InvalidOperationException("Guest thread owner is unbound.")).StopOthers((nint)system);
    public static unsafe int blink_host_guest_pthread_sigmask(int how, blink_host_sigset* mask, blink_host_sigset* previous)
    {
        int saved = Libc.errno;
        try { return blink_host_sigprocmask(how, mask, previous) == 0 ? 0 : Libc.errno; }
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
    public static unsafe void blink_host_guest_signal_actor(Machine* machine)
        => GuestThreadOwner.RunSignalActor((nint)machine);
    public static unsafe void blink_host_guest_signal_checkpoint(Machine* machine)
        => GuestThreadOwner.SignalCheckpoint((nint)machine);
    public static unsafe int blink_host_guest_signal_wake(Machine* machine)
    {
        int saved = Libc.errno;
        try { return GuestThreadOwner.WakeSignal((nint)machine); }
        finally { Libc.errno = saved; }
    }
    public static unsafe void blink_host_guest_signal_enqueue_info(Machine* machine, int signal, int processId, uint userId)
        => GuestThreadOwner.EnqueueSignalInfo((nint)machine, signal, processId, userId);
    public static unsafe void blink_host_guest_signal_deliver_tkill(Machine* machine, int signal, int processId, uint userId)
        => GuestThreadOwner.DeliverThreadSignal((nint)machine, signal, processId, userId);
    public static unsafe void blink_host_guest_signal_apply_info(Machine* machine, int signal, siginfo_linux* info)
    {
        var sender = GuestThreadOwner.TakeSignalInfo((nint)machine, signal, info == null);
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
    public static unsafe int blink_host_guest_pthread_atfork(delegate*<void> prepare, delegate*<void> parent, delegate*<void> child)
        => 95; // Explicit no-fork profile; do not pretend to register callbacks.
}

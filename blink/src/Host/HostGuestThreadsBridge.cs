using global::System;
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
    public static unsafe void blink_host_guest_thread_exit(Machine* machine, int status)
    {
        (guestThreads ?? throw new InvalidOperationException("Guest thread owner is unbound.")).Exit((nint)machine, status, false);
        throw new InvalidOperationException("Guest thread exit returned.");
    }
    public static unsafe void blink_host_guest_group_exit(Machine* machine, int status)
    {
        (guestThreads ?? throw new InvalidOperationException("Guest thread owner is unbound.")).Exit((nint)machine, status, true);
        throw new InvalidOperationException("Guest group exit returned.");
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
    // raise addresses this private process only. Positive asynchronous signals
    // retain the explicit signal-policy error until delivery is qualified.
    public static int blink_host_raise(int signal) => blink_host_kill(0, signal);
    public static unsafe int blink_host_guest_pthread_atfork(delegate*<void> prepare, delegate*<void> parent, delegate*<void> child)
        => 95; // Explicit no-fork profile; do not pretend to register callbacks.
}

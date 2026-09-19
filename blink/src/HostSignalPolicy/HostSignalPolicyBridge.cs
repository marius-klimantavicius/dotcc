namespace Managed.Emulation;
#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    private static int SignalPolicyError(int error) { Libc.errno = error; return -1; }
    private static int SignalPolicyOwner() => identity == null ? SignalPolicyError(19) : 0;
    private static int TimerSelector(int which)
    {
        if (SignalPolicyOwner() != 0) return -1;
        return (uint)which <= 2 ? 0 : SignalPolicyError(22);
    }
    // The selected worker profile has no asynchronous signal-delivery source.
    // Its interval timers remain disarmed; attempting to arm one is refused.
    public static unsafe int blink_host_getitimer(int which, itimerval* value)
    {
        if (TimerSelector(which) != 0) return -1;
        if (value == null) return SignalPolicyError(14);
        *value = default;
        return 0;
    }
    public static unsafe int blink_host_setitimer(int which, itimerval* value, itimerval* previous)
    {
        if (TimerSelector(which) != 0) return -1;
        if (value == null) return SignalPolicyError(14);
        itimerval requested = *value; // Preserve input when output aliases it.
        if (requested.it_value.tv_sec < 0 || requested.it_interval.tv_sec < 0 ||
            (ulong)requested.it_value.tv_usec >= 1000000 || (ulong)requested.it_interval.tv_usec >= 1000000)
            return SignalPolicyError(22);
        if (requested.it_value.tv_sec != 0 || requested.it_value.tv_usec != 0 ||
            requested.it_interval.tv_sec != 0 || requested.it_interval.tv_usec != 0)
            return SignalPolicyError(95);
        if (previous != null) *previous = default;
        return 0;
    }
    public static uint blink_host_alarm(uint seconds)
    {
        if (SignalPolicyOwner() != 0) return uint.MaxValue;
        if (seconds == 0) return 0;
        SignalPolicyError(95);
        return uint.MaxValue; // SysAlarm's signed result exposes the profile error.
    }
    public static int blink_host_kill(int process, int signal)
    {
        if (SignalPolicyOwner() != 0) return -1;
        if ((uint)signal > 64) return SignalPolicyError(22);
        if (process != 0 && process != identity!.ProcessId && process != -identity.ProcessId)
            return SignalPolicyError(3);
        // Signal zero really queries this private process/group namespace.
        return signal == 0 ? 0 : SignalPolicyError(95);
    }
    public static int blink_host_pause()
        => SignalPolicyOwner() != 0 ? -1 : SignalPolicyError(95);
    public static unsafe int blink_host_sigsuspend(blink_host_sigset* mask)
    {
        if (SignalPolicyOwner() != 0) return -1;
        return SignalPolicyError(mask == null ? 14 : 95);
    }
}

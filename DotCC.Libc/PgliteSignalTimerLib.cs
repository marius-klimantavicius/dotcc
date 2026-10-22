#nullable enable
using System;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // DotCC's guest LP64 records, NOT the host libc sigaction ABI. Keep in
    // agreement with include/signal.h and include/sys/time.h.
    [StructLayout(LayoutKind.Sequential)]
    public struct PgliteSignalAction
    {
        public nint Handler, Sigaction;
        public ulong Mask;
        public int Flags;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct PgliteIntervalTimer
    {
        public long IntervalSeconds, IntervalMicroseconds, ValueSeconds, ValueMicroseconds;
    }

    internal sealed class PgliteSignalState
    {
        internal readonly object Gate = new();
        internal readonly int Thread = Environment.CurrentManagedThreadId;
        internal readonly PgliteSignalAction[] Actions = new PgliteSignalAction[64];
        internal ulong Mask, Pending;
        internal long Due, Interval;
        internal bool Stopped, Delivering;
    }

    public sealed partial class RuntimeContext
    {
        internal PgliteSignalState? PgliteSignals;

        /// <summary>Opt into single-thread cooperative guest signals. No OS
        /// signal disposition is changed. The host must pump at safe points;
        /// this cannot interrupt a blocking or long-running translated query.</summary>
        public void EnablePgliteSignals()
        {
            if (Current != this || Termination != null)
                throw new InvalidOperationException("A live bound owner is required.");
            if (PgliteSignals == null) PgliteSignals = new PgliteSignalState();
            if (PgliteSignals.Thread != Environment.CurrentManagedThreadId || PgliteSignals.Stopped)
                throw new InvalidOperationException("PGlite signals have one execution thread and lifetime.");
        }

        public void PumpPgliteSignals()
        {
            if (Current != this || !TryPgliteSignals(out var state))
                throw new InvalidOperationException("PGlite signal delivery requires its live owning thread.");
            PumpPgliteSignalsCore(this, state);
        }

        internal void StopPgliteSignals()
        {
            var state = PgliteSignals;
            if (state == null) return;
            lock (state.Gate)
            {
                state.Stopped = true; state.Due = state.Interval = 0;
                state.Mask = state.Pending = 0;
                Array.Clear(state.Actions);
            }
        }
    }

    private static bool TryPgliteSignals(out PgliteSignalState state)
    {
        state = RuntimeContext.Current?.PgliteSignals!;
        if (state == null) { errno = ENOTSUP; return false; }
        if (state.Stopped || state.Thread != Environment.CurrentManagedThreadId)
        { errno = EINVAL; return false; }
        return true;
    }

    private const ulong UnmaskablePgliteSignals = (1UL << 8) | (1UL << 18);
    private static bool ValidPgliteSignal(int sig) => sig >= 1 && sig <= 64 && sig != 9 && sig != 19;

    /// <summary>Lexical sigsetjmp activation state. Keep this in the emitted
    /// activation, never inside C's numeric jmp_buf or a process-wide table.</summary>
    public readonly struct GuestSignalMaskSnapshot
    {
        internal readonly RuntimeContext? Owner;
        internal readonly ulong Mask;
        internal readonly int Thread;
        internal GuestSignalMaskSnapshot(RuntimeContext owner, ulong mask)
        { Owner = owner; Mask = mask; Thread = Environment.CurrentManagedThreadId; }
    }

    /// <summary>Runtime half of signal-aware lexical jump lowering.
    /// saveMask=0 intentionally saves nothing. A nonzero request requires the
    /// implemented guest-signal capability; it cannot silently ignore a mask.</summary>
    public static GuestSignalMaskSnapshot CaptureGuestSignalMask(int saveMask)
    {
        if (saveMask == 0) return default;
        if (!TryPgliteSignals(out var state))
            throw new NotSupportedException("Signal-mask capture requires an opted-in live owner thread.");
        lock (state.Gate)
        {
            if (state.Stopped) throw new InvalidOperationException("The signal owner terminated.");
            return new GuestSignalMaskSnapshot(RuntimeContext.Current!, state.Mask);
        }
    }

    /// <summary>Call inside the matching emitted jump catch, after unwinding.
    /// Pending delivery stays deferred until the next explicit safe point.</summary>
    public static void RestoreGuestSignalMask(GuestSignalMaskSnapshot snapshot)
    {
        if (snapshot.Owner == null) return;
        if (RuntimeContext.Current != snapshot.Owner || snapshot.Thread != Environment.CurrentManagedThreadId ||
            !TryPgliteSignals(out var state))
            throw new InvalidOperationException("A saved signal mask belongs to its capturing owner thread.");
        lock (state.Gate)
        {
            if (state.Stopped) throw new InvalidOperationException("The signal owner terminated.");
            state.Mask = snapshot.Mask;
        }
    }

    /// <summary>A single lexical signal-jump region. No CLR reference is stored
    /// in guest memory, and no global jump registry retains an expired frame.</summary>
    public sealed class SignalJumpActivation : IDisposable
    {
        private readonly GuestSignalMaskSnapshot mask;
        private readonly RuntimeContext? owner;
        private readonly int thread;
        private bool active = true;
        public ulong Identity { get; }

        internal SignalJumpActivation(ulong* buffer, int saveMask)
        {
            owner = RuntimeContext.Current;
            thread = Environment.CurrentManagedThreadId;
            if (owner?.Termination != null)
                throw new InvalidOperationException("A terminated owner cannot establish a signal jump.");
            mask = CaptureGuestSignalMask(saveMask);
            Identity = ArmJumpBuffer(buffer);
        }

        /// <summary>Called after unwind and before any recovery statement.
        /// Check the throwing owner as well as the current binding: a foreign
        /// binding may already have unwound before this catch is reached.</summary>
        public void Restore(JumpBufferException jump)
        {
            if (!active || jump.Identity != Identity ||
                thread != Environment.CurrentManagedThreadId || thread != jump.OriginThread ||
                owner != RuntimeContext.Current || owner != jump.OriginOwner || owner?.Termination != null)
                throw new InvalidOperationException("A signal jump requires its live lexical owner thread.");
            RestoreGuestSignalMask(mask);
        }

        public void Dispose() => active = false;
    }

    public static SignalJumpActivation ArmSignalJumpBuffer(ulong* buffer, int saveMask)
        => new(buffer, saveMask);

    private static int PgliteSigaction(int sig, void* action, void* previous)
    {
        if (!ValidPgliteSignal(sig)) { errno = EINVAL; return -1; }
        if (!TryPgliteSignals(out var state)) return -1;
        var requested = action == null ? default : *(PgliteSignalAction*)action;
        // SA_RESTART is meaningful only for actual interruptible syscalls. The
        // cooperative service has none. SA_SIGINFO has a different callback ABI.
        const uint supported = 0x10000000U | 0x80000000U;
        if (action != null && ((unchecked((uint)requested.Flags) & ~supported) != 0 || requested.Handler == -1))
        { errno = ENOTSUP; return -1; }
        lock (state.Gate)
        {
            if (state.Stopped) { errno = EINVAL; return -1; }
            if (previous != null) *(PgliteSignalAction*)previous = state.Actions[sig - 1];
            if (action != null)
            {
                requested.Mask &= ~UnmaskablePgliteSignals;
                state.Actions[sig - 1] = requested;
                if (requested.Handler == 1) state.Pending &= ~(1UL << (sig - 1));
            }
        }
        return 0;
    }

    private static int PgliteSigprocmask(int how, ulong* set, ulong* previous)
    {
        if (!TryPgliteSignals(out var state)) return -1;
        ulong requested = set == null ? 0 : *set;
        if (set != null && (how < 0 || how > 2)) { errno = EINVAL; return -1; }
        lock (state.Gate)
        {
            if (state.Stopped) { errno = EINVAL; return -1; }
            if (previous != null) *previous = state.Mask;
            if (set != null)
                state.Mask = (how == 0 ? state.Mask | requested : how == 1 ? state.Mask & ~requested : requested) & ~UnmaskablePgliteSignals;
        }
        if (set != null) PumpPgliteSignalsCore(RuntimeContext.Current!, state);
        return 0;
    }

    private static int QueuePgliteSignal(int sig)
    {
        if (!ValidPgliteSignal(sig)) { errno = EINVAL; return -1; }
        if (!TryPgliteSignals(out var state)) return -1;
        lock (state.Gate)
        {
            if (state.Stopped) { errno = EINVAL; return -1; }
            if (state.Actions[sig - 1].Handler == 0 && (sig == 20 || sig == 21 || sig == 22))
            { errno = ENOTSUP; return -1; }
            if (state.Actions[sig - 1].Handler != 1) state.Pending |= 1UL << (sig - 1);
        }
        PumpPgliteSignalsCore(RuntimeContext.Current!, state);
        return 0;
    }

    /// <summary>Report blocked guest signals waiting for delivery, observing
    /// the owned real-time timer without consuming signals or running callbacks.</summary>
    public static int sigpending(ulong* pending)
    {
        if (!TryPgliteSignals(out var state)) return -1;
        if (pending == null) { errno = EFAULT; return -1; }
        lock (state.Gate)
        {
            if (state.Stopped) { errno = EINVAL; return -1; }
            ObservePgliteTimer(state, global::System.Diagnostics.Stopwatch.GetTimestamp());
            *pending = state.Pending & state.Mask;
        }
        return 0;
    }

    /// <summary>Consume one selected blocked guest signal without invoking its
    /// handler. An owned ITIMER_REAL can supply a future SIGALRM. This serialized
    /// service has no asynchronous foreign-thread sender: without a pending
    /// match or selected active timer, waiting returns ENOTSUP. Errors are
    /// positive return values and never replace errno.</summary>
    public static int sigwait(ulong* set, int* signal)
    {
        int savedError = errno;
        try
        {
            if (!TryPgliteSignals(out var state)) return errno;
            if (set == null || signal == null) return EFAULT;
            ulong requested = *set;
            if (requested == 0 || (requested & UnmaskablePgliteSignals) != 0) return EINVAL;
            const ulong alarm = 1UL << 13;
            while (true)
            {
                long remaining;
                lock (state.Gate)
                {
                    if (state.Stopped) return EINVAL;
                    if ((requested & ~state.Mask) != 0) return EINVAL;
                    long now = global::System.Diagnostics.Stopwatch.GetTimestamp();
                    ObservePgliteTimer(state, now);
                    ulong ready = state.Pending & requested;
                    if (ready != 0)
                    {
                        int selected = global::System.Numerics.BitOperations.TrailingZeroCount(ready);
                        state.Pending &= ~(1UL << selected);
                        *signal = selected + 1;
                        return 0;
                    }
                    if ((requested & alarm) == 0 || state.Due == 0 || state.Actions[13].Handler == 1)
                        return ENOTSUP;
                    remaining = state.Due - now;
                }
                // Do not hold the signal gate while waiting. Terminal owner
                // cleanup may cancel the timer and is observed on the next pass.
                int milliseconds = (int)Math.Clamp(decimal.Ceiling(
                    (decimal)remaining * 1000 / global::System.Diagnostics.Stopwatch.Frequency), 1, 20);
                global::System.Threading.Thread.Sleep(milliseconds);
            }
        }
        finally { errno = savedError; }
    }

    private static void ObservePgliteTimer(PgliteSignalState state, long now)
    {
        if (state.Due == 0 || now < state.Due) return;
        if (state.Actions[13].Handler != 1) state.Pending |= 1UL << 13;
        if (state.Interval == 0) state.Due = 0;
        else
        {
            long remainder = (now - state.Due) % state.Interval;
            state.Due = now > long.MaxValue - state.Interval ? long.MaxValue : now + state.Interval - remainder;
        }
    }

    private static void PumpPgliteSignalsCore(RuntimeContext owner, PgliteSignalState state)
    {
        if (state.Delivering) return;
        state.Delivering = true;
        int savedError = errno;
        try
        {
            while (true)
            {
                PgliteSignalAction action;
                ulong savedMask;
                int sig;
                lock (state.Gate)
                {
                    if (state.Stopped) return;
                    ObservePgliteTimer(state, global::System.Diagnostics.Stopwatch.GetTimestamp());
                    ulong ready = state.Pending & ~state.Mask;
                    if (ready == 0) return;
                    sig = global::System.Numerics.BitOperations.TrailingZeroCount(ready) + 1;
                    state.Pending &= ~(1UL << (sig - 1));
                    action = state.Actions[sig - 1];
                    savedMask = state.Mask;
                    state.Mask |= action.Mask;
                    state.Mask |= 1UL << (sig - 1);
                    if ((unchecked((uint)action.Flags) & 0x80000000U) != 0) state.Actions[sig - 1] = default;
                }
                bool restoreMask = true;
                try
                {
                    if (action.Handler == 1) continue;
                    if (action.Handler == 0)
                    {
                        // Linux default-ignore dispositions used by PostgreSQL.
                        if (sig == 17 || sig == 18 || sig == 23 || sig == 28) continue;
                        // Owner-local terminal delivery; never signal the CLR host.
                        throw new RuntimeTerminationException(owner, owner.RecordTermination(
                            sig == 6 ? RuntimeTerminationKind.Abort : RuntimeTerminationKind.Exit, sig == 6 ? 0 : 128 + sig));
                    }
                    ((delegate*<int, void>)action.Handler)(sig);
                }
                catch (JumpBufferException)
                {
                    // Nonlocal return skips POSIX handler-return restoration.
                    // A saving sigsetjmp restores its snapshot at its own catch;
                    // saveMask=0 and ordinary setjmp retain the handler's mask.
                    restoreMask = false;
                    throw;
                }
                finally { lock (state.Gate) { if (restoreMask && !state.Stopped) state.Mask = savedMask; } }
            }
        }
        finally { state.Delivering = false; errno = savedError; }
    }

    private static bool PgliteTimerTicks(long seconds, long microseconds, out long ticks)
    {
        ticks = 0;
        if (seconds < 0 || microseconds < 0 || microseconds >= 1000000) return false;
        try
        {
            ticks = checked(seconds * global::System.Diagnostics.Stopwatch.Frequency +
                (long)decimal.Ceiling((decimal)microseconds * global::System.Diagnostics.Stopwatch.Frequency / 1000000));
            return true;
        }
        catch (OverflowException) { return false; }
    }

    /// <summary>Backing for setitimer(ITIMER_REAL), using the guest LP64 ABI.
    /// Delivery occurs on the owning thread's next explicit signal pump.</summary>
    public static int PgliteSetitimer(int which, void* value, void* previous)
    {
        if (which != 0) { errno = ENOTSUP; return -1; }
        if (value == null) { errno = EFAULT; return -1; }
        if (!TryPgliteSignals(out var state)) return -1;
        var requested = *(PgliteIntervalTimer*)value;
        if (!PgliteTimerTicks(requested.IntervalSeconds, requested.IntervalMicroseconds, out long interval) ||
            !PgliteTimerTicks(requested.ValueSeconds, requested.ValueMicroseconds, out long delay))
        { errno = EINVAL; return -1; }
        lock (state.Gate)
        {
            if (state.Stopped) { errno = EINVAL; return -1; }
            long now = global::System.Diagnostics.Stopwatch.GetTimestamp();
            if (delay > long.MaxValue - now) { errno = EINVAL; return -1; }
            ObservePgliteTimer(state, now);
            if (previous != null)
            {
                long remaining = state.Due == 0 ? 0 : Math.Max(0, state.Due - now);
                *(PgliteIntervalTimer*)previous = PgliteTimerValue(state.Interval, remaining);
            }
            state.Interval = interval;
            state.Due = delay == 0 ? 0 : now + delay;
        }
        return 0;
    }

    private static uint PgliteAlarm(uint seconds)
    {
        PgliteIntervalTimer requested = new() { ValueSeconds = seconds }, previous = default;
        if (PgliteSetitimer(0, &requested, &previous) != 0) return uint.MaxValue;
        long secondsLeft = previous.ValueSeconds + (previous.ValueMicroseconds == 0 ? 0 : 1);
        return (uint)Math.Min(secondsLeft, uint.MaxValue);
    }

    public static int getitimer(int which, void* value)
    {
        if (which != 0) { errno = ENOTSUP; return -1; }
        if (value == null) { errno = EFAULT; return -1; }
        if (!TryPgliteSignals(out var state)) return -1;
        lock (state.Gate)
        {
            if (state.Stopped) { errno = EINVAL; return -1; }
            long now = global::System.Diagnostics.Stopwatch.GetTimestamp();
            ObservePgliteTimer(state, now);
            long remaining = state.Due == 0 ? 0 : Math.Max(0, state.Due - now);
            *(PgliteIntervalTimer*)value = PgliteTimerValue(state.Interval, remaining);
        }
        return 0;
    }

    private static PgliteIntervalTimer PgliteTimerValue(long interval, long remaining)
    {
        long micros = (long)decimal.Ceiling((decimal)remaining * 1000000 / global::System.Diagnostics.Stopwatch.Frequency);
        long intervalMicros = (long)((decimal)interval * 1000000 / global::System.Diagnostics.Stopwatch.Frequency);
        return new PgliteIntervalTimer
        {
            IntervalSeconds = intervalMicros / 1000000,
            IntervalMicroseconds = intervalMicros % 1000000,
            ValueSeconds = micros / 1000000,
            ValueMicroseconds = micros % 1000000,
        };
    }

    public static int pthread_sigmask(int how, ulong* set, ulong* previous) => PgliteSigprocmask(how, set, previous);
}

using System;
using System.Threading;
using Xunit;
using RuntimeLibc = global::DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PgliteLifecycleTests
{
    private static RuntimeLibc.RuntimeContext? expectedOwner;
    private static int delivered, nested;
    private static ulong seenMask;
    private static void Handler(int sig)
    {
        Assert.Same(expectedOwner, RuntimeLibc.RuntimeContext.Current);
        delivered++;
        ulong mask = 0;
        Assert.Equal(0, RuntimeLibc.sigprocmask(2, null, &mask));
        seenMask = mask;
        if (nested++ == 0) Assert.Equal(0, RuntimeLibc.raise(sig));
    }
    private static void SimpleHandler(int sig)
    {
        Assert.Equal(14, sig);
        Assert.Same(expectedOwner, RuntimeLibc.RuntimeContext.Current);
        delivered++;
    }
    private static void YieldingHandler(int sig)
    {
        RuntimeLibc.errno = 999;
        RuntimeLibc.exit(100);
    }

    [Fact]
    public void Repeated_yields_preserve_owner_and_true_exit_is_terminal()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var other = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            for (int i = 0; i < 8; i++)
            {
                using var scope = owner.InterceptPgliteExits();
                int status = i % 2 == 0 ? 99 : 100;
                var error = Assert.Throws<RuntimeLibc.PgliteYieldException>(() => RuntimeLibc.exit(status));
                Assert.Same(owner, error.Owner);
                Assert.Equal(status, error.Status);
                Assert.Null(owner.Termination);
                Assert.False(owner.TerminationTask.IsCompleted);
            }
            using (owner.InterceptPgliteExits())
            {
                using (other.Enter())
                {
                    var error = Assert.Throws<RuntimeLibc.RuntimeTerminationException>(() => RuntimeLibc.exit(99));
                    Assert.Same(other, error.Owner);
                }
                Assert.Null(owner.Termination);
                Assert.Equal(23, Assert.Throws<RuntimeLibc.RuntimeTerminationException>(() => RuntimeLibc.exit(23)).Termination.Status);
                Assert.Throws<InvalidOperationException>(() => owner.InterceptPgliteExits());
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Scope_does_not_intercept_immediate_exit_or_unscoped_99(bool immediate)
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var binding = owner.Enter();
        if (immediate)
        {
            using var scope = owner.InterceptPgliteExits();
            Assert.Equal(RuntimeLibc.RuntimeTerminationKind.ImmediateExit,
                Assert.Throws<RuntimeLibc.RuntimeTerminationException>(() => RuntimeLibc._Exit(99)).Termination.Kind);
        }
        else Assert.Equal(99, Assert.Throws<RuntimeLibc.RuntimeTerminationException>(() => RuntimeLibc.exit(99)).Termination.Status);
    }

    [Fact]
    public void Scopes_enforce_order_and_binding_identity()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var binding = owner.Enter();
        var outer = owner.InterceptPgliteExits();
        var inner = owner.InterceptPgliteExits();
        Assert.Throws<InvalidOperationException>(outer.Dispose);
        inner.Dispose();
        using (owner.Enter())
            Assert.Throws<InvalidOperationException>(outer.Dispose);
        outer.Dispose(); outer.Dispose();
    }

    [Fact]
    public void Sigaction_roundtrips_oldact_and_masks_defer_and_restore_delivery()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var binding = owner.Enter();
        owner.EnablePgliteSignals(); expectedOwner = owner; delivered = nested = 0;
        RuntimeLibc.PgliteSignalAction action = new() { Handler = (nint)(delegate*<int, void>)&Handler, Mask = 1UL << 9 };
        RuntimeLibc.PgliteSignalAction old = new() { Handler = 42, Sigaction = 42, Mask = 42, Flags = 42 };
        Assert.Equal(0, RuntimeLibc.sigaction(14, &action, &old)); Assert.Equal(0, old.Handler); Assert.Equal(0UL, old.Mask);
        Assert.Equal(0, RuntimeLibc.sigaction(14, null, &old)); Assert.Equal(action.Handler, old.Handler); Assert.Equal(action.Mask, old.Mask);
        ulong block = 1UL << 13, previous = 99;
        Assert.Equal(0, RuntimeLibc.sigprocmask(0, &block, &previous)); Assert.Equal(0UL, previous);
        Assert.Equal(0, RuntimeLibc.raise(14)); Assert.Equal(0, RuntimeLibc.raise(14)); Assert.Equal(0, delivered);
        Assert.Equal(0, RuntimeLibc.sigprocmask(1, &block, &previous));
        Assert.Equal(block, previous); Assert.Equal(2, delivered);
        Assert.Equal(block | action.Mask, seenMask);
        Assert.Equal(0, RuntimeLibc.sigprocmask(2, null, &previous)); Assert.Equal(0UL, previous);
        action.Handler = 1; Assert.Equal(0, RuntimeLibc.sigaction(14, &action, null));
        Assert.Equal(0, RuntimeLibc.raise(14)); Assert.Equal(2, delivered);
        action.Flags = 4; Assert.Equal(-1, RuntimeLibc.sigaction(14, &action, null)); Assert.Equal(RuntimeLibc.ENOTSUP, RuntimeLibc.errno);
        Assert.Equal(-1, RuntimeLibc.sigaction(9, null, &old)); Assert.Equal(RuntimeLibc.EINVAL, RuntimeLibc.errno);
    }

    [Fact]
    public void Timer_delivers_only_on_owner_pump_and_cancel_and_terminal_cleanup_work()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var other = new RuntimeLibc.RuntimeContext();
        using var binding = owner.Enter();
        owner.EnablePgliteSignals(); expectedOwner = owner; delivered = 0;
        RuntimeLibc.PgliteSignalAction action = new() { Handler = (nint)(delegate*<int, void>)&SimpleHandler };
        Assert.Equal(0, RuntimeLibc.sigaction(14, &action, null));
        RuntimeLibc.PgliteIntervalTimer timer = new() { ValueMicroseconds = 1000 }, zero = default, old = default;
        Assert.Equal(0, RuntimeLibc.PgliteSetitimer(0, &timer, null));
        Thread.Sleep(15); Assert.Equal(0, delivered);
        using (other.Enter())
        {
            other.EnablePgliteSignals(); other.PumpPgliteSignals();
            Assert.Equal(0, delivered);
        }
        owner.PumpPgliteSignals(); Assert.Equal(1, delivered);
        timer.ValueSeconds = 1;
        Assert.Equal(0, RuntimeLibc.PgliteSetitimer(0, &timer, null));
        Assert.Equal(0, RuntimeLibc.PgliteSetitimer(0, &zero, &old));
        Assert.True(old.ValueSeconds > 0 || old.ValueMicroseconds > 0);
        Thread.Sleep(15); owner.PumpPgliteSignals(); Assert.Equal(1, delivered);
        timer.ValueSeconds = 0; timer.IntervalMicroseconds = 1000;
        Assert.Equal(0, RuntimeLibc.PgliteSetitimer(0, &timer, null));
        Thread.Sleep(15); owner.PumpPgliteSignals(); Assert.True(delivered >= 2);
        Assert.Throws<RuntimeLibc.RuntimeTerminationException>(() => RuntimeLibc.exit(0));
        Assert.True(owner.PgliteSignals!.Stopped);
        Assert.Equal(0L, owner.PgliteSignals.Due); Assert.Equal(0UL, owner.PgliteSignals.Pending);
        Assert.Equal(0, owner.PgliteSignals.Actions[13].Handler);
        Assert.Throws<InvalidOperationException>(owner.PumpPgliteSignals);
    }

    [Fact]
    public void Timer_rejects_invalid_requests_without_replacing_existing_timer()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var binding = owner.Enter(); owner.EnablePgliteSignals();
        RuntimeLibc.PgliteIntervalTimer timer = new() { ValueSeconds = 60 };
        Assert.Equal(0, RuntimeLibc.PgliteSetitimer(0, &timer, null));
        long due = owner.PgliteSignals!.Due;
        timer.ValueMicroseconds = 1000000;
        Assert.Equal(-1, RuntimeLibc.PgliteSetitimer(0, &timer, null)); Assert.Equal(RuntimeLibc.EINVAL, RuntimeLibc.errno);
        Assert.Equal(due, owner.PgliteSignals.Due);
        Assert.Equal(-1, RuntimeLibc.PgliteSetitimer(1, &timer, null)); Assert.Equal(RuntimeLibc.ENOTSUP, RuntimeLibc.errno);
        Exception? wrongThread = null;
        var thread = new Thread(() => { using var scope = owner.Enter(); wrongThread = Record.Exception(owner.PumpPgliteSignals); });
        thread.Start(); thread.Join(); Assert.IsType<InvalidOperationException>(wrongThread);
    }

    [Fact]
    public void Timer_query_and_aliasing_preserve_interval_and_mask_query_works()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var binding = owner.Enter(); owner.EnablePgliteSignals();
        RuntimeLibc.PgliteIntervalTimer value = new() { ValueSeconds = 60, IntervalSeconds = 2, IntervalMicroseconds = 3 };
        Assert.Equal(0, RuntimeLibc.PgliteSetitimer(0, &value, null));
        Assert.Equal(0, RuntimeLibc.getitimer(0, &value));
        Assert.Equal(2, value.IntervalSeconds); Assert.Equal(3, value.IntervalMicroseconds);
        Assert.InRange(value.ValueSeconds, 59, 60); Assert.InRange(value.ValueMicroseconds, 0, 999999);
        value = default; Assert.Equal(0, RuntimeLibc.PgliteSetitimer(0, &value, &value));
        Assert.Equal(2, value.IntervalSeconds);
        Assert.Equal(0, RuntimeLibc.getitimer(0, &value)); Assert.Equal(0, value.ValueSeconds);
        ulong mask = ulong.MaxValue;
        Assert.Equal(0, RuntimeLibc.pthread_sigmask(2, &mask, &mask)); Assert.Equal(0UL, mask);
        Assert.Equal(0, RuntimeLibc.pthread_sigmask(2, null, &mask));
        Assert.Equal(0UL, mask & ((1UL << 8) | (1UL << 18)));
    }

    [Fact]
    public void Handler_unwind_restores_owner_mask_and_errno_without_terminating()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var binding = owner.Enter(); owner.EnablePgliteSignals();
        RuntimeLibc.PgliteSignalAction action = new() { Handler = (nint)(delegate*<int, void>)&YieldingHandler, Mask = 1UL << 9 };
        Assert.Equal(0, RuntimeLibc.sigaction(14, &action, null));
        RuntimeLibc.errno = 73;
        using (owner.InterceptPgliteExits())
            Assert.Equal(100, Assert.Throws<RuntimeLibc.PgliteYieldException>(() => RuntimeLibc.raise(14)).Status);
        Assert.Null(owner.Termination); Assert.Equal(73, RuntimeLibc.errno);
        ulong mask = ulong.MaxValue;
        Assert.Equal(0, RuntimeLibc.sigprocmask(2, null, &mask)); Assert.Equal(0UL, mask);
    }

    [Fact]
    public void Reset_hand_and_default_delivery_keep_terminal_status_owner_local()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var other = new RuntimeLibc.RuntimeContext();
        using var binding = owner.Enter(); owner.EnablePgliteSignals(); expectedOwner = owner; delivered = 0;
        RuntimeLibc.PgliteSignalAction action = new()
        { Handler = (nint)(delegate*<int, void>)&SimpleHandler, Flags = unchecked((int)0x80000000U) };
        Assert.Equal(0, RuntimeLibc.sigaction(14, &action, null));
        Assert.Equal(0, RuntimeLibc.kill(RuntimeLibc.getpid(), 14)); Assert.Equal(1, delivered);
        Assert.Equal(-1, RuntimeLibc.kill(int.MaxValue, 14)); Assert.Equal(RuntimeLibc.EPERM, RuntimeLibc.errno);
        Assert.Equal(142, Assert.Throws<RuntimeLibc.RuntimeTerminationException>(() => RuntimeLibc.raise(14)).Termination.Status);
        Assert.Null(other.Termination);
    }

    [Fact]
    public void Missing_signal_capability_fails_and_disposal_clears_handlers_and_timers()
    {
        var owner = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            Assert.Equal(-1, RuntimeLibc.sigaction(14, null, null)); Assert.Equal(RuntimeLibc.ENOTSUP, RuntimeLibc.errno);
            owner.EnablePgliteSignals();
            RuntimeLibc.PgliteIntervalTimer timer = new() { ValueSeconds = 60 };
            Assert.Equal(0, RuntimeLibc.PgliteSetitimer(0, &timer, null));
            byte* localeName = stackalloc byte[] { (byte)'C', 0 };
            Assert.NotEqual((nint)0, (nint)RuntimeLibc.newlocale(1, localeName, null));
            Assert.Single(owner.PosixLocales);
            Assert.Equal(-1, RuntimeLibc.sigemptyset(null));
            Assert.Equal(-1, RuntimeLibc.sigfillset(null));
        }
        owner.Dispose(); Assert.True(owner.PgliteSignals!.Stopped); Assert.Equal(0L, owner.PgliteSignals.Due);
        Assert.Empty(owner.PosixLocales);
    }

    [Fact]
    public void Lexical_mask_snapshot_restores_without_delivering_until_safe_point()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var other = new RuntimeLibc.RuntimeContext();
        using var binding = owner.Enter(); owner.EnablePgliteSignals(); expectedOwner = owner; delivered = 0;
        RuntimeLibc.PgliteSignalAction action = new() { Handler = (nint)(delegate*<int, void>)&SimpleHandler };
        Assert.Equal(0, RuntimeLibc.sigaction(14, &action, null));
        var saved = RuntimeLibc.CaptureGuestSignalMask(1);
        var unsaved = RuntimeLibc.CaptureGuestSignalMask(0);
        ulong mask = 1UL << 13, observed = 0;
        Assert.Equal(0, RuntimeLibc.sigprocmask(0, &mask, null));
        Assert.Equal(0, RuntimeLibc.raise(14)); Assert.Equal(0, delivered);
        RuntimeLibc.RestoreGuestSignalMask(unsaved);
        Assert.Equal(0, RuntimeLibc.sigprocmask(2, null, &observed)); Assert.Equal(mask, observed);
        using (other.Enter())
        {
            other.EnablePgliteSignals();
            Assert.Throws<InvalidOperationException>(() => RuntimeLibc.RestoreGuestSignalMask(saved));
        }
        RuntimeLibc.RestoreGuestSignalMask(saved);
        Assert.Equal(0, delivered);
        Assert.Equal(0, RuntimeLibc.sigprocmask(2, null, &observed)); Assert.Equal(0UL, observed);
        owner.PumpPgliteSignals(); Assert.Equal(1, delivered);
    }
}

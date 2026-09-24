using System;
using System.Diagnostics;
using System.Threading;
using RuntimeLibc = DotCC.Libc.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PosixClockTests
{
    [Fact]
    public void Realtime_matches_UTC_and_monotonic_never_moves_backwards()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var binding = owner.Enter();
        RuntimeLibc.timespec value;
        long before = DateTimeOffset.UtcNow.UtcTicks;
        RuntimeLibc.errno = 123;
        RuntimeLibc.clock_gettime(RuntimeLibc.CLOCK_REALTIME, &value).ShouldBe(0);
        long after = DateTimeOffset.UtcNow.UtcTicks;
        value.tv_nsec.ShouldBeInRange(0L, 999_999_999L);
        (DateTimeOffset.UnixEpoch.UtcTicks + value.tv_sec * TimeSpan.TicksPerSecond + value.tv_nsec / 100)
            .ShouldBeInRange(before, after);
        long previousSeconds = 0, previousNanoseconds = 0;
        for (int i = 0; i < 1000; i++)
        {
            RuntimeLibc.clock_gettime(RuntimeLibc.CLOCK_MONOTONIC, &value).ShouldBe(0);
            value.tv_nsec.ShouldBeInRange(0L, 999_999_999L);
            (value.tv_sec > previousSeconds || value.tv_sec == previousSeconds && value.tv_nsec >= previousNanoseconds).ShouldBeTrue();
            previousSeconds = value.tv_sec;
            previousNanoseconds = value.tv_nsec;
        }
        RuntimeLibc.errno.ShouldBe(123);
    }

    [Fact]
    public void Invalid_clocks_and_null_outputs_use_owner_errno_without_writing_storage()
    {
        using var first = new RuntimeLibc.RuntimeContext();
        using var second = new RuntimeLibc.RuntimeContext();
        using var binding = first.Enter();
        RuntimeLibc.errno = 321;
        using (second.Enter())
        {
            RuntimeLibc.timespec value = new() { tv_sec = 41, tv_nsec = 42 };
            RuntimeLibc.clock_gettime(-1, &value).ShouldBe(-1);
            RuntimeLibc.errno.ShouldBe(RuntimeLibc.EINVAL);
            value.tv_sec.ShouldBe(41);
            value.tv_nsec.ShouldBe(42);
            RuntimeLibc.clock_gettime(RuntimeLibc.CLOCK_MONOTONIC, null).ShouldBe(-1);
            RuntimeLibc.errno.ShouldBe(RuntimeLibc.EFAULT);
        }
        RuntimeLibc.errno.ShouldBe(321);
    }

    [Theory]
    [InlineData(-1L, 0L)]
    [InlineData(0L, -1L)]
    [InlineData(0L, 1_000_000_000L)]
    [InlineData(long.MaxValue, 1_000_000_000L)]
    public void Invalid_sleep_duration_preserves_remaining(long seconds, long nanoseconds)
    {
        RuntimeLibc.timespec request = new() { tv_sec = seconds, tv_nsec = nanoseconds };
        RuntimeLibc.timespec remaining = new() { tv_sec = 7, tv_nsec = 8 };
        RuntimeLibc.nanosleep(&request, &remaining).ShouldBe(-1);
        RuntimeLibc.errno.ShouldBe(RuntimeLibc.EINVAL);
        remaining.tv_sec.ShouldBe(7);
        remaining.tv_nsec.ShouldBe(8);
    }

    [Fact]
    public void Sleep_covers_full_requested_interval_and_success_leaves_remaining_untouched()
    {
        RuntimeLibc.timespec request = new() { tv_nsec = 25_123_456 };
        RuntimeLibc.timespec remaining = new() { tv_sec = 7, tv_nsec = 8 };
        var elapsed = Stopwatch.StartNew();
        RuntimeLibc.nanosleep(&request, &remaining).ShouldBe(0);
        elapsed.Elapsed.TotalMilliseconds.ShouldBeGreaterThanOrEqualTo(25.123456);
        remaining.tv_sec.ShouldBe(7);
        remaining.tv_nsec.ShouldBe(8);
        request = default;
        RuntimeLibc.nanosleep(&request, null).ShouldBe(0);
        RuntimeLibc.nanosleep(null, null).ShouldBe(-1);
        RuntimeLibc.errno.ShouldBe(RuntimeLibc.EFAULT);
    }

    [Theory]
    [InlineData(5L)]
    [InlineData(long.MaxValue)]
    public void Interrupted_sleep_reports_unslept_interval_without_overflow(long seconds)
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var entered = new ManualResetEventSlim();
        int result = 0, error = 0;
        long leftSeconds = -1, leftNanoseconds = -1;
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var binding = owner.Enter();
                RuntimeLibc.timespec request = new() { tv_sec = seconds, tv_nsec = 999_999_999 }, remaining;
                entered.Set();
                result = RuntimeLibc.nanosleep(&request, &remaining);
                error = RuntimeLibc.errno;
                leftSeconds = remaining.tv_sec;
                leftNanoseconds = remaining.tv_nsec;
            }
            catch (Exception ex) { failure = ex; }
        }) { IsBackground = true };
        thread.Start();
        entered.Wait(TimeSpan.FromSeconds(10)).ShouldBeTrue();
        thread.Interrupt();
        thread.Join(TimeSpan.FromSeconds(10)).ShouldBeTrue();
        failure.ShouldBeNull();
        result.ShouldBe(-1);
        error.ShouldBe(RuntimeLibc.EINTR);
        leftSeconds.ShouldBeInRange(0L, seconds);
        leftNanoseconds.ShouldBeInRange(0L, 999_999_999L);
        (leftSeconds > 0 || leftNanoseconds > 0).ShouldBeTrue();
    }
}

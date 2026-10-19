using System;
using System.Diagnostics;
using System.Threading;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class ProcessTimesTests
{
    [Fact]
    public void Real_cpu_and_elapsed_ticks_share_sysconf_units_and_preserve_output_bounds()
    {
        using var context = new RuntimeContext();
        using var entry = context.Enter();
        long* before = stackalloc long[5];
        long* after = stackalloc long[5];
        before[4] = 31415;
        after[4] = 92653;
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8)
        {
            before[0] = 123;
            times(before).ShouldBe(-1);
            errno.ShouldBe(ENOTSUP);
            before[0].ShouldBe(123);
            sysconf(2).ShouldBe(-1);
            errno.ShouldBe(ENOTSUP);
            return;
        }
        long frequency = sysconf(2);
        frequency.ShouldBeGreaterThan(0);
        errno = EIO;
        long elapsed = times(before);
        elapsed.ShouldBeGreaterThan(0);
        errno.ShouldBe(EIO);
        for (int i = 0; i < 4; ++i) before[i].ShouldBeGreaterThanOrEqualTo(0);
        var deadline = Stopwatch.StartNew();
        do
        {
            Thread.SpinWait(100_000);
            times(after).ShouldBeGreaterThanOrEqualTo(elapsed);
            deadline.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(8));
        } while (after[0] + after[1] < before[0] + before[1] + 2);
        for (int i = 0; i < 4; ++i) after[i].ShouldBeGreaterThanOrEqualTo(before[i]);
        before[4].ShouldBe(31415);
        after[4].ShouldBe(92653);
        long start = times(null);
        Thread.Sleep(40);
        long delta = times(null) - start;
        delta.ShouldBeGreaterThanOrEqualTo(Math.Max(1, frequency / 100));
        // This bounds actual tick units against real elapsed wall time. Do not
        // infer CPU time from sleep or require nondeterministic exact counts.
        ((double)delta / frequency).ShouldBeLessThan(5);
        errno = 0;
        sysconf(-123).ShouldBe(-1);
        errno.ShouldBe(EINVAL);
    }

    [Fact]
    public void Cpu_of_a_real_waited_child_is_observable_without_product_spawn_support()
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8)
            Assert.Skip("Linux LP64 waited-child accounting control");
        long* before = stackalloc long[4];
        long* after = stackalloc long[4];
        times(before).ShouldBeGreaterThan(0);
        // Harness-only OS process. The translated CPython product still
        // excludes fork/exec/subprocess; no engine or service is delegated.
        var start = new ProcessStartInfo("/bin/sh") { UseShellExecute = false };
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add("i=0; while [ \"$i\" -lt 100000 ]; do i=$((i+1)); done");
        using var child = Process.Start(start)!;
        child.WaitForExit(10_000).ShouldBeTrue();
        child.ExitCode.ShouldBe(0);
        times(after).ShouldBeGreaterThan(0);
        (after[2] + after[3]).ShouldBeGreaterThan(before[2] + before[3]);
    }
}

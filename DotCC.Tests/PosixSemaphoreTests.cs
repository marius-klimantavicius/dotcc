using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PosixSemaphoreTests
{
    [Fact]
    public void Counting_errors_and_owner_separation_match_native_contract()
    {
        if (!OperatingSystem.IsLinux()) return;
        long* storage = stackalloc long[4];
        using var first = new RuntimeContext();
        using var second = new RuntimeContext();
        using (first.Enter())
        {
            sem_init(storage, 1, 2).ShouldBe(0);
            sem_init(storage, 1, 1).ShouldBe(-1); errno.ShouldBe(EBUSY);
            int value;
            sem_getvalue(storage, &value).ShouldBe(0); value.ShouldBe(2);
            sem_wait(storage).ShouldBe(0); sem_trywait(storage).ShouldBe(0);
            sem_trywait(storage).ShouldBe(-1); errno.ShouldBe(EAGAIN);
            using (second.Enter())
            { sem_post(storage).ShouldBe(-1); errno.ShouldBe(EINVAL); }
            sem_post(storage).ShouldBe(0);
            sem_getvalue(storage, &value).ShouldBe(0); value.ShouldBe(1);
            sem_destroy(storage).ShouldBe(0);
            sem_post(storage).ShouldBe(-1); errno.ShouldBe(EINVAL);
            sem_destroy(storage).ShouldBe(-1); errno.ShouldBe(EINVAL);
            sem_init(storage, 0, uint.MaxValue).ShouldBe(-1); errno.ShouldBe(EINVAL);
            sem_init(null, 0, 0).ShouldBe(-1); errno.ShouldBe(EFAULT);
        }
    }

    [Fact]
    public void Real_wait_releases_after_cross_thread_post()
    {
        if (!OperatingSystem.IsLinux()) return;
        using var owner = new RuntimeContext();
        nint address = (nint)NativeMemory.AllocZeroed(32);
        try
        {
            using (owner.Enter()) sem_init((void*)address, 1, 0).ShouldBe(0);
            using var entering = new ManualResetEventSlim();
            var waiter = Task.Run(() =>
            {
                using var binding = owner.Enter();
                entering.Set();
                return sem_wait((void*)address);
            });
            entering.Wait(TimeSpan.FromSeconds(5)).ShouldBeTrue();
            using (owner.Enter())
            {
                waiter.IsCompleted.ShouldBeFalse();
                sem_post((void*)address).ShouldBe(0);
            }
            waiter.Wait(TimeSpan.FromSeconds(5)).ShouldBeTrue(); waiter.Result.ShouldBe(0);
            using (owner.Enter()) sem_destroy((void*)address).ShouldBe(0);
        }
        finally { NativeMemory.Free((void*)address); }
    }
}

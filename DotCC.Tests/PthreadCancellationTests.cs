using System.Runtime.InteropServices;
using DotCC.Libc;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PthreadCancellationTests
{
    private sealed class Owner : IProgramInstance, IDisposable
    {
        public RuntimeContext __DotCcRuntime { get; } = new();
        public readonly ManualResetEventSlim Ready = new(), Resume = new(), Done = new();
        public readonly List<int> Cleanup = new();
        public int Mode, PassedDisabled, MutexReleased, Returned;
        public int* Mutex;
        public int* Condition;
        public void Dispose()
        {
            __DotCcRuntime.Dispose(); Ready.Dispose(); Resume.Dispose(); Done.Dispose();
        }
    }

    private static void Cleanup(Owner owner, void* argument)
    {
        ReferenceEquals(RuntimeContext.Current, owner.__DotCcRuntime).ShouldBeTrue();
        owner.Cleanup.Add((int)(nint)argument);
        if (owner.Mode == 2 && (nint)argument == 1)
            owner.MutexReleased = pthread_mutex_unlock(owner.Mutex) == 0 ? 1 : -1;
        if ((nint)argument == 1) owner.Done.Set();
    }

    private static void* Worker(Owner owner, void* ignored)
    {
        pthread_cleanup_push(owner, &Cleanup, (void*)1);
        pthread_cleanup_push(owner, &Cleanup, (void*)2);
        if (owner.Mode == 1) pthread_setcancelstate(PTHREAD_CANCEL_DISABLE, null).ShouldBe(0);
        if (owner.Mode == 2)
        {
            pthread_mutex_lock(owner.Mutex).ShouldBe(0);
            owner.Ready.Set();
            pthread_cond_wait(owner.Condition, owner.Mutex);
        }
        else
        {
            owner.Ready.Set(); owner.Resume.Wait();
            if (owner.Mode == 1)
            {
                pthread_testcancel(); owner.PassedDisabled = 1;
                pthread_setcancelstate(PTHREAD_CANCEL_ENABLE, null).ShouldBe(0);
            }
            if (owner.Mode == 3) pthread_exit((void*)73);
            if (owner.Mode == 4)
            {
                pthread_cleanup_pop(0);
                pthread_cleanup_pop(1);
                return (void*)74;
            }
            pthread_testcancel();
        }
        owner.Returned = 1;
        return null;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Cancellation_and_exit_run_owned_cleanup_in_reverse_order(int mode)
    {
        using var owner = new Owner { Mode = mode };
        using var binding = owner.__DotCcRuntime.Enter();
        owner.Mutex = (int*)calloc(1, sizeof(int));
        owner.Condition = (int*)calloc(1, sizeof(int));
        long worker = 0;
        pthread_create(owner, &worker, null, &Worker, null).ShouldBe(0);
        owner.Ready.Wait(TimeSpan.FromSeconds(5)).ShouldBeTrue();
        if (mode < 3) pthread_cancel(worker).ShouldBe(0);
        owner.Resume.Set();
        owner.Done.Wait(TimeSpan.FromSeconds(5)).ShouldBeTrue();
        void* result = null;
        pthread_join(worker, &result).ShouldBe(0);
        ((nint)result).ShouldBe(mode < 3 ? -1 : mode == 3 ? 73 : 74);
        owner.Cleanup.ShouldBe(mode == 4 ? new[] { 1 } : new[] { 2, 1 });
        owner.Returned.ShouldBe(0);
        owner.__DotCcRuntime.Termination.ShouldBeNull();
        if (mode == 1) owner.PassedDisabled.ShouldBe(1);
        if (mode == 2) owner.MutexReleased.ShouldBe(1);
        pthread_mutex_destroy(owner.Mutex).ShouldBe(0);
        pthread_cond_destroy(owner.Condition).ShouldBe(0);
        pthread_cancel(worker).ShouldBe(ESRCH);
    }

    [Fact]
    public void Cancellation_policy_errors_preserve_previous_output()
    {
        using var context = new RuntimeContext();
        using var binding = context.Enter();
        int previous = 42;
        pthread_setcanceltype(PTHREAD_CANCEL_ASYNCHRONOUS, &previous).ShouldBe(ENOTSUP);
        pthread_setcanceltype(99, &previous).ShouldBe(EINVAL);
        pthread_setcancelstate(99, &previous).ShouldBe(EINVAL);
        previous.ShouldBe(42);
        pthread_setcanceltype(PTHREAD_CANCEL_DEFERRED, &previous).ShouldBe(0);
        previous.ShouldBe(PTHREAD_CANCEL_DEFERRED);
        pthread_setcancelstate(PTHREAD_CANCEL_DISABLE, &previous).ShouldBe(0);
        previous.ShouldBe(PTHREAD_CANCEL_ENABLE);
        pthread_setcancelstate(PTHREAD_CANCEL_ENABLE, &previous).ShouldBe(0);
        previous.ShouldBe(PTHREAD_CANCEL_DISABLE);
        pthread_cancel(long.MaxValue).ShouldBe(ESRCH);
    }
}

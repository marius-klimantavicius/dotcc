#nullable enable
using System;
using System.Threading;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Threads")]
public unsafe class PthreadLibTests
{
    private static void* Echo(void* arg) => arg;
    private static void* Exit(void* arg) { pthread_exit(arg); return null; }
    private static void* Self(void* arg) => (void*)(nint)pthread_equal(pthread_self(), *(long*)arg);
    private static void* ForeignUnlock(void* arg) => (void*)(nint)pthread_mutex_unlock((int*)arg);
    private static void* ExpiredLock(void* arg)
    {
        timespec deadline = new() { tv_sec = 0, tv_nsec = 0 };
        return (void*)(nint)pthread_mutex_timedlock((int*)arg, &deadline);
    }
    private static long Spawn(delegate*<void*, void*> callback, void* argument)
    {
        long thread;
        pthread_create(&thread, null, callback, argument).ShouldBe(0);
        return thread;
    }
    private static void* Join(long thread)
    {
        void* value = null;
        pthread_join(thread, &value).ShouldBe(0);
        return value;
    }
    [Fact]
    public void thread_results_preserve_pointer_width_and_exit_results()
    {
        void* value = (void*)0x123456789abL;
        ((nint)Join(Spawn(&Echo, value))).ShouldBe((nint)value);
        ((nint)Join(Spawn(&Exit, value))).ShouldBe((nint)value);
        pthread_equal(pthread_self(), pthread_self()).ShouldBe(1);
        pthread_join(pthread_self(), null).ShouldBe(EDEADLK);
        long self = pthread_self();
        ((nint)Join(Spawn(&Self, &self))).ShouldBe(0);
    }
    [Fact]
    public void errors_do_not_change_errno_or_output()
    {
        errno = 123;
        long thread = 87;
        pthread_create(&thread, null, null, null).ShouldBe(EINVAL);
        thread.ShouldBe(87);
        pthread_join(-123, null).ShouldBe(ESRCH);
        pthread_detach(-123).ShouldBe(ESRCH);
        int key = 73;
        pthread_setspecific(-1, null).ShouldBe(EINVAL);
        pthread_key_delete(-1).ShouldBe(EINVAL);
        pthread_attr_getdetachstate(null, &key).ShouldBe(EINVAL);
        key.ShouldBe(73);
        errno.ShouldBe(123);
    }
    [Fact]
    public void recursive_mutex_tracks_depth_and_rejects_foreign_unlock()
    {
        int attr, mutex;
        pthread_mutexattr_init(&attr).ShouldBe(0);
        pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_RECURSIVE).ShouldBe(0);
        pthread_mutex_init(&mutex, &attr).ShouldBe(0);
        pthread_mutex_lock(&mutex).ShouldBe(0);
        pthread_mutex_trylock(&mutex).ShouldBe(0);
        ((nint)Join(Spawn(&ForeignUnlock, &mutex))).ShouldBe(EPERM);
        pthread_mutex_destroy(&mutex).ShouldBe(EBUSY);
        pthread_mutex_unlock(&mutex).ShouldBe(0);
        ((nint)Join(Spawn(&ExpiredLock, &mutex))).ShouldBe(ETIMEDOUT);
        pthread_mutex_unlock(&mutex).ShouldBe(0);
        pthread_mutex_unlock(&mutex).ShouldBe(EPERM);
        pthread_mutex_destroy(&mutex).ShouldBe(0);
        pthread_mutex_lock(&mutex).ShouldBe(EINVAL);
        pthread_mutexattr_destroy(&attr).ShouldBe(0);
    }
    [Fact]
    public void errorcheck_mutex_and_timed_validation_match_posix()
    {
        int attr, mutex;
        pthread_mutexattr_init(&attr);
        pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_ERRORCHECK);
        pthread_mutex_init(&mutex, &attr);
        timespec invalid = new() { tv_sec = long.MaxValue, tv_nsec = 1_000_000_000 };
        pthread_mutex_timedlock(&mutex, &invalid).ShouldBe(0); // available: no timeout validation required
        pthread_mutex_lock(&mutex).ShouldBe(EDEADLK);
        pthread_mutex_trylock(&mutex).ShouldBe(EBUSY);
        pthread_mutex_unlock(&mutex).ShouldBe(0);
        pthread_mutex_destroy(&mutex).ShouldBe(0);
        pthread_mutexattr_destroy(&attr).ShouldBe(0);
    }
    private struct Counter { public int Mutex, Value; }
    private static void* Increment(void* arg)
    {
        var counter = (Counter*)arg;
        for (int i = 0; i < 500; i++)
        {
            if (pthread_mutex_lock(&counter->Mutex) != 0) return (void*)1;
            counter->Value++;
            if (pthread_mutex_unlock(&counter->Mutex) != 0) return (void*)2;
        }
        return null;
    }
    [Fact]
    public void static_mutex_initializes_once_under_contention()
    {
        Counter counter = default;
        long* threads = stackalloc long[8];
        for (int i = 0; i < 8; i++) threads[i] = Spawn(&Increment, &counter);
        for (int i = 0; i < 8; i++) ((nint)Join(threads[i])).ShouldBe(0);
        counter.Value.ShouldBe(4000);
        pthread_mutex_destroy(&counter.Mutex).ShouldBe(0);
    }
    [Fact]
    public void condition_timeout_reacquires_mutex_and_invalid_deadline_keeps_ownership()
    {
        int condition = 0, mutex = 0;
        timespec deadline = new() { tv_sec = -1, tv_nsec = 0 };
        pthread_mutex_lock(&mutex).ShouldBe(0);
        pthread_cond_timedwait(&condition, &mutex, &deadline).ShouldBe(ETIMEDOUT);
        pthread_mutex_trylock(&mutex).ShouldBe(EBUSY);
        deadline.tv_nsec = -1;
        pthread_cond_timedwait(&condition, &mutex, &deadline).ShouldBe(EINVAL);
        pthread_mutex_unlock(&mutex).ShouldBe(0);
        pthread_cond_wait(&condition, &mutex).ShouldBe(EPERM);
        pthread_cond_destroy(&condition).ShouldBe(0);
        pthread_mutex_destroy(&mutex).ShouldBe(0);
    }
    private static readonly ManualResetEventSlim Ready = new(false);
    private struct ConditionArgs { public int Mutex, Condition, Waiting, Go, Completed; }
    private static void* ConditionWorker(void* argument)
    {
        var a = (ConditionArgs*)argument;
        pthread_mutex_lock(&a->Mutex);
        if (++a->Waiting == 4) Ready.Set();
        while (a->Go == 0)
        {
            int result = pthread_cond_wait(&a->Condition, &a->Mutex);
            if (result != 0) { pthread_mutex_unlock(&a->Mutex); return (void*)(nint)result; }
        }
        a->Completed++;
        pthread_mutex_unlock(&a->Mutex);
        return null;
    }
    [Fact]
    public void condition_broadcast_wakes_all_without_lost_wakeups_and_busy_destroy_is_detected()
    {
        Ready.Reset();
        ConditionArgs args = default;
        long* threads = stackalloc long[4];
        for (int i = 0; i < 4; i++) threads[i] = Spawn(&ConditionWorker, &args);
        Ready.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).ShouldBeTrue();
        pthread_mutex_lock(&args.Mutex).ShouldBe(0);
        pthread_cond_destroy(&args.Condition).ShouldBe(EBUSY);
        pthread_mutex_unlock(&args.Mutex).ShouldBe(0);
        pthread_mutex_destroy(&args.Mutex).ShouldBe(EBUSY); // mutex released by all condition waiters
        pthread_mutex_lock(&args.Mutex).ShouldBe(0);
        args.Go = 1;
        pthread_cond_broadcast(&args.Condition).ShouldBe(0);
        pthread_mutex_unlock(&args.Mutex).ShouldBe(0);
        for (int i = 0; i < 4; i++) ((nint)Join(threads[i])).ShouldBe(0);
        args.Completed.ShouldBe(4);
        pthread_cond_destroy(&args.Condition).ShouldBe(0);
        pthread_mutex_destroy(&args.Mutex).ShouldBe(0);
    }
    [Fact]
    public void managed_interruption_reacquires_mutex_and_clears_waiter_accounting()
    {
        int condition = 0, mutex = 0;
        nint c = (nint)(&condition), m = (nint)(&mutex);
        using var waiting = new ManualResetEventSlim(false);
        int unlocked = -1;
        var thread = new Thread(() =>
        {
            pthread_mutex_lock((int*)m);
            waiting.Set();
            try { pthread_cond_wait((int*)c, (int*)m); }
            catch (ThreadInterruptedException) { unlocked = pthread_mutex_unlock((int*)m); }
        });
        thread.Start();
        waiting.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).ShouldBeTrue();
        thread.Interrupt();
        thread.Join(TimeSpan.FromSeconds(10)).ShouldBeTrue();
        unlocked.ShouldBe(0);
        pthread_cond_destroy(&condition).ShouldBe(0);
        pthread_mutex_destroy(&mutex).ShouldBe(0);
    }
    private static void* FarFutureWait(void* argument)
    {
        var a = (ConditionArgs*)argument;
        timespec future = new() { tv_sec = long.MaxValue, tv_nsec = 999_999_999 };
        pthread_mutex_lock(&a->Mutex);
        Ready.Set();
        int result = 0;
        while (a->Go == 0 && result == 0)
            result = pthread_cond_timedwait(&a->Condition, &a->Mutex, &future);
        pthread_mutex_unlock(&a->Mutex);
        return (void*)(nint)result;
    }
    [Fact]
    public void signal_wakes_timed_wait_with_overflow_safe_far_future_deadline()
    {
        Ready.Reset();
        ConditionArgs args = default;
        long thread = Spawn(&FarFutureWait, &args);
        Ready.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).ShouldBeTrue();
        pthread_mutex_lock(&args.Mutex).ShouldBe(0);
        args.Go = 1;
        pthread_cond_signal(&args.Condition).ShouldBe(0);
        pthread_mutex_unlock(&args.Mutex).ShouldBe(0);
        ((nint)Join(thread)).ShouldBe(0);
        pthread_cond_destroy(&args.Condition).ShouldBe(0);
        pthread_mutex_destroy(&args.Mutex).ShouldBe(0);
    }
    private static int _onceCalls;
    private static void OnceInitialize() => Interlocked.Increment(ref _onceCalls);
    private static void FailInitialize() => throw new InvalidOperationException("retry initialization");
    private static void* OnceWorker(void* argument) => (void*)(nint)pthread_once((int*)argument, &OnceInitialize);
    [Fact]
    public void once_serializes_initialization_and_retries_after_unwind()
    {
        int flag = 0;
        _onceCalls = 0;
        bool threw = false;
        try { pthread_once(&flag, &FailInitialize); }
        catch (InvalidOperationException) { threw = true; }
        threw.ShouldBeTrue(); flag.ShouldBe(0);
        long* threads = stackalloc long[8];
        for (int i = 0; i < 8; i++) threads[i] = Spawn(&OnceWorker, &flag);
        for (int i = 0; i < 8; i++) ((nint)Join(threads[i])).ShouldBe(0);
        _onceCalls.ShouldBe(1);
    }
    private static int _key, _destructors, _clearedBeforeDestructor;
    private static void KeyDestructor(void* value)
    {
        if (pthread_getspecific(_key) == null) _clearedBeforeDestructor++;
        _destructors++;
        pthread_setspecific(_key, value); // bounded retry; value intentionally restored each round
    }
    private static void* KeyWorker(void* value)
    {
        if (pthread_getspecific(_key) != null) return (void*)1;
        pthread_setspecific(_key, value);
        if (pthread_getspecific(_key) != value) return (void*)2;
        pthread_exit(value);
        return null;
    }
    [Fact]
    public void tls_is_isolated_destructors_clear_values_and_retry_four_times_on_exit()
    {
        int key;
        pthread_key_create(&key, &KeyDestructor).ShouldBe(0);
        _key = key; _destructors = 0; _clearedBeforeDestructor = 0;
        pthread_setspecific(key, (void*)17).ShouldBe(0);
        ((nint)Join(Spawn(&KeyWorker, (void*)42))).ShouldBe(42);
        ((nint)pthread_getspecific(key)).ShouldBe(17);
        _destructors.ShouldBe(PTHREAD_DESTRUCTOR_ITERATIONS);
        _clearedBeforeDestructor.ShouldBe(PTHREAD_DESTRUCTOR_ITERATIONS);
        pthread_key_delete(key).ShouldBe(0);
        ((nint)pthread_getspecific(key)).ShouldBe(0);
        _destructors.ShouldBe(PTHREAD_DESTRUCTOR_ITERATIONS); // delete never invokes destructor
    }
    private static readonly ManualResetEventSlim DetachedRelease = new(false), DetachedDone = new(false);
    private static void* DetachedWorker(void* value) { DetachedRelease.Wait(); DetachedDone.Set(); return null; }
    [Fact]
    public void detach_rejects_join_and_second_detach_while_thread_is_alive()
    {
        DetachedRelease.Reset(); DetachedDone.Reset();
        long thread = Spawn(&DetachedWorker, null);
        try
        {
            pthread_detach(thread).ShouldBe(0);
            pthread_detach(thread).ShouldBe(EINVAL);
            pthread_join(thread, null).ShouldBe(EINVAL);
        }
        finally { DetachedRelease.Set(); }
        DetachedDone.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).ShouldBeTrue();
    }
    [Fact]
    public void detached_creation_honors_attributes_and_joined_thread_cannot_be_joined_twice()
    {
        long finished = Spawn(&Echo, null);
        Join(finished);
        pthread_join(finished, null).ShouldBe(ESRCH);
        DetachedRelease.Reset(); DetachedDone.Reset();
        int attr;
        pthread_attr_init(&attr);
        pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED);
        long thread;
        pthread_create(&thread, &attr, &DetachedWorker, null).ShouldBe(0);
        try { pthread_join(thread, null).ShouldBe(EINVAL); }
        finally { DetachedRelease.Set(); }
        DetachedDone.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).ShouldBeTrue();
        pthread_attr_destroy(&attr).ShouldBe(0);
    }
    [Fact]
    public void attributes_reject_unsupported_process_sharing()
    {
        int attr, value;
        pthread_attr_init(&attr).ShouldBe(0);
        pthread_attr_getdetachstate(&attr, &value).ShouldBe(0);
        value.ShouldBe(PTHREAD_CREATE_JOINABLE);
        pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED).ShouldBe(0);
        pthread_attr_getdetachstate(&attr, &value).ShouldBe(0);
        value.ShouldBe(PTHREAD_CREATE_DETACHED);
        pthread_attr_destroy(&attr).ShouldBe(0);
        pthread_mutexattr_init(&attr).ShouldBe(0);
        pthread_mutexattr_setpshared(&attr, PTHREAD_PROCESS_SHARED).ShouldBe(ENOTSUP);
        pthread_mutexattr_getpshared(&attr, &value).ShouldBe(0); value.ShouldBe(0);
        pthread_mutexattr_destroy(&attr).ShouldBe(0);
        pthread_condattr_init(&attr).ShouldBe(0);
        pthread_condattr_setpshared(&attr, PTHREAD_PROCESS_SHARED).ShouldBe(ENOTSUP);
        pthread_condattr_getpshared(&attr, &value).ShouldBe(0); value.ShouldBe(0);
        pthread_condattr_destroy(&attr).ShouldBe(0);
    }
}

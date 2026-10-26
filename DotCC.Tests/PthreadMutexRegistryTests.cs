#nullable enable
using System;
using System.Threading;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Threads")]
public unsafe class PthreadMutexRegistryTests
{
    [Fact]
    public void duplicate_ids_in_nested_contexts_keep_their_own_type_and_owner()
    {
        using var first = new RuntimeContext();
        using var second = new RuntimeContext();
        int a = 0, b = 0;
        using (first.Enter())
        {
            int type = PTHREAD_MUTEX_ERRORCHECK;
            pthread_mutex_init(&a, &type).ShouldBe(0);
            pthread_mutex_lock(&a).ShouldBe(0);
            using (second.Enter())
            {
                type = PTHREAD_MUTEX_RECURSIVE;
                pthread_mutex_init(&b, &type).ShouldBe(0);
                a.ShouldBe(b);
                pthread_mutex_lock(&b).ShouldBe(0);
                pthread_mutex_lock(&b).ShouldBe(0);
                pthread_mutex_unlock(&b).ShouldBe(0);
            }
            pthread_mutex_lock(&a).ShouldBe(EDEADLK);
            pthread_mutex_destroy(&a).ShouldBe(EBUSY);
            pthread_mutex_unlock(&a).ShouldBe(0);
            pthread_mutex_destroy(&a).ShouldBe(0);
            using (second.Enter())
            {
                pthread_mutex_destroy(&b).ShouldBe(EBUSY);
                pthread_mutex_unlock(&b).ShouldBe(0);
                pthread_mutex_destroy(&b).ShouldBe(0);
            }
        }
    }

    [Fact]
    public void same_address_destroy_and_reinitialize_never_revives_a_copied_handle()
    {
        using var context = new RuntimeContext();
        using var binding = context.Enter();
        int mutex = 0;
        for (int i = 0; i < context.MutexCache.Length * 4; i++)
        {
            pthread_mutex_lock(&mutex).ShouldBe(0);
            pthread_mutex_unlock(&mutex).ShouldBe(0);
            int stale = mutex;
            var state = context.Mutexes[stale];
            pthread_mutex_lock(&stale).ShouldBe(0);
            pthread_mutex_unlock(&stale).ShouldBe(0);
            pthread_mutex_destroy(&mutex).ShouldBe(0);
            state.Destroyed.ShouldBeTrue();
            pthread_mutex_lock(&stale).ShouldBe(EINVAL);
            pthread_mutex_unlock(&stale).ShouldBe(EINVAL);
            pthread_mutex_destroy(&stale).ShouldBe(EINVAL);
            pthread_mutex_init(&mutex, null).ShouldBe(0);
            mutex.ShouldBeGreaterThan(stale);
            pthread_mutex_trylock(&mutex).ShouldBe(0);
            pthread_mutex_unlock(&mutex).ShouldBe(0);
            pthread_mutex_destroy(&mutex).ShouldBe(0);
            mutex = 0; // The same storage can also become a new static initializer.
        }
        context.Mutexes.Count.ShouldBe(0);
        foreach (var cached in context.MutexCache) cached.ShouldBeNull();
    }

    [Fact]
    public void registry_larger_than_cache_bypasses_publication_and_reenables_after_destroy()
    {
        using var context = new RuntimeContext();
        using var binding = context.Enter();
        int count = context.MutexCache.Length * 4 + 1;
        int* handles = stackalloc int[count];
        for (int i = 0; i < count; i++) pthread_mutex_init(&handles[i], null).ShouldBe(0);
        context.MutexCacheEnabled.ShouldBeFalse();
        foreach (var cached in context.MutexCache) cached.ShouldBeNull();
        for (int pass = 0; pass < 2; pass++)
            for (int i = 0; i < count; i++)
            {
                pthread_mutex_lock(&handles[i]).ShouldBe(0);
                pthread_mutex_unlock(&handles[i]).ShouldBe(0);
            }
        foreach (var cached in context.MutexCache) cached.ShouldBeNull();
        int removed = count - context.MutexCache.Length;
        for (int i = 0; i < removed; i++) pthread_mutex_destroy(&handles[i]).ShouldBe(0);
        context.MutexCacheEnabled.ShouldBeTrue();
        for (int i = removed; i < count; i++)
        {
            int id = handles[i];
            context.MutexCache[id & (context.MutexCache.Length - 1)].ShouldBeSameAs(context.Mutexes[id]);
            pthread_mutex_lock(&handles[i]).ShouldBe(0);
            pthread_mutex_unlock(&handles[i]).ShouldBe(0);
            pthread_mutex_destroy(&handles[i]).ShouldBe(0);
        }
        context.Mutexes.Count.ShouldBe(0);
        context.MutexCache.Length.ShouldBe(64);
        foreach (var cached in context.MutexCache) cached.ShouldBeNull();
    }

    [Fact]
    public void destroy_keeps_a_different_live_id_in_the_same_cache_slot()
    {
        using var context = new RuntimeContext();
        using var binding = context.Enter();
        int first = 0, other = 0;
        pthread_mutex_init(&first, null).ShouldBe(0);
        for (int i = 1; i < context.MutexCache.Length; i++)
        {
            pthread_mutex_init(&other, null).ShouldBe(0);
            pthread_mutex_destroy(&other).ShouldBe(0);
        }
        pthread_mutex_init(&other, null).ShouldBe(0);
        int slot = other & (context.MutexCache.Length - 1);
        slot.ShouldBe(first & (context.MutexCache.Length - 1));
        var otherState = context.Mutexes[other];
        context.MutexCache[slot].ShouldBeSameAs(otherState);
        pthread_mutex_destroy(&first).ShouldBe(0);
        context.MutexCache[slot].ShouldBeSameAs(otherState);
        pthread_mutex_lock(&other).ShouldBe(0);
        pthread_mutex_unlock(&other).ShouldBe(0);
        pthread_mutex_destroy(&other).ShouldBe(0);
    }

    [Fact]
    public void static_initializers_also_disable_cache_above_capacity()
    {
        using var context = new RuntimeContext();
        using var binding = context.Enter();
        int count = context.MutexCache.Length + 1;
        int* handles = stackalloc int[count];
        for (int i = 0; i < count; i++)
        {
            handles[i] = 0;
            pthread_mutex_lock(&handles[i]).ShouldBe(0);
            pthread_mutex_unlock(&handles[i]).ShouldBe(0);
        }
        context.MutexCacheEnabled.ShouldBeFalse();
        foreach (var cached in context.MutexCache) cached.ShouldBeNull();
        for (int i = 0; i < count; i++) pthread_mutex_destroy(&handles[i]).ShouldBe(0);
        context.MutexCacheEnabled.ShouldBeTrue();
        foreach (var cached in context.MutexCache) cached.ShouldBeNull();
    }

    [Fact]
    public void cache_capacity_transitions_preserve_an_active_owner_and_blocked_waiter()
    {
        using var context = new RuntimeContext();
        using var binding = context.Enter();
        int mutex = 0;
        pthread_mutex_lock(&mutex).ShouldBe(0);
        var state = context.Mutexes[mutex];
        int id = mutex;
        Exception? failure = null;
        var worker = new Thread(() =>
        {
            try
            {
                using var scope = context.Enter();
                int copy = id;
                pthread_mutex_lock(&copy).ShouldBe(0);
                pthread_mutex_unlock(&copy).ShouldBe(0);
            }
            catch (Exception error) { failure = error; }
        }) { IsBackground = true };
        worker.Start();
        try
        {
            SpinWait.SpinUntil(() => { lock (state) return state.Waiters == 1; }, TimeSpan.FromSeconds(10)).ShouldBeTrue();
            int* temporary = stackalloc int[context.MutexCache.Length];
            for (int i = 0; i < context.MutexCache.Length; i++) pthread_mutex_init(&temporary[i], null).ShouldBe(0);
            context.MutexCacheEnabled.ShouldBeFalse();
            foreach (var cached in context.MutexCache) cached.ShouldBeNull();
            for (int i = 0; i < context.MutexCache.Length; i++) pthread_mutex_destroy(&temporary[i]).ShouldBe(0);
            context.MutexCacheEnabled.ShouldBeTrue();
            // A temporary colliding ID may have evicted this entry during
            // repopulation. The next lookup must resolve the original state.
            pthread_mutex_trylock(&mutex).ShouldBe(EBUSY);
            context.MutexCache[id & (context.MutexCache.Length - 1)].ShouldBeSameAs(state);
            lock (state) state.Waiters.ShouldBe(1);
            pthread_mutex_destroy(&mutex).ShouldBe(EBUSY);
        }
        finally
        {
            pthread_mutex_unlock(&mutex).ShouldBe(0);
            worker.Join(TimeSpan.FromSeconds(10)).ShouldBeTrue();
        }
        failure.ShouldBeNull();
        state.Waiters.ShouldBe(0);
        state.Owner.ShouldBe(0);
        pthread_mutex_destroy(&mutex).ShouldBe(0);
    }

    [Fact]
    public void cached_destroyed_handle_on_another_thread_is_rejected_after_address_reuse()
    {
        using var context = new RuntimeContext();
        using var binding = context.Enter();
        int mutex = 0;
        pthread_mutex_init(&mutex, null).ShouldBe(0);
        int oldId = mutex;
        using var primed = new ManualResetEventSlim(false);
        using var resume = new ManualResetEventSlim(false);
        Exception? failure = null;
        int staleResult = -1;
        var worker = new Thread(() =>
        {
            try
            {
                using var scope = context.Enter();
                int copy = oldId;
                pthread_mutex_lock(&copy).ShouldBe(0);
                pthread_mutex_unlock(&copy).ShouldBe(0);
                primed.Set();
                if (!resume.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
                staleResult = pthread_mutex_lock(&copy);
            }
            catch (Exception error) { failure = error; primed.Set(); }
        }) { IsBackground = true };
        worker.Start();
        try
        {
            primed.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).ShouldBeTrue();
            failure.ShouldBeNull();
            pthread_mutex_destroy(&mutex).ShouldBe(0);
            pthread_mutex_init(&mutex, null).ShouldBe(0);
            mutex.ShouldNotBe(oldId);
        }
        finally
        {
            resume.Set();
            worker.Join(TimeSpan.FromSeconds(10)).ShouldBeTrue();
        }
        failure.ShouldBeNull();
        staleResult.ShouldBe(EINVAL);
        pthread_mutex_lock(&mutex).ShouldBe(0);
        pthread_mutex_unlock(&mutex).ShouldBe(0);
        pthread_mutex_destroy(&mutex).ShouldBe(0);
    }

    [Fact]
    public void interrupted_mutex_waiter_retains_busy_destroy_and_releases_waiter_accounting()
    {
        using var context = new RuntimeContext();
        using var binding = context.Enter();
        int mutex = 0;
        pthread_mutex_lock(&mutex).ShouldBe(0);
        var state = context.Mutexes[mutex];
        int id = mutex;
        Exception? failure = null;
        bool interrupted = false;
        var worker = new Thread(() =>
        {
            try
            {
                using var scope = context.Enter();
                int copy = id;
                pthread_mutex_lock(&copy);
            }
            catch (ThreadInterruptedException) { interrupted = true; }
            catch (Exception error) { failure = error; }
        }) { IsBackground = true };
        worker.Start();
        try
        {
            SpinWait.SpinUntil(() => { lock (state) return state.Waiters == 1; }, TimeSpan.FromSeconds(10)).ShouldBeTrue();
            pthread_mutex_destroy(&mutex).ShouldBe(EBUSY);
            worker.Interrupt();
            worker.Join(TimeSpan.FromSeconds(10)).ShouldBeTrue();
        }
        finally
        {
            pthread_mutex_unlock(&mutex).ShouldBe(0);
            worker.Join(TimeSpan.FromSeconds(10)).ShouldBeTrue();
        }
        failure.ShouldBeNull();
        interrupted.ShouldBeTrue();
        state.Waiters.ShouldBe(0);
        pthread_mutex_destroy(&mutex).ShouldBe(0);
    }

    [Fact]
    public void cached_condition_mutex_stays_busy_until_waiter_reacquires_and_finishes()
    {
        using var context = new RuntimeContext();
        using var binding = context.Enter();
        int mutex = 0, condition = 0;
        nint m = (nint)(&mutex), c = (nint)(&condition);
        using var ready = new ManualResetEventSlim(false);
        Exception? failure = null;
        int go = 0;
        var worker = new Thread(() =>
        {
            try
            {
                using var scope = context.Enter();
                timespec deadline = new() { tv_sec = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 20 };
                pthread_mutex_lock((int*)m).ShouldBe(0);
                try
                {
                    ready.Set();
                    while (go == 0) pthread_cond_timedwait((int*)c, (int*)m, &deadline).ShouldBe(0);
                }
                finally { pthread_mutex_unlock((int*)m).ShouldBe(0); }
            }
            catch (Exception error) { failure = error; ready.Set(); }
        }) { IsBackground = true };
        worker.Start();
        try
        {
            ready.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).ShouldBeTrue();
            pthread_mutex_lock(&mutex).ShouldBe(0);
            pthread_mutex_unlock(&mutex).ShouldBe(0);
            var state = context.Mutexes[mutex];
            lock (state)
            {
                state.Owner.ShouldBe(0);
                state.ConditionWaiters.ShouldBe(1);
            }
            pthread_mutex_destroy(&mutex).ShouldBe(EBUSY);
            pthread_cond_destroy(&condition).ShouldBe(EBUSY);
        }
        finally
        {
            pthread_mutex_lock(&mutex).ShouldBe(0);
            go = 1;
            pthread_cond_signal(&condition).ShouldBe(0);
            pthread_mutex_unlock(&mutex).ShouldBe(0);
            worker.Join(TimeSpan.FromSeconds(10)).ShouldBeTrue();
        }
        failure.ShouldBeNull();
        context.Mutexes[mutex].ConditionWaiters.ShouldBe(0);
        pthread_cond_destroy(&condition).ShouldBe(0);
        pthread_mutex_destroy(&mutex).ShouldBe(0);
    }

    [Fact]
    public void disposing_runtime_clears_live_registry_and_bounded_cache_references()
    {
        var context = new RuntimeContext();
        using (context.Enter())
        {
            int mutex = 0;
            pthread_mutex_lock(&mutex).ShouldBe(0);
            pthread_mutex_unlock(&mutex).ShouldBe(0);
        }
        context.Mutexes.Count.ShouldBe(1);
        context.MutexCache[1].ShouldNotBeNull();
        context.Dispose();
        context.Mutexes.Count.ShouldBe(0);
        foreach (var cached in context.MutexCache) cached.ShouldBeNull();
        Should.Throw<ObjectDisposedException>(() => context.Enter());
    }
}

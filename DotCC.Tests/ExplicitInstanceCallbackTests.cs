#nullable enable
using System;
using System.Threading;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class ExplicitInstanceCallbackTests
{
    private sealed class Owner(int direction = 1) : IProgramInstance, IDisposable
    {
        public RuntimeContext __DotCcRuntime { get; } = new();
        public int Direction = direction, Comparisons, Seen;
        public long ThreadId;
        public readonly ManualResetEventSlim Entered = new(), Leave = new();
        public IDisposable __DotCcEnter() => __DotCcRuntime.Enter();
        public void Dispose() { __DotCcRuntime.Dispose(); Entered.Dispose(); Leave.Dispose(); }
    }
    private static int Compare(Owner owner, void* left, void* right)
    {
        using var binding = owner.__DotCcEnter();
        ReferenceEquals(RuntimeContext.Current, owner.__DotCcRuntime).ShouldBeTrue();
        ++owner.Comparisons;
        errno = owner.Direction;
        return owner.Direction * (*(int*)left).CompareTo(*(int*)right);
    }
    private static void* ThreadStart(Owner owner, void* argument)
    {
        using var binding = owner.__DotCcEnter();
        owner.Seen = ReferenceEquals(RuntimeContext.Current, owner.__DotCcRuntime) ? 1 : -1;
        owner.ThreadId = pthread_self();
        errno = 73;
        owner.Entered.Set();
        if (!owner.Leave.Wait(TimeSpan.FromSeconds(10))) return (void*)999;
        return argument;
    }

    private static void OnceInitialize(Owner owner)
    {
        ReferenceEquals(RuntimeContext.Current, owner.__DotCcRuntime).ShouldBeTrue();
        ++owner.Seen;
        errno = owner.Direction;
    }

    [Fact]
    public void Once_initializers_receive_their_owner_and_restore_unrelated_binding()
    {
        using var a = new Owner(17); using var b = new Owner(23); using var ambient = new Owner();
        int first = 0, second = 0;
        using (ambient.__DotCcEnter())
        {
            errno = 91;
            pthread_once(a, &first, &OnceInitialize).ShouldBe(0);
            pthread_once(b, &second, &OnceInitialize).ShouldBe(0);
            pthread_once(a, &first, &OnceInitialize).ShouldBe(0);
            pthread_once(b, &second, &OnceInitialize).ShouldBe(0);
            ReferenceEquals(RuntimeContext.Current, ambient.__DotCcRuntime).ShouldBeTrue();
            errno.ShouldBe(91);
        }
        a.Seen.ShouldBe(1); b.Seen.ShouldBe(1);
        using (a.__DotCcEnter()) errno.ShouldBe(17);
        using (b.__DotCcEnter()) errno.ShouldBe(23);
    }

    [Fact]
    public void Sort_and_search_forward_each_explicit_instance_and_restore_ambient_owner()
    {
        using var ascending = new Owner(); using var descending = new Owner(-1); using var ambient = new Owner();
        int* a = stackalloc int[] { 7, -2, 4, 4, 0 };
        int* b = stackalloc int[] { 7, -2, 4, 4, 0 };
        using (ambient.__DotCcEnter())
        {
            errno = 91;
            qsort(ascending, a, 5, sizeof(int), &Compare);
            qsort(descending, b, 5, sizeof(int), &Compare);
            new ReadOnlySpan<int>(a, 5).ToArray().ShouldBe(new[] { -2, 0, 4, 4, 7 });
            new ReadOnlySpan<int>(b, 5).ToArray().ShouldBe(new[] { 7, 4, 4, 0, -2 });
            int key = 0;
            ((nint)bsearch(ascending, &key, a, 5, sizeof(int), &Compare)).ShouldBe((nint)(a + 1));
            ((nint)bsearch(descending, &key, b, 5, sizeof(int), &Compare)).ShouldBe((nint)(b + 3));
            key = 99;
            ((nint)bsearch(ascending, &key, a, 5, sizeof(int), &Compare)).ShouldBe(0);
            ReferenceEquals(RuntimeContext.Current, ambient.__DotCcRuntime).ShouldBeTrue();
            errno.ShouldBe(91);
        }
        ascending.Comparisons.ShouldBeGreaterThan(0); descending.Comparisons.ShouldBeGreaterThan(0);
        using (ascending.__DotCcEnter()) errno.ShouldBe(1);
        using (descending.__DotCcEnter()) errno.ShouldBe(-1);
    }

    [Fact]
    public void Deferred_registration_lease_reserves_owner_without_binding_and_releases_on_another_thread()
    {
        using var owner = new Owner(); using var ambient = new Owner();
        IDisposable lease;
        using (ambient.__DotCcEnter())
        {
            lease = owner.__DotCcRuntime.RetainLease();
            ReferenceEquals(RuntimeContext.Current, ambient.__DotCcRuntime).ShouldBeTrue();
        }
        try { Should.Throw<InvalidOperationException>(() => owner.__DotCcRuntime.Dispose()); }
        finally
        {
            var unregister = new Thread(lease.Dispose);
            unregister.Start(); unregister.Join();
            lease.Dispose(); // Registration cleanup is safely idempotent.
        }
        owner.__DotCcRuntime.Dispose();
        Should.Throw<ObjectDisposedException>(() => owner.__DotCcRuntime.RetainLease());
    }

    [Fact]
    public void Pthread_reserves_explicit_origin_under_an_unrelated_binding_until_joinable_cleanup()
    {
        using var origin = new Owner(); using var unrelated = new Owner();
        long thread = 0;
        using (unrelated.__DotCcEnter())
        {
            pthread_create(origin, &thread, null, &ThreadStart, (void*)123).ShouldBe(0);
            ReferenceEquals(RuntimeContext.Current, unrelated.__DotCcRuntime).ShouldBeTrue();
        }
        try
        {
            origin.Entered.Wait(TimeSpan.FromSeconds(5)).ShouldBeTrue();
            origin.Seen.ShouldBe(1); origin.ThreadId.ShouldBe(thread);
            Should.Throw<InvalidOperationException>(() => origin.__DotCcRuntime.Dispose());
            // The new thread did not accidentally reserve its caller's owner.
            unrelated.__DotCcRuntime.Dispose();
        }
        finally
        {
            origin.Leave.Set();
            using (origin.__DotCcEnter())
            {
                void* result = null;
                pthread_join(thread, &result).ShouldBe(0);
                ((nint)result).ShouldBe(123);
                // Thread-local runtime state does not leak onto the joining thread.
                errno.ShouldBe(0);
            }
        }
        origin.__DotCcRuntime.Dispose();
    }
}

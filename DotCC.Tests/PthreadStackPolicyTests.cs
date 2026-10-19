#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PthreadStackPolicyTests
{
    private const int Minimum = 64 * 1024 * 1024;
    private static readonly AsyncLocal<int> Flow = new();
    private sealed class Owner : IProgramInstance, IDisposable
    {
        public RuntimeContext __DotCcRuntime { get; } = new();
        internal int Requested, SeenFlow, SeenContext, DestructorCalls;
        internal ulong Physical;
        internal int Key;
        internal bool Descendant;
        internal readonly ManualResetEventSlim Entered = new(), Leave = new();
        public void Dispose() { __DotCcRuntime.Dispose(); Entered.Dispose(); Leave.Dispose(); }
    }
    [DllImport("libc", EntryPoint = "pthread_self")] private static extern nuint NativeSelf();
    [DllImport("libc", EntryPoint = "pthread_getattr_np")] private static extern int NativeGetAttributes(nuint thread, void* attributes);
    [DllImport("libc", EntryPoint = "pthread_attr_getstack")] private static extern int NativeGetStack(void* attributes, void** address, nuint* size);
    [DllImport("libc", EntryPoint = "pthread_attr_destroy")] private static extern int NativeDestroy(void* attributes);

    private static ulong PhysicalStack()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64) return 0;
        // Opaque host pthread_attr_t (56 bytes on the selected Linux x64 ABI),
        // intentionally distinct from dotcc's 4-byte logical attribute handle.
        byte* attributes = stackalloc byte[128];
        if (NativeGetAttributes(NativeSelf(), attributes) != 0) throw new InvalidOperationException("pthread_getattr_np failed");
        try
        {
            void* address;
            nuint size;
            if (NativeGetStack(attributes, &address, &size) != 0 || address == null) throw new InvalidOperationException("pthread_attr_getstack failed");
            return size;
        }
        finally { NativeDestroy(attributes).ShouldBe(0); }
    }
    private static void Observe(Owner owner)
    {
        RuntimeContext context = CaptureRuntimeContext();
        owner.SeenContext = ReferenceEquals(context, owner.__DotCcRuntime) ? 1 : -1;
        owner.SeenFlow = Flow.Value;
        owner.Requested = context.Threads[pthread_self()].Admission.StackSize;
        owner.Physical = PhysicalStack();
        SynchronizationContext.Current.ShouldBeNull();
    }
    private static void* StaticStart(void* argument)
    {
        var owner = (Owner)GCHandle.FromIntPtr((nint)argument).Target!;
        Observe(owner);
        return argument;
    }
    private static void* InstanceStart(Owner owner, void* argument) { Observe(owner); return argument; }
    private static void* BlockingStart(Owner owner, void* argument)
    {
        Observe(owner);
        if (pthread_setspecific(owner.Key, argument) != 0) throw new InvalidOperationException("TLS failed");
        owner.Entered.Set();
        if (!owner.Leave.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
        return argument;
    }
    private static void Destructor(void* argument)
    {
        var owner = (Owner)GCHandle.FromIntPtr((nint)argument).Target!;
        Interlocked.Increment(ref owner.DestructorCalls);
        if (owner.Descendant)
        {
            long child;
            pthread_create(owner, &child, null, &InstanceStart, null).ShouldBe(0);
            pthread_join(child, null).ShouldBe(0);
        }
    }
    private static void Verify(Owner owner, int expected)
    {
        owner.Requested.ShouldBe(expected);
        owner.SeenContext.ShouldBe(1);
        owner.SeenFlow.ShouldBe(0);
        if (OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64)
            owner.Physical.ShouldBeGreaterThanOrEqualTo((ulong)expected);
        Console.WriteLine($"stack-policy requested={owner.Requested} Linux-physical={owner.Physical}");
    }

    [Fact]
    public void Logical_attributes_stay_unchanged_while_both_creation_routes_use_the_target_minimum()
    {
        using var owner = new Owner(); using var unrelated = new Owner();
        using var scope = owner.__DotCcRuntime.UseMinimumPthreadStackSize(Minimum);
        Flow.Value = 97;
        var handle = GCHandle.Alloc(owner);
        try
        {
            int attribute;
            using (owner.__DotCcRuntime.Enter()) pthread_attr_init(&attribute).ShouldBe(0);
            foreach (int requested in new[] { 0, DOTCC_PTHREAD_DEFAULT_STACK_SIZE, PTHREAD_STACK_MIN, Minimum + 1024 * 1024 })
            {
                using (owner.__DotCcRuntime.Enter())
                {
                    if (requested != 0) pthread_attr_setstacksize(&attribute, (ulong)requested).ShouldBe(0);
                    ulong logical;
                    pthread_attr_getstacksize(&attribute, &logical).ShouldBe(0);
                    logical.ShouldBe((ulong)(requested == 0 ? DOTCC_PTHREAD_DEFAULT_STACK_SIZE : requested));
                    long thread;
                    using (ExecutionContext.SuppressFlow())
                        pthread_create(&thread, requested == 0 ? null : &attribute, &StaticStart, (void*)GCHandle.ToIntPtr(handle)).ShouldBe(0);
                    pthread_join(thread, null).ShouldBe(0);
                }
                Verify(owner, Math.Max(requested == 0 ? DOTCC_PTHREAD_DEFAULT_STACK_SIZE : requested, Minimum));
                using (unrelated.__DotCcRuntime.Enter())
                {
                    long thread;
                    pthread_create(owner, &thread, requested == 0 ? null : &attribute, &InstanceStart, null).ShouldBe(0);
                    ReferenceEquals(RuntimeContext.Current, unrelated.__DotCcRuntime).ShouldBeTrue();
                    using (owner.__DotCcRuntime.Enter()) pthread_join(thread, null).ShouldBe(0);
                }
                Verify(owner, Math.Max(requested == 0 ? DOTCC_PTHREAD_DEFAULT_STACK_SIZE : requested, Minimum));
            }
            using (owner.__DotCcRuntime.Enter())
            {
                int invalid = -1;
                long thread = 79;
                pthread_create(&thread, &invalid, &StaticStart, null).ShouldBe(EINVAL);
                thread.ShouldBe(79);
                pthread_attr_setstacksize(&attribute, PTHREAD_STACK_MIN - 1).ShouldBe(EINVAL);
                pthread_attr_setstacksize(&attribute, (ulong)int.MaxValue + 1).ShouldBe(EINVAL);
                pthread_attr_destroy(&attribute).ShouldBe(0);
            }
        }
        finally { handle.Free(); Flow.Value = 0; }
    }

    [Fact]
    public void Retirement_retains_descendants_through_TLS_destructors_and_allows_replacement_only_after_drain()
    {
        using var owner = new Owner { Descendant = true };
        var handle = GCHandle.Alloc(owner);
        var policy = owner.__DotCcRuntime.UseMinimumPthreadStackSize(Minimum);
        using var binding = owner.__DotCcRuntime.Enter();
        try
        {
            int key;
            pthread_key_create(&key, &Destructor).ShouldBe(0);
            owner.Key = key;
            long worker;
            pthread_create(owner, &worker, null, &BlockingStart, (void*)GCHandle.ToIntPtr(handle)).ShouldBe(0);
            owner.Entered.Wait(TimeSpan.FromSeconds(5)).ShouldBeTrue();
            policy.Dispose(); policy.Dispose();
            Should.Throw<InvalidOperationException>(() => owner.__DotCcRuntime.UseMinimumPthreadStackSize(Minimum));
            Should.Throw<InvalidOperationException>(() => owner.__DotCcRuntime.Dispose());
            owner.Leave.Set();
            pthread_join(worker, null).ShouldBe(0);
            owner.DestructorCalls.ShouldBe(1);
            Verify(owner, Minimum);
            pthread_key_delete(key).ShouldBe(0);
            using var next = owner.__DotCcRuntime.UseMinimumPthreadStackSize(Minimum);
        }
        finally { owner.Leave.Set(); policy.Dispose(); handle.Free(); }
    }

    [Fact]
    public void Admission_rollback_and_scope_disposal_are_idempotent_and_keep_context_generation_isolated()
    {
        using var owner = new Owner(); using var unrelated = new Owner();
        Should.Throw<ArgumentOutOfRangeException>(() => owner.__DotCcRuntime.UseMinimumPthreadStackSize(PTHREAD_STACK_MIN - 1));
        var policy = owner.__DotCcRuntime.UseMinimumPthreadStackSize(Minimum);
        var admission = owner.__DotCcRuntime.AdmitPthread(PTHREAD_STACK_MIN);
        admission.StackSize.ShouldBe(Minimum);
        policy.Dispose(); policy.Dispose();
        Should.Throw<InvalidOperationException>(() => owner.__DotCcRuntime.UseMinimumPthreadStackSize(Minimum));
        admission.Dispose(); admission.Dispose(); // Same rollback used for allocation/constructor/Start failures.
        using (var next = owner.__DotCcRuntime.UseMinimumPthreadStackSize(Minimum)) { }
        using (var defaultAdmission = owner.__DotCcRuntime.AdmitPthread(DOTCC_PTHREAD_DEFAULT_STACK_SIZE))
        {
            defaultAdmission.StackSize.ShouldBe(DOTCC_PTHREAD_DEFAULT_STACK_SIZE);
            Should.Throw<InvalidOperationException>(() => owner.__DotCcRuntime.UseMinimumPthreadStackSize(Minimum));
        }
        using (var independent = unrelated.__DotCcRuntime.AdmitPthread(DOTCC_PTHREAD_DEFAULT_STACK_SIZE))
            independent.StackSize.ShouldBe(DOTCC_PTHREAD_DEFAULT_STACK_SIZE);
        owner.__DotCcRuntime.Dispose();
        Should.Throw<ObjectDisposedException>(() => owner.__DotCcRuntime.UseMinimumPthreadStackSize(Minimum));
        Should.Throw<ObjectDisposedException>(() => owner.__DotCcRuntime.AdmitPthread(DOTCC_PTHREAD_DEFAULT_STACK_SIZE));
    }

    [Fact]
    public void Detached_workers_retain_retired_policy_until_destructors_and_context_cleanup_finish()
    {
        using var owner = new Owner { Descendant = true };
        var handle = GCHandle.Alloc(owner);
        using var binding = owner.__DotCcRuntime.Enter();
        var policy = owner.__DotCcRuntime.UseMinimumPthreadStackSize(Minimum);
        try
        {
            int key;
            pthread_key_create(&key, &Destructor).ShouldBe(0);
            owner.Key = key;
            long worker;
            pthread_create(owner, &worker, null, &BlockingStart, (void*)GCHandle.ToIntPtr(handle)).ShouldBe(0);
            owner.Entered.Wait(TimeSpan.FromSeconds(5)).ShouldBeTrue();
            var admitted = owner.__DotCcRuntime.Threads[worker];
            pthread_detach(worker).ShouldBe(0);
            pthread_join(worker, null).ShouldBe(EINVAL);
            policy.Dispose();
            owner.Leave.Set();
            admitted.Thread.Join(TimeSpan.FromSeconds(5)).ShouldBeTrue();
            owner.__DotCcRuntime.Threads.ContainsKey(worker).ShouldBeFalse();
            owner.DestructorCalls.ShouldBe(1);
            Verify(owner, Minimum);
            pthread_key_delete(key).ShouldBe(0);
            using var next = owner.__DotCcRuntime.UseMinimumPthreadStackSize(Minimum);
        }
        finally { owner.Leave.Set(); policy.Dispose(); handle.Free(); }
    }

    [Fact]
    public void Unrelated_default_and_legacy_fault_boundary_remain_unchanged()
    {
        RuntimeContext legacy = CaptureRuntimeContext();
        ReferenceEquals(legacy, CaptureRuntimeContext()).ShouldBeTrue();
        long thread;
        // Block long enough to inspect the existing fault-boundary selection.
        using var release = new ManualResetEventSlim();
        var handle = GCHandle.Alloc(release);
        try
        {
            pthread_create(&thread, null, &LegacyBlocked, (void*)GCHandle.ToIntPtr(handle)).ShouldBe(0);
            var state = legacy.Threads[thread];
            state.Context.ShouldBeNull();
            ReferenceEquals(state.OwnerContext, legacy).ShouldBeTrue();
            state.Admission.StackSize.ShouldBe(DOTCC_PTHREAD_DEFAULT_STACK_SIZE);
            release.Set();
            pthread_join(thread, null).ShouldBe(0);
        }
        finally { release.Set(); handle.Free(); }
    }
    [Fact]
    public void Legacy_binding_stays_null_and_raw_heap_ownership_remains_cross_thread()
    {
        RuntimeContext.Current.ShouldBeNull();
        void* allocation = malloc(32);
        ((nint)allocation).ShouldNotBe(0);
        long thread;
        pthread_create(&thread, null, &LegacyHeapTransfer, allocation).ShouldBe(0);
        void* result = null;
        pthread_join(thread, &result).ShouldBe(0);
        if (result == null) free(allocation); // The old candidate never consumed it.
        ((nint)result).ShouldNotBe(0, "legacy workers must keep Current null and permit existing unowned heap transfers");
        free(result);
        RuntimeContext.Current.ShouldBeNull();
    }
    private static void* LegacyHeapTransfer(void* allocation)
    {
        // Return a failure value rather than throwing on an unowned legacy
        // worker: the regression must fail normally, not crash its test process.
        if (RuntimeContext.Current != null) return null;
        byte* cwd = getcwd(null, 0);
        if (cwd == null) return null;
        bool correctPath = Marshal.PtrToStringUTF8((nint)cwd) == System.IO.Directory.GetCurrentDirectory();
        free(cwd);
        if (!correctPath) return null;
        free(allocation);
        return malloc(32);
    }
    private static void* LegacyBlocked(void* argument)
    {
        ((ManualResetEventSlim)GCHandle.FromIntPtr((nint)argument).Target!).Wait(TimeSpan.FromSeconds(5)).ShouldBeTrue();
        return null;
    }
}

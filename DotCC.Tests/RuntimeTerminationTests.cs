using System;
using System.Runtime.InteropServices;
using System.Threading;
using Shouldly;
using Xunit;
using RuntimeLibc = DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class RuntimeTerminationTests
{
    private static void Request(int operation, int status)
    {
        switch (operation)
        {
            case 0: RuntimeLibc.exit(status); break;
            case 1: RuntimeLibc._exit(status); break;
            case 2: RuntimeLibc._Exit(status); break;
            case 3: RuntimeLibc.abort(); break;
            default: throw new ArgumentOutOfRangeException(nameof(operation));
        }
    }

    [Theory]
    [InlineData(0, RuntimeLibc.RuntimeTerminationKind.Exit)]
    [InlineData(1, RuntimeLibc.RuntimeTerminationKind.ImmediateExit)]
    [InlineData(2, RuntimeLibc.RuntimeTerminationKind.ImmediateExit)]
    [InlineData(3, RuntimeLibc.RuntimeTerminationKind.Abort)]
    public void Bound_termination_unwinds_only_its_owner_and_notifies_host(int operation, RuntimeLibc.RuntimeTerminationKind kind)
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var other = new RuntimeLibc.RuntimeContext();
        using (other.Enter())
        {
            using (owner.Enter())
            {
                bool continued = false;
                var exception = Should.Throw<RuntimeLibc.RuntimeTerminationException>(() =>
                {
                    Request(operation, 37);
                    continued = true;
                });
                continued.ShouldBeFalse();
                exception.Owner.ShouldBeSameAs(owner);
                exception.Termination.ShouldBeSameAs(owner.Termination);
                exception.Termination.Kind.ShouldBe(kind);
                exception.Termination.Status.ShouldBe(operation == 3 ? 0 : 37);
                exception.Termination.ManagedThreadId.ShouldBe(Environment.CurrentManagedThreadId);
                exception.Termination.Fault.ShouldBeNull();
                owner.TerminationTask.IsCompletedSuccessfully.ShouldBeTrue();
                owner.TerminationTask.Result.ShouldBeSameAs(exception.Termination);
            }
            RuntimeLibc.RuntimeContext.Current.ShouldBeSameAs(other);
            other.Termination.ShouldBeNull();
            other.TerminationTask.IsCompleted.ShouldBeFalse();
            void* survivor = RuntimeLibc.malloc(16);
            RuntimeLibc.free(survivor);
        }
    }

    [Fact]
    public void First_terminal_request_is_preserved_during_later_cleanup_failures()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            Should.Throw<RuntimeLibc.RuntimeTerminationException>(() => RuntimeLibc.exit(7));
            var original = owner.Termination;
            Should.Throw<RuntimeLibc.RuntimeTerminationException>(RuntimeLibc.abort);
            owner.RecordWorkerFailure(new InvalidOperationException("later cleanup failure"));
            owner.Termination.ShouldBeSameAs(original);
            owner.Termination!.Status.ShouldBe(7);
            owner.TerminationTask.Result.ShouldBeSameAs(original);
        }
    }

    private static void* TerminatingWorker(void* argument)
    {
        int* markers = (int*)argument;
        markers[1] = 1;
        Request(markers[0], 23);
        markers[2] = 1;
        return null;
    }

    [Theory]
    [InlineData(0, RuntimeLibc.RuntimeTerminationKind.Exit)]
    [InlineData(1, RuntimeLibc.RuntimeTerminationKind.ImmediateExit)]
    [InlineData(2, RuntimeLibc.RuntimeTerminationKind.ImmediateExit)]
    [InlineData(3, RuntimeLibc.RuntimeTerminationKind.Abort)]
    public void Pthread_terminal_request_ends_worker_without_resuming_C(int operation, RuntimeLibc.RuntimeTerminationKind kind)
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            int* markers = stackalloc int[] { operation, 0, 0 };
            long thread;
            RuntimeLibc.pthread_create(&thread, null, &TerminatingWorker, markers).ShouldBe(0);
            RuntimeLibc.pthread_join(thread, null).ShouldBe(0);
            markers[1].ShouldBe(1);
            markers[2].ShouldBe(0);
            owner.Termination.ShouldNotBeNull();
            owner.Termination!.Kind.ShouldBe(kind);
            owner.Termination.Status.ShouldBe(operation == 3 ? 0 : 23);
            owner.Termination.ManagedThreadId.ShouldNotBe(Environment.CurrentManagedThreadId);
            owner.TerminationTask.IsCompletedSuccessfully.ShouldBeTrue();
        }
        owner.Dispose(); // Both the worker retention and its binding were released.
    }

    private static void* FaultingWorker(void* argument) => throw new InvalidOperationException("original worker failure");

    [Fact]
    public void Worker_clr_fault_reaches_owner_with_original_exception_and_stack()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            long thread;
            RuntimeLibc.pthread_create(&thread, null, &FaultingWorker, null).ShouldBe(0);
            RuntimeLibc.pthread_join(thread, null).ShouldBe(0);
            owner.Termination!.Kind.ShouldBe(RuntimeLibc.RuntimeTerminationKind.WorkerFault);
            var failure = owner.Termination.Fault.ShouldBeOfType<InvalidOperationException>();
            failure.Message.ShouldBe("original worker failure");
            failure.StackTrace.ShouldContain(nameof(FaultingWorker));
            owner.TerminationTask.Result.Fault.ShouldBeSameAs(failure);
        }
    }

    private static void DestructorFault(void* ignored) => throw new InvalidOperationException("destructor failure");
    private static void* InstallDestructor(void* argument)
    {
        RuntimeLibc.pthread_setspecific(*(int*)argument, (void*)1);
        return null;
    }

    [Fact]
    public void Worker_destructor_fault_is_also_contained_at_outer_boundary()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            int key;
            RuntimeLibc.pthread_key_create(&key, &DestructorFault).ShouldBe(0);
            long thread;
            RuntimeLibc.pthread_create(&thread, null, &InstallDestructor, &key).ShouldBe(0);
            RuntimeLibc.pthread_join(thread, null).ShouldBe(0);
            owner.Termination!.Kind.ShouldBe(RuntimeLibc.RuntimeTerminationKind.WorkerFault);
            owner.Termination.Fault!.Message.ShouldBe("destructor failure");
            RuntimeLibc.pthread_key_delete(key).ShouldBe(0);
        }
    }

    private static void* NormalThreadExit(void* ignored)
    {
        RuntimeLibc.pthread_exit((void*)42);
        return null;
    }

    [Fact]
    public void Ordinary_pthread_exit_is_not_a_program_termination()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            long thread;
            RuntimeLibc.pthread_create(&thread, null, &NormalThreadExit, null).ShouldBe(0);
            void* result;
            RuntimeLibc.pthread_join(thread, &result).ShouldBe(0);
            ((nint)result).ShouldBe((nint)42);
            owner.Termination.ShouldBeNull();
        }
    }

    private static int TerminatingC11Worker(void* ignored)
    {
        RuntimeLibc.malloc(16); // Must be retained by the creating program too.
        RuntimeLibc.exit(19);
        return 77;
    }
    private static int FaultingC11Worker(void* ignored) => throw new InvalidOperationException("C11 worker failure");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void C11_workers_inherit_owner_for_heap_and_terminal_errors(bool fault)
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            RuntimeLibc.thrd_t thread;
            RuntimeLibc.thrd_create(&thread, fault ? &FaultingC11Worker : &TerminatingC11Worker, null).ShouldBe(RuntimeLibc.thrd_success);
            RuntimeLibc.thrd_join(thread, null).ShouldBe(RuntimeLibc.thrd_success);
            owner.Termination!.Kind.ShouldBe(fault ? RuntimeLibc.RuntimeTerminationKind.WorkerFault : RuntimeLibc.RuntimeTerminationKind.Exit);
            if (fault) owner.Termination.Fault!.Message.ShouldBe("C11 worker failure");
            else { owner.Termination.Status.ShouldBe(19); owner.NativeAllocations.Count.ShouldBe(1); }
        }
        owner.Dispose();
        owner.NativeAllocations.ShouldBeEmpty();
    }

    private sealed class Gate : IDisposable
    {
        internal readonly ManualResetEventSlim Ready = new(false), Release = new(false);
        public void Dispose() { Ready.Dispose(); Release.Dispose(); }
    }
    private static void* WaitingWorker(void* argument)
    {
        var gate = (Gate)GCHandle.FromIntPtr((nint)argument).Target!;
        gate.Ready.Set();
        gate.Release.Wait(TimeSpan.FromSeconds(10));
        return null;
    }

    [Fact]
    public void Recorded_termination_does_not_allow_disposal_while_another_worker_is_live()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using var gate = new Gate();
        var handle = GCHandle.Alloc(gate);
        long thread = 0;
        try
        {
            using (owner.Enter())
            {
                RuntimeLibc.pthread_create(&thread, null, &WaitingWorker, (void*)GCHandle.ToIntPtr(handle)).ShouldBe(0);
                gate.Ready.Wait(TimeSpan.FromSeconds(10)).ShouldBeTrue();
                Should.Throw<RuntimeLibc.RuntimeTerminationException>(() => RuntimeLibc.exit(3));
            }
            owner.Termination!.Status.ShouldBe(3);
            Should.Throw<InvalidOperationException>(owner.Dispose);
        }
        finally
        {
            gate.Release.Set();
            if (thread != 0) using (owner.Enter()) RuntimeLibc.pthread_join(thread, null).ShouldBe(0);
            handle.Free();
        }
        owner.Dispose();
    }
}

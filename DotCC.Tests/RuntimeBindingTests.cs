using System;
using System.Threading;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed class RuntimeBindingTests
{
    [Fact]
    public void Enter_and_dispose_allocate_no_managed_memory()
    {
        using var context = new RuntimeContext();
        for (int i = 0; i < 100; ++i)
        {
            using var scope = context.Enter();
        }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; ++i)
        {
            using var scope = context.Enter();
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        allocated.ShouldBe(0);
    }

    [Fact]
    public void Nested_scopes_restore_context_after_exception_and_refuse_live_retirement()
    {
        using var first = new RuntimeContext();
        using var second = new RuntimeContext();
        var original = RuntimeContext.Current;
        using (first.Enter())
        {
            Should.Throw<InvalidOperationException>(() => first.Dispose());
            try
            {
                using var scope = second.Enter();
                RuntimeContext.Current.ShouldBeSameAs(second);
                throw new ApplicationException();
            }
            catch (ApplicationException) { }
            RuntimeContext.Current.ShouldBeSameAs(first);
        }
        RuntimeContext.Current.ShouldBeSameAs(original);
    }

    [Fact]
    public void Recursive_scopes_reject_wrong_order_and_stale_copies_without_releasing_context()
    {
        using var context = new RuntimeContext();
        var outer = context.Enter();
        var inner = context.Enter();
        var copy = inner;
        Should.Throw<InvalidOperationException>(() => outer.Dispose());
        inner.Dispose();
        inner.Dispose();
        var newer = context.Enter();
        Should.Throw<InvalidOperationException>(() => copy.Dispose());
        Should.Throw<InvalidOperationException>(() => context.Dispose());
        RuntimeContext.Current.ShouldBeSameAs(context);
        newer.Dispose();
        outer.Dispose();
        default(RuntimeBinding).Dispose();
        context.Dispose();
        Should.Throw<ObjectDisposedException>(() => context.Enter());
    }

    [Fact]
    public void Wrong_thread_disposal_leaves_scope_active_for_its_owner()
    {
        using var context = new RuntimeContext();
        var scope = context.Enter();
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { scope.Dispose(); }
            catch (Exception error) { failure = error; }
        });
        thread.Start();
        thread.Join();
        failure.ShouldBeOfType<InvalidOperationException>();
        RuntimeContext.Current.ShouldBeSameAs(context);
        Should.Throw<InvalidOperationException>(() => context.Dispose());
        scope.Dispose();
    }
}

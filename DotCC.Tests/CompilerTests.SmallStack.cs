#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed partial class CompilerTests
{
    // Exercise the compiler on an explicitly sized stack on every platform.
    // A regression can terminate the test host (StackOverflowException cannot
    // be caught), which is preferable to silently testing Linux's larger stack.
    private static void RunOnSmallStack(Action action)
    {
        ExceptionDispatchInfo? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception error) { failure = ExceptionDispatchInfo.Capture(error); }
        }, 1024 * 1024) { IsBackground = true };
        thread.Start();
        thread.Join(TimeSpan.FromMinutes(2)).ShouldBeTrue("small-stack compiler check timed out");
        failure?.Throw();
    }

    [Fact]
    public void Small_stack_preserves_large_struct_member_order_and_layout()
    {
        const int count = 1000;
        var source = WriteTemp("struct S {" + string.Concat(Enumerable.Range(0, count).Select(i => $"int f{i};"))
            + "}; _Static_assert(sizeof(struct S) == 4000, \"size\");"
            + "_Static_assert(offsetof(struct S, f999) == 3996, \"last\");");
        try
        {
            RunOnSmallStack(() =>
            {
                var ir = new CFrontend().BuildIr(new FrontendRequest(new[] { source }));
                ir.Types.Single(t => t.Name == "S").Fields.Select(f => f.Name)
                    .ShouldBe(Enumerable.Range(0, count).Select(i => $"f{i}"));
            });
        }
        finally { File.Delete(source); }
    }

    [Fact]
    public void Small_stack_preserves_large_positional_initializer_order()
    {
        const int count = 12000;
        var source = WriteTemp("int f(void) { int data[] = {"
            + string.Join(",", Enumerable.Range(0, count)) + ",}; return data[11999]; }");
        try
        {
            RunOnSmallStack(() =>
            {
                var ir = new CFrontend().BuildIr(new FrontendRequest(new[] { source }));
                var array = ir.Functions.Single().Body.Stmts.OfType<ArrayDecl>().Single();
                array.Inits!.Select(value => value.ShouldBeOfType<LitInt>().Value)
                    .ShouldBe(Enumerable.Range(0, count).Select(i => (long?)i));
            });
        }
        finally { File.Delete(source); }
    }

    [Fact]
    public void Small_stack_preserves_last_designator_wins()
    {
        var source = WriteTemp("struct S { int f; }; struct S value = {"
            + string.Join(",", Enumerable.Range(0, 12000).Select(i => $".f = {i}")) + ",};");
        try
        {
            RunOnSmallStack(() =>
            {
                var ir = new CFrontend().BuildIr(new FrontendRequest(new[] { source }));
                var member = ir.Globals.Single().Init.ShouldBeOfType<StructInit>().Members.Single();
                member.Name.ShouldBe("f");
                member.Value.ShouldBeOfType<LitInt>().Value.ShouldBe(11999);
            });
        }
        finally { File.Delete(source); }
    }
}

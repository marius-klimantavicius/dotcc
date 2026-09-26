using System;
using System.IO;
using System.Linq;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed partial class CompilerTests
{
    [Fact]
    public void Small_stack_large_repeated_global_initializer_preserves_deduplication()
    {
        var definition = "static const int table[] = {" + string.Join(",", Enumerable.Range(0, 12000)) + "};";
        var first = WriteTemp(definition + "int first(void) { return table[0]; } int main(void) { return first(); }");
        var second = WriteTemp("\n\n" + definition + "int last(void) { return table[11999]; }");
        try
        {
            RunOnSmallStack(() =>
            {
                var ir = new CFrontend().BuildIr(new FrontendRequest(new[] { first, second }));
                // A repeated header table owns one backing store, preserving
                // every initializer in order across the duplicate definition.
                var table = ir.Globals.Single();
                table.Sym.Name.ShouldBe("table");
                table.Init.ShouldBeOfType<PinnedArray>().Elems!
                    .Select(value => value.ShouldBeOfType<LitInt>().Value)
                    .ShouldBe(Enumerable.Range(0, 12000).Select(value => (long?)value));
                ir.Functions.Select(function => function.Sym.Name).ShouldBe(new[] { "first", "main", "last" });
            });
        }
        finally { File.Delete(first); File.Delete(second); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Small_stack_long_function_identity_distinguishes_changed_tokens(bool changed)
    {
        var statements = string.Concat(Enumerable.Repeat("x += 1;", 12000));
        var first = WriteTemp("int shared(int x) {" + statements + "return x; } int main(void) { return shared(0); }");
        var second = WriteTemp("\n\nint shared(int x) {" + statements + (changed ? "return x + 1; }" : "return x; }"));
        try
        {
            RunOnSmallStack(() =>
            {
                if (changed)
                    Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { first, second }))
                        .Message.ShouldContain("duplicate definition of external function 'shared'");
                else
                {
                    var emitted = Compiler.EmitCSharp(new[] { first, second });
                    emitted.Split("int shared(", StringSplitOptions.None).Length.ShouldBe(2);
                    emitted.Split("x += 1;", StringSplitOptions.None).Length.ShouldBe(12001);
                }
            });
        }
        finally { File.Delete(first); File.Delete(second); }
    }
}

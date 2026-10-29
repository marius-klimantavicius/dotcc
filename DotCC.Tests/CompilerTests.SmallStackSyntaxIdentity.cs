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
    public void Small_stack_large_repeated_global_initializer_preserves_per_unit_storage()
    {
        var definition = "static const int table[] = {" + string.Join(",", Enumerable.Range(0, 12000)) + "};";
        var first = WriteTemp(definition + "int first(void) { return table[0]; } int main(void) { return first(); }");
        var second = WriteTemp("\n\n" + definition + "int last(void) { return table[11999]; }");
        try
        {
            RunOnSmallStack(() =>
            {
                var ir = new CFrontend().BuildIr(new FrontendRequest(new[] { first, second }));
                // Each translation unit owns its header table, including every
                // initializer in order, even when the token sequences match.
                var tables = ir.Globals.ToArray();
                tables.Length.ShouldBe(2);
                tables[0].Sym.ShouldNotBeSameAs(tables[1].Sym);
                tables[0].Sym.TargetName.ShouldNotBe(tables[1].Sym.TargetName);
                foreach (var table in tables)
                {
                    table.Sym.Name.ShouldBe("table");
                    ir.InternalGlobals.ShouldContain(table.Sym);
                    table.Sym.Type.ShouldBeOfType<CType.Array>().Count.ShouldBe(12000);
                    table.Init.ShouldBeOfType<PinnedArray>().Elems!
                        .Select(value => value.ShouldBeOfType<LitInt>().Value)
                        .ShouldBe(Enumerable.Range(0, 12000).Select(value => (long?)value));
                }
                foreach (var (name, table) in new[] { ("first", tables[0]), ("last", tables[1]) })
                {
                    var function = ir.Functions.Single(function => function.Sym.Name == name);
                    var index = function.Body.Stmts.OfType<Return>().Single().Value.ShouldBeOfType<DotCC.Ir.Index>();
                    index.Base.ShouldBeOfType<VarRef>().Sym.ShouldBeSameAs(table.Sym);
                }
                ir.Functions.Select(function => function.Sym.Name).ShouldBe(new[] { "first", "main", "last" });
            });
        }
        finally { File.Delete(first); File.Delete(second); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Small_stack_large_same_unit_array_declarations_preserve_storage_and_reject_redefinition(bool redefine)
    {
        var definition = "static const int table[] = {" + string.Join(",", Enumerable.Range(0, 12000)) + "};";
        var source = WriteTemp("static const int table[12000]; const int *early(void) { return table; }"
            + definition + "extern const int table[12000]; static const int table[12000];"
            + (redefine ? definition : "") + "const int *late(void) { return table; }");
        try
        {
            RunOnSmallStack(() =>
            {
                if (redefine)
                {
                    Should.Throw<CompileException>(() => new CFrontend().BuildIr(new FrontendRequest([source])))
                        .Message.ShouldContain("redefinition of global 'table'");
                    return;
                }
                var ir = new CFrontend().BuildIr(new FrontendRequest([source]));
                var table = ir.Globals.Single();
                ir.InternalGlobals.ShouldContain(table.Sym);
                table.Sym.Type.ShouldBeOfType<CType.Array>().Count.ShouldBe(12000);
                table.Init.ShouldBeOfType<PinnedArray>().Elems!
                    .Select(value => value.ShouldBeOfType<LitInt>().Value)
                    .ShouldBe(Enumerable.Range(0, 12000).Select(value => (long?)value));
                ir.Functions.Count.ShouldBe(2);
                foreach (var function in ir.Functions)
                    function.Body.Stmts.OfType<Return>().Single().Value.ShouldBeOfType<VarRef>()
                        .Sym.ShouldBeSameAs(table.Sym);
            });
        }
        finally { File.Delete(source); }
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

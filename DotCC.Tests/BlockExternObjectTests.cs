using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class BlockExternObjectTests
{
    private static string Emit(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-block-extern-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try { return Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib); }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Block_declaration_before_definition_shares_one_object_and_no_local_storage()
    {
        var output = Emit("int *address(void) { extern int shared; return &shared; } int shared = 17; int main(void) { return *address(); }");
        Assert.Equal(1, output.Split("public int shared;").Length - 1);
        Assert.DoesNotContain("int shared = default", output);
        Assert.Contains("Globals.shared", output);
    }

    [Fact]
    public void Declaration_only_fragment_does_not_define_or_rename_the_external_object()
    {
        var output = Emit("int read(void) { int shared = 1; { extern int shared; return shared; } }");
        Assert.DoesNotContain("public int shared;", output);
        Assert.DoesNotContain("shared_1", output);
    }

    [Theory]
    [InlineData("int value; int read(void) { extern long value; return 0; }")]
    [InlineData("int read(void) { extern int value; { extern long value; } return 0; }")]
    [InlineData("int value(void); int read(void) { extern int value; return 0; }")]
    [InlineData("int read(void) { extern int value = 1; return value; }")]
    [InlineData("int read(void) { extern int value; return value; } long value;")]
    public void Conflicting_or_initialized_block_extern_objects_are_rejected(string source)
        => Assert.ThrowsAny<CompileException>(() => Emit(source));

    [Fact]
    public void Existing_block_function_prototype_remains_a_function()
    {
        var output = Emit("int read(void) { extern int calculate(int); return calculate(2); } int calculate(int n) { return n + 3; }");
        Assert.Contains("calculate(2)", output);
        Assert.DoesNotContain("delegate*<int, int> calculate", output);
    }

    [Fact]
    public void Pointer_to_array_cast_rejects_a_runtime_extent_explicitly()
        => Assert.ThrowsAny<CompileException>(() => Emit("int read(void *p, int n) { return ((int (*)[n])p)[1][0]; }"));
}

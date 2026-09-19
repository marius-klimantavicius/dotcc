#nullable enable
using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;
namespace DotCC.Tests;
public sealed class FlexibleArrayInitializerTests
{
    private static string Emit(string source)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-flexinit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.c");
        File.WriteAllText(path, source);
        try { return Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib); }
        finally { Directory.Delete(directory, true); }
    }
    [Theory]
    [InlineData("{ .count = 2, .values = {7, 9} }")]
    [InlineData("{ 2, {7, 9} }")]
    public void Static_flexible_tail_initializer_preserves_header_type(string initializer)
    {
        Emit("struct table { int count; int values[]; }; struct table value = " + initializer + "; int get(void) { return value.values[1] + sizeof(value); }")
            .ShouldContain("GlobalAlignedZeroed<byte>");
    }
    [Fact]
    public void Automatic_flexible_tail_initialization_remains_rejected()
    {
        Should.Throw<CompileException>(() => Emit("struct table { int count; int values[]; }; int get(void) { struct table value = { 2, {7, 9} }; return value.values[0]; }"))
            .Message.ShouldContain("flexible or zero-length");
    }
    [Fact]
    public void Explicit_zero_length_member_does_not_gain_flexible_storage()
    {
        Should.Throw<CompileException>(() => Emit("struct table { int count; int values[0]; }; struct table value = { 2, {7, 9} };"))
            .Message.ShouldContain("flexible or zero-length");
    }
}

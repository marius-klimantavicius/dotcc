#nullable enable

using System;
using System.IO;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigNoreturnTests
{
    [Fact]
    public void Bottom_is_distinct_from_void_and_has_no_storage_size()
    {
        CType.NoReturn.ShouldNotBe(CType.Void);
        CType.NoReturn.ShouldBeOfType<CType.NoReturnType>();
        Should.Throw<IrUnsupportedException>(() => _ = CType.NoReturn.SizeOf).Message.ShouldContain("noreturn");
    }

    [Theory]
    [InlineData("fn stop() noreturn {} pub fn main() u8 { stop(); }")]
    [InlineData("fn stop() noreturn { while (true) { break; } } pub fn main() u8 { stop(); }")]
    [InlineData("fn stop() noreturn { out: while (true) { while (true) { break :out; } } } pub fn main() u8 { stop(); }")]
    [InlineData("fn stop() noreturn { return; } pub fn main() u8 { stop(); }")]
    [InlineData("fn stop() noreturn { return 0; } pub fn main() u8 { stop(); }")]
    [InlineData("pub fn main() u8 { const value: noreturn = undefined; _ = value; return 42; }")]
    [InlineData("pub fn main() u8 { _ = @as(noreturn, {}); return 42; }")]
    [InlineData("pub fn main() u8 { return @intCast(@sizeOf(noreturn)); }")]
    [InlineData("fn consume(value: noreturn) void { _ = value; } pub fn main() u8 { consume(unreachable); }")]
    public void Uninhabited_values_cannot_be_materialized_or_returned(string source)
    {
        var error = Should.Throw<CompileException>(() => Emit(source));
        error.Message.ShouldContain("noreturn");
    }

    [Fact]
    public void Void_branch_does_not_coerce_as_bottom()
    {
        var error = Should.Throw<CompileException>(() => Emit("""
            fn nothing() void {}
            fn choose(ok: bool) u8 { return if (ok) 42 else nothing(); }
            pub fn main() u8 { return choose(true); }
            """));
        error.Message.ShouldContain("void");
    }

    private static string Emit(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-zig-noreturn-" + Guid.NewGuid().ToString("N") + ".zig");
        File.WriteAllText(path, source);
        try { return Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }
}

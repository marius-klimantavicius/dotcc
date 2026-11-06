using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigWrappingAssignmentTests
{
    [Theory]
    [InlineData("u3", "u5", "1")]
    [InlineData("u3", "i3", "1")]
    [InlineData("i3", "u3", "1")]
    [InlineData("u3", "u8", "255")]
    public void Incompatible_RHS_types_fail_instead_of_truncating(string target, string value, string initial)
        => WithSource($"fn bad(amount: {value}) {target} {{ var slot: {target} = 1; slot +%= amount; return slot; }} pub fn main() u8 {{ return @intCast(bad({initial})); }}", path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("cannot coerce"));

    [Theory]
    [InlineData("u3", "8")]
    [InlineData("u3", "-1")]
    [InlineData("i3", "4")]
    [InlineData("i3", "-5")]
    public void Non_fitting_literals_fail_instead_of_wrapping_the_RHS(string target, string value)
        => WithSource($"pub fn main() u8 {{ var slot: {target} = 1; slot +%= {value}; return @intCast(slot); }}", path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("does not fit"));

    [Theory]
    [InlineData("pub fn main() u8 { const slot: u3 = 1; slot +%= 1; return slot; }")]
    [InlineData("fn bad(slot: u3) u3 { slot +%= 1; return slot; } pub fn main() u8 { return bad(1); }")]
    [InlineData("fn bad(slot: *const u3) void { slot.* +%= 1; } pub fn main() u8 { const slot: u3 = 1; bad(&slot); return 0; }")]
    public void Wrapping_assignment_rejects_constant_storage(string source)
        => WithSource(source, path => Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("cannot assign to constant"));

    [Theory]
    [InlineData("fn bad(slot: *volatile u3) void { slot.* +%= 1; } pub fn main() u8 { var slot: u3 = 1; bad(&slot); return slot; }")]
    [InlineData("fn bad(slots: []volatile u3) void { slots[0] +%= 1; } pub fn main() u8 { var slots = [1]u3{1}; bad(slots[0..]); return slots[0]; }")]
    public void Narrow_volatile_storage_fails_explicitly(string source)
        => WithSource(source, path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("atomic/volatile"));

    [Fact]
    public void Statement_hoisting_does_not_reorder_the_old_value_read()
        => WithSource("fn value() ?u3 { return 1; } pub fn main() u8 { var slot: u3 = 1; slot +%= value() orelse return 0; return slot; }", path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("statement-hoisted operands"));

    private static void WithSource(string source, Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-wrapping-unit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { var path = Path.Combine(directory, "main.zig"); File.WriteAllText(path, source); test(path); }
        finally { Directory.Delete(directory, true); }
    }
}

using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigFieldWidthTests
{
    [Theory]
    [InlineData("var state: State = .{ .serial = 1 }; comptime { state.serial = 2; }")]
    [InlineData("comptime { const state: State = .{ .serial = 1 }; state.serial = 2; }")]
    public void Comptime_field_stores_require_a_mutable_comptime_owner(string body)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-field-store-" + Guid.NewGuid().ToString("N") + ".zig");
        File.WriteAllText(path, "const State = struct { serial: u16 };\n"
            + "pub fn main() u8 { " + body + " return 42; }\n");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path]))
                .Message.ShouldContain("assignment target must be a `comptime var`");
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("u64", "u48")]
    [InlineData("u48", "u47")]
    public void Runtime_field_reads_still_reject_real_narrowing(string source, string target)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-field-width-" + Guid.NewGuid().ToString("N") + ".zig");
        File.WriteAllText(path, $"const State = struct {{ serial: {source} }};\n"
            + $"fn copy(state: *const State) {target} {{ return state.serial; }}\n"
            + "pub fn main() u8 { const state: State = .{ .serial = 42 }; return @intCast(copy(&state)); }\n");
        try
        {
            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp([path]));
            error.Message.ShouldContain($"expected type '{target}', found '{source}'");
        }
        finally { File.Delete(path); }
    }
}

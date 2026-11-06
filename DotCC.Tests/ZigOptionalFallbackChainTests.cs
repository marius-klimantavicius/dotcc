using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigOptionalFallbackChainTests
{
    [Theory]
    [InlineData("fn bad(a: ?u8, b: ?bool) ?u8 { return a orelse b; } pub fn main() u8 { return bad(null, null) orelse 42; }")]
    [InlineData("pub fn main() u8 { const a: ?u8 = null; const b: ?bool = null; const x = a orelse b; _ = x; return 42; }")]
    [InlineData("fn choose(b: bool) ?bool { return b; } fn bad(a: ?u8, b: ?bool) ?u8 { return a orelse choose(b orelse return null); } pub fn main() u8 { return bad(null, null) orelse 42; }")]
    public void Incompatible_optional_payloads_fail_in_source_and_objects(string source)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-optional-chain-unit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, source);
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path])).Message.ShouldContain("incompatible");
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("incompatible");
        }
        finally { Directory.Delete(directory, true); }
    }
}

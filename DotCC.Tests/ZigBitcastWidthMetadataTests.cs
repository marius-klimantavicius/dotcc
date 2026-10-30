using System;
using System.IO;
using Shouldly;
using Xunit;
namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigBitcastWidthMetadataTests
{
    [Fact]
    public void A_branch_route_without_typed_metadata_fails_instead_of_guessing_carrier_width()
    {
        // Native-valid, but the switch helper is outside this release's inventory.
        const string source = "fn choose(b: bool) u32 { return switch (b) { true => @bitCast(@as(i32, -1)), false => @bitCast(@as(i32, 0)), }; } pub fn main() u8 { return if (choose(true) == 0xffffffff) 0 else 1; }";
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-width-unknown-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.zig");
        File.WriteAllText(path, source);
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain("logical");
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("logical");
        }
        finally { Directory.Delete(directory, true); }
    }
}

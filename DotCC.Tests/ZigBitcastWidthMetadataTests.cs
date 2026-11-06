using System;
using System.IO;
using Shouldly;
using Xunit;
namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigBitcastWidthMetadataTests
{
    [Fact]
    public void A_branch_route_keeps_source_widths_and_rejects_a_logical_mismatch()
    {
        // i24 and i32 share a carrier. The switch must retain the source widths,
        // so the invalid 24-to-32-bit bitcast still fails after branch propagation.
        const string source = "fn choose(b: bool) u32 { return switch (b) { true => @bitCast(@as(i24, -1)), false => @bitCast(@as(i24, 0)), }; } pub fn main() u8 { return if (choose(true) == 0xffffffff) 0 else 1; }";
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-width-unknown-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.zig");
        File.WriteAllText(path, source);
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain("equal logical");
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("equal logical");
        }
        finally { Directory.Delete(directory, true); }
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reflected_fields_reject_a_bitcast_between_distinct_widths_in_one_carrier(bool objects)
    {
        const string source = """
            fn Copy(comptime T: type) type {
                const info = @typeInfo(T).@"struct";
                var types: [info.field_names.len]type = undefined;
                inline for (info.field_types, &types) |Type, *out| { out.* = Type; }
                return @Struct(.auto, null, info.field_names, &types, &@splat(.{}));
            }
            const S = Copy(struct { value: u17 });
            pub fn main() u8 { const s: S = .{ .value = 1 }; const bad: u32 = @bitCast(s.value); return @intCast(bad); }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-reflected-width-reject-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.zig");
        File.WriteAllText(path, source);
        try
        {
            Should.Throw<CompileException>(() => objects ? Compiler.EmitObject(path) : Compiler.EmitCSharp([path]))
                .Message.ShouldContain("equal logical source and target bit widths");
        }
        finally { Directory.Delete(directory, true); }
    }

}

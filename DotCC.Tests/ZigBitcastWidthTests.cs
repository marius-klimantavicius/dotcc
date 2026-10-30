using System;
using System.Collections.Generic;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigBitcastWidthTests
{
    // Native Zig 0.17.0 controls and exact source hashes live under
    // libghostty/build/direct-bitcast-width-r37/native-results.json.
    public static IEnumerable<object[]> NativeInvalidSources()
    {
        foreach (var (logical, carrier) in new[] { (1, 8), (3, 8), (7, 8), (9, 16), (24, 32), (33, 64), (65, 128) })
        {
            yield return new object[] { $"pub fn main() u8 {{ const x: u{logical} = @bitCast(@as(u{carrier}, 1)); return @intCast(x); }}" };
            yield return new object[] { $"pub fn main() u8 {{ const x: u{carrier} = @bitCast(@as(u{logical}, 1)); return @intCast(x); }}" };
        }
        yield return new object[] { "const U = u24; pub fn main() u8 { const x: U = @bitCast(@as(u32, 1)); return @truncate(x); }" };
        yield return new object[] { "fn word() u24 { return @bitCast(@as(u32, 1)); } pub fn main() u8 { return @truncate(word()); }" };
        yield return new object[] { "fn take(x: u24) u8 { return @truncate(x); } pub fn main() u8 { return take(@bitCast(@as(u32, 1))); }" };
        yield return new object[] { "const H = struct { x: u24 }; pub fn main() u8 { const h: H = .{ .x = @bitCast(@as(u32, 1)) }; return @truncate(h.x); }" };
        yield return new object[] { "pub fn main() u8 { const a: [1]u24 = .{ @bitCast(@as(u32, 1)) }; return @truncate(a[0]); }" };
        yield return new object[] { "pub fn main() u8 { const x = @as(u24, @bitCast(@as(u32, 1))); return @truncate(x); }" };
        yield return new object[] { "pub fn main() u8 { var x: u24 = 0; x = @bitCast(@as(u32, 1)); return @truncate(x); }" };
        yield return new object[] { "pub fn main() u8 { const x: u24 = @as(u32, @bitCast(@as(u24, 1))); return @truncate(x); }" };
        yield return new object[] { "pub fn main() u8 { const a = [4]u8{1,2,3,4}; const x: u24 = @bitCast(a); return @truncate(x); }" };
        yield return new object[] { "pub fn main() u8 { const a = [3]u3{1,2,3}; const x: u24 = @bitCast(a); return @truncate(x); }" };
        yield return new object[] { "pub fn main() u8 { const x: u24 = @bitCast(@as(f32, 1.0)); return @truncate(x); }" };
        yield return new object[] { "pub fn main() u8 { const x: f32 = @bitCast(@as(u24, 1)); return if (x == 0.0) 0 else 1; }" };
        // Preserve R34's direct-width rejection; a C arithmetic promotion is a
        // distinct source witness and cannot excuse this direct u64 operand.
        yield return new object[] { "pub fn main() u8 { const x: u32 = @bitCast(@as(u64, 1)); return @truncate(x); }" };
    }

    [Theory]
    [MemberData(nameof(NativeInvalidSources))]
    public void Native_invalid_logical_widths_are_rejected_in_both_public_emission_routes(string source)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-bitcast-width-reject-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.zig");
        File.WriteAllText(path, source);
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain("@bitCast");
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("@bitCast");
        }
        finally { Directory.Delete(directory, true); }
    }
}

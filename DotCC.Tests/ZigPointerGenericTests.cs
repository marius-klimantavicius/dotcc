using System;
using System.IO;
using System.Linq;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigPointerGenericTests
{
    [Theory]
    [InlineData("pub fn main() u8 { const x: u32 = @bitCast(@as(u64, 1)); return @truncate(x); }")]
    [InlineData("pub fn main() u8 { const a = [_]u8{1,2,3,4}; const x: u32 = @bitCast(a[0..4]); return @truncate(x); }")]
    [InlineData("pub fn main() u8 { const a = [_]u8{1,2,3}; const x: u32 = @bitCast(a); return @truncate(x); }")]
    [InlineData("pub fn main() u8 { var a: u8 = 1; const x: u64 = @bitCast(&a); return @truncate(x); }")]
    [InlineData("pub fn main() u8 { const x: u32 = @bitCast(@as(u16, 1)); return @truncate(x); }")]
    [InlineData("pub fn main() u8 { return @bitCast(@as(u7, 1)); }")]
    [InlineData("pub fn main() u8 { const a = [_]u8{}; const x: u32 = @bitCast(a); return @truncate(x); }")]
    [InlineData("fn read(s: []const u8, n: usize) u32 { return @bitCast(s[0..n].*); } pub fn main() u8 { const a = [_]u8{1,2,3,4}; return @truncate(read(&a, 4)); }")]
    public void Native_invalid_bitcasts_are_rejected_before_emitting_a_data_load(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-pointer-generic-reject-" + Guid.NewGuid().ToString("N") + ".zig");
        File.WriteAllText(path, source);
        try { Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path })).Message.ShouldContain("@bitCast"); }
        finally { File.Delete(path); }
    }

    [Fact]
    public void A_slice_dereference_bitcast_retains_the_native_array_extent_in_ir()
    {
        const string source = "fn word(s: []const u8) u32 { return @bitCast(s[0..4].*); } pub fn main() u8 { return @truncate(word(\"abcd\")); }";
        var path = Path.Combine(Path.GetTempPath(), "dotcc-pointer-generic-ir-" + Guid.NewGuid().ToString("N") + ".zig");
        File.WriteAllText(path, source);
        try
        {
            var ir = new ZigFrontend().BuildIr(new FrontendRequest(new[] { path }));
            var function = ir.Functions.Single(f => f.Sym.Name == "word");
            var value = function.Body.Stmts.OfType<Return>().Single().Value.ShouldBeOfType<BitCast>();
            var array = value.Operand.Type.Unqualified.ShouldBeOfType<CType.Array>();
            array.Count.ShouldBe(4);
            array.Element.SizeOf.ShouldBe(1);
        }
        finally { File.Delete(path); }
    }
}

using System;
using System.IO;
using DotCC.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigWideVectorTests
{
    [Fact]
    public unsafe void Runtime_storage_has_exact_size_and_unaligned_multibyte_lanes()
    {
        sizeof(ZigVector1024<byte>).ShouldBe(128);
        sizeof(ZigVector1024<ulong>).ShouldBe(128);
        sizeof(ZigVector32<ushort>).ShouldBe(4);
        byte* data = stackalloc byte[132];
        for (var i = 0; i < 132; i++) data[i] = 0xaa;
        var splat = ZigVec.Splat1024(0x12345678u);
        ZigVec.Store(splat, (uint*)(data + 1));
        var loaded = ZigVec.Load1024((uint*)(data + 1));
        for (ulong i = 0; i < 32; i++) ZigVec.Get(loaded, i).ShouldBe(0x12345678u);
        ZigVec.Get(ZigVec.With(loaded, 31, 0xabcdef01u), 31).ShouldBe(0xabcdef01u);
        ZigVec.Get(loaded, 31).ShouldBe(0x12345678u);
        data[0].ShouldBe((byte)0xaa);
        for (var i = 129; i < 132; i++) data[i].ShouldBe((byte)0xaa);
        ZigVec.Store(ZigVec.Splat32((ushort)0x5678), (ushort*)(data + 3));
        var small = ZigVec.Load32((ushort*)(data + 3));
        ZigVec.Get(small, 0).ShouldBe((ushort)0x5678);
        ZigVec.Get(small, 1).ShouldBe((ushort)0x5678);
    }

    [Theory]
    [InlineData("a + a", "binary")]
    [InlineData("a == a", "at most 64 lanes")]
    [InlineData("@reduce(.Add, a)", "@reduce")]
    [InlineData("-a", "unary")]
    [InlineData("@as(@Vector(128, i8), @intCast(a))", "lane conversion")]
    [InlineData("@as(@Vector(128, i8), @truncate(a))", "lane conversion")]
    [InlineData("@as(@Vector(128, u4), @splat(3))", "non-native integer lane widths")]
    public void Unsupported_storage_vector_operations_are_explicit_cuts(string expression, string diagnostic)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-vector-cut-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, "pub fn main() u8 { const a: @Vector(128, u8) = @splat(7); _ = " + expression + "; return 0; }");
            var ex = Should.Throw<CompileException>(() => Compiler.EmitCSharp([path]));
            ex.Message.ShouldContain("storage vector");
            ex.Message.ShouldContain(diagnostic);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void Bool_masks_keep_their_existing_64_lane_limit()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-vector-mask-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, "pub fn main() u8 { const a: @Vector(128, bool) = @splat(true); _ = a; return 0; }");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path])).Message.ShouldContain("at most 64 lanes");
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("const V = @Vector(32, f32); const a: V = @splat(1); _ = a & a;", "native-width integer")]
    [InlineData("const V = @Vector(32, f32); const a: V = @splat(1); _ = ~a;", "native-width integer")]
    [InlineData("const V = @Vector(32, u31); const a: V = @splat(1); _ = a & a;", "non-native integer lane widths")]
    [InlineData("const V = @Vector(128, u4); const a: V = @splat(1); _ = a | a;", "non-native integer lane widths")]
    [InlineData("const V = @Vector(3, u8); const a: V = @splat(1); _ = a ^ a;", "dotcc lowers a vector")]
    [InlineData("const a: @Vector(32, u32) = @splat(1); const b: @Vector(16, u64) = @splat(1); _ = a & b;", "same integer vector type")]
    [InlineData("const a: @Vector(32, u32) = @splat(1); _ = a & @as(u32, 1);", "same integer vector type")]
    public void Rejected_lane_kinds_logical_widths_and_shapes_remain_explicit(string body, string diagnostic)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-vector-invalid-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, "pub fn main() u8 { " + body + " return 0; }");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path])).Message.ShouldContain(diagnostic);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void Warm_bitwise_helpers_allocate_no_heap_storage()
    {
        var initial = ZigVec.Splat1024(0x12345678u);
        var mask = ZigVec.Splat1024(0x0f0f0f0fu);
        var small = ZigVec.Splat32((short)-7);
        var smallMask = ZigVec.Splat32((short)0x71);
        ulong equal = 0;
        // One bounded warmup and measurement, retaining observable real data.
        for (var i = 0; i < 100; i++) Step();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1000; i++) Step();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        allocated.ShouldBe(0);
        equal.ShouldBe(0xffffffffUL);
        ZigVec.Get(initial, 31).ShouldBe(0x12345678u);
        ZigVec.Get(small, 1).ShouldBe((short)-7);

        void Step()
        {
            var anded = ZigVec.BitAnd(initial, mask);
            var ored = ZigVec.BitOr(anded, ZigVec.BitAnd(initial, ZigVec.BitNot(mask)));
            initial = ZigVec.BitXor(ZigVec.BitXor(ored, mask), mask);
            var smallAnd = ZigVec.BitAnd(small, smallMask);
            var smallOr = ZigVec.BitOr(smallAnd, ZigVec.BitAnd(small, ZigVec.BitNot(smallMask)));
            small = ZigVec.BitXor(ZigVec.BitXor(smallOr, smallMask), smallMask);
            equal = ZigVec.Eq(initial, ored) ^ ZigVec.Ne(small, small);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using DotCC.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class Float128ToSingleRoundingTests
{
    // Native Zig @floatCast + @bitCast confirms these exact bit patterns.
    public static IEnumerable<object[]> Cases => new object[][]
    {
        new object[] { "exact-00000000-positive", 0x0000000000000000UL, 0x0000000000000000UL, 0x00000000u },
        new object[] { "exact-00000000-negative", 0x8000000000000000UL, 0x0000000000000000UL, 0x80000000u },
        new object[] { "exact-00000001-positive", 0x3f6a000000000000UL, 0x0000000000000000UL, 0x00000001u },
        new object[] { "exact-00000001-negative", 0xbf6a000000000000UL, 0x0000000000000000UL, 0x80000001u },
        new object[] { "exact-00000002-positive", 0x3f6b000000000000UL, 0x0000000000000000UL, 0x00000002u },
        new object[] { "exact-00000002-negative", 0xbf6b000000000000UL, 0x0000000000000000UL, 0x80000002u },
        new object[] { "exact-007ffffe-positive", 0x3f80fffff8000000UL, 0x0000000000000000UL, 0x007ffffeu },
        new object[] { "exact-007ffffe-negative", 0xbf80fffff8000000UL, 0x0000000000000000UL, 0x807ffffeu },
        new object[] { "exact-007fffff-positive", 0x3f80fffffc000000UL, 0x0000000000000000UL, 0x007fffffu },
        new object[] { "exact-007fffff-negative", 0xbf80fffffc000000UL, 0x0000000000000000UL, 0x807fffffu },
        new object[] { "exact-00800000-positive", 0x3f81000000000000UL, 0x0000000000000000UL, 0x00800000u },
        new object[] { "exact-00800000-negative", 0xbf81000000000000UL, 0x0000000000000000UL, 0x80800000u },
        new object[] { "exact-00800001-positive", 0x3f81000002000000UL, 0x0000000000000000UL, 0x00800001u },
        new object[] { "exact-00800001-negative", 0xbf81000002000000UL, 0x0000000000000000UL, 0x80800001u },
        new object[] { "exact-3f000000-positive", 0x3ffe000000000000UL, 0x0000000000000000UL, 0x3f000000u },
        new object[] { "exact-3f000000-negative", 0xbffe000000000000UL, 0x0000000000000000UL, 0xbf000000u },
        new object[] { "exact-3f7fffff-positive", 0x3ffefffffe000000UL, 0x0000000000000000UL, 0x3f7fffffu },
        new object[] { "exact-3f7fffff-negative", 0xbffefffffe000000UL, 0x0000000000000000UL, 0xbf7fffffu },
        new object[] { "exact-3f800000-positive", 0x3fff000000000000UL, 0x0000000000000000UL, 0x3f800000u },
        new object[] { "exact-3f800000-negative", 0xbfff000000000000UL, 0x0000000000000000UL, 0xbf800000u },
        new object[] { "exact-3f800001-positive", 0x3fff000002000000UL, 0x0000000000000000UL, 0x3f800001u },
        new object[] { "exact-3f800001-negative", 0xbfff000002000000UL, 0x0000000000000000UL, 0xbf800001u },
        new object[] { "exact-40000000-positive", 0x4000000000000000UL, 0x0000000000000000UL, 0x40000000u },
        new object[] { "exact-40000000-negative", 0xc000000000000000UL, 0x0000000000000000UL, 0xc0000000u },
        new object[] { "exact-4b000001-positive", 0x4016000002000000UL, 0x0000000000000000UL, 0x4b000001u },
        new object[] { "exact-4b000001-negative", 0xc016000002000000UL, 0x0000000000000000UL, 0xcb000001u },
        new object[] { "exact-7f000000-positive", 0x407e000000000000UL, 0x0000000000000000UL, 0x7f000000u },
        new object[] { "exact-7f000000-negative", 0xc07e000000000000UL, 0x0000000000000000UL, 0xff000000u },
        new object[] { "exact-7f7fffff-positive", 0x407efffffe000000UL, 0x0000000000000000UL, 0x7f7fffffu },
        new object[] { "exact-7f7fffff-negative", 0xc07efffffe000000UL, 0x0000000000000000UL, 0xff7fffffu },
        new object[] { "midpoint-00000000-quad-ulp-1-positive", 0x3f68ffffffffffffUL, 0xffffffffffffffffUL, 0x00000000u },
        new object[] { "midpoint-00000000-quad-ulp-1-negative", 0xbf68ffffffffffffUL, 0xffffffffffffffffUL, 0x80000000u },
        new object[] { "midpoint-00000000-quad-ulp+0-positive", 0x3f69000000000000UL, 0x0000000000000000UL, 0x00000000u },
        new object[] { "midpoint-00000000-quad-ulp+0-negative", 0xbf69000000000000UL, 0x0000000000000000UL, 0x80000000u },
        new object[] { "midpoint-00000000-quad-ulp+1-positive", 0x3f69000000000000UL, 0x0000000000000001UL, 0x00000001u },
        new object[] { "midpoint-00000000-quad-ulp+1-negative", 0xbf69000000000000UL, 0x0000000000000001UL, 0x80000001u },
        new object[] { "midpoint-00000001-quad-ulp-1-positive", 0x3f6a7fffffffffffUL, 0xffffffffffffffffUL, 0x00000001u },
        new object[] { "midpoint-00000001-quad-ulp-1-negative", 0xbf6a7fffffffffffUL, 0xffffffffffffffffUL, 0x80000001u },
        new object[] { "midpoint-00000001-quad-ulp+0-positive", 0x3f6a800000000000UL, 0x0000000000000000UL, 0x00000002u },
        new object[] { "midpoint-00000001-quad-ulp+0-negative", 0xbf6a800000000000UL, 0x0000000000000000UL, 0x80000002u },
        new object[] { "midpoint-00000001-quad-ulp+1-positive", 0x3f6a800000000000UL, 0x0000000000000001UL, 0x00000002u },
        new object[] { "midpoint-00000001-quad-ulp+1-negative", 0xbf6a800000000000UL, 0x0000000000000001UL, 0x80000002u },
        new object[] { "midpoint-00000002-quad-ulp-1-positive", 0x3f6b3fffffffffffUL, 0xffffffffffffffffUL, 0x00000002u },
        new object[] { "midpoint-00000002-quad-ulp-1-negative", 0xbf6b3fffffffffffUL, 0xffffffffffffffffUL, 0x80000002u },
        new object[] { "midpoint-00000002-quad-ulp+0-positive", 0x3f6b400000000000UL, 0x0000000000000000UL, 0x00000002u },
        new object[] { "midpoint-00000002-quad-ulp+0-negative", 0xbf6b400000000000UL, 0x0000000000000000UL, 0x80000002u },
        new object[] { "midpoint-00000002-quad-ulp+1-positive", 0x3f6b400000000000UL, 0x0000000000000001UL, 0x00000003u },
        new object[] { "midpoint-00000002-quad-ulp+1-negative", 0xbf6b400000000000UL, 0x0000000000000001UL, 0x80000003u },
        new object[] { "midpoint-007ffffe-quad-ulp-1-positive", 0x3f80fffff9ffffffUL, 0xffffffffffffffffUL, 0x007ffffeu },
        new object[] { "midpoint-007ffffe-quad-ulp-1-negative", 0xbf80fffff9ffffffUL, 0xffffffffffffffffUL, 0x807ffffeu },
        new object[] { "midpoint-007ffffe-quad-ulp+0-positive", 0x3f80fffffa000000UL, 0x0000000000000000UL, 0x007ffffeu },
        new object[] { "midpoint-007ffffe-quad-ulp+0-negative", 0xbf80fffffa000000UL, 0x0000000000000000UL, 0x807ffffeu },
        new object[] { "midpoint-007ffffe-quad-ulp+1-positive", 0x3f80fffffa000000UL, 0x0000000000000001UL, 0x007fffffu },
        new object[] { "midpoint-007ffffe-quad-ulp+1-negative", 0xbf80fffffa000000UL, 0x0000000000000001UL, 0x807fffffu },
        new object[] { "midpoint-007fffff-quad-ulp-1-positive", 0x3f80fffffdffffffUL, 0xffffffffffffffffUL, 0x007fffffu },
        new object[] { "midpoint-007fffff-quad-ulp-1-negative", 0xbf80fffffdffffffUL, 0xffffffffffffffffUL, 0x807fffffu },
        new object[] { "midpoint-007fffff-quad-ulp+0-positive", 0x3f80fffffe000000UL, 0x0000000000000000UL, 0x00800000u },
        new object[] { "midpoint-007fffff-quad-ulp+0-negative", 0xbf80fffffe000000UL, 0x0000000000000000UL, 0x80800000u },
        new object[] { "midpoint-007fffff-quad-ulp+1-positive", 0x3f80fffffe000000UL, 0x0000000000000001UL, 0x00800000u },
        new object[] { "midpoint-007fffff-quad-ulp+1-negative", 0xbf80fffffe000000UL, 0x0000000000000001UL, 0x80800000u },
        new object[] { "midpoint-00800000-quad-ulp-1-positive", 0x3f81000000ffffffUL, 0xffffffffffffffffUL, 0x00800000u },
        new object[] { "midpoint-00800000-quad-ulp-1-negative", 0xbf81000000ffffffUL, 0xffffffffffffffffUL, 0x80800000u },
        new object[] { "midpoint-00800000-quad-ulp+0-positive", 0x3f81000001000000UL, 0x0000000000000000UL, 0x00800000u },
        new object[] { "midpoint-00800000-quad-ulp+0-negative", 0xbf81000001000000UL, 0x0000000000000000UL, 0x80800000u },
        new object[] { "midpoint-00800000-quad-ulp+1-positive", 0x3f81000001000000UL, 0x0000000000000001UL, 0x00800001u },
        new object[] { "midpoint-00800000-quad-ulp+1-negative", 0xbf81000001000000UL, 0x0000000000000001UL, 0x80800001u },
        new object[] { "midpoint-00800001-quad-ulp-1-positive", 0x3f81000002ffffffUL, 0xffffffffffffffffUL, 0x00800001u },
        new object[] { "midpoint-00800001-quad-ulp-1-negative", 0xbf81000002ffffffUL, 0xffffffffffffffffUL, 0x80800001u },
        new object[] { "midpoint-00800001-quad-ulp+0-positive", 0x3f81000003000000UL, 0x0000000000000000UL, 0x00800002u },
        new object[] { "midpoint-00800001-quad-ulp+0-negative", 0xbf81000003000000UL, 0x0000000000000000UL, 0x80800002u },
        new object[] { "midpoint-00800001-quad-ulp+1-positive", 0x3f81000003000000UL, 0x0000000000000001UL, 0x00800002u },
        new object[] { "midpoint-00800001-quad-ulp+1-negative", 0xbf81000003000000UL, 0x0000000000000001UL, 0x80800002u },
        new object[] { "midpoint-3e8000ee-quad-ulp-1-positive", 0x3ffd0001dcffffffUL, 0xffffffffffffffffUL, 0x3e8000eeu },
        new object[] { "midpoint-3e8000ee-quad-ulp-1-negative", 0xbffd0001dcffffffUL, 0xffffffffffffffffUL, 0xbe8000eeu },
        new object[] { "midpoint-3e8000ee-quad-ulp+0-positive", 0x3ffd0001dd000000UL, 0x0000000000000000UL, 0x3e8000eeu },
        new object[] { "midpoint-3e8000ee-quad-ulp+0-negative", 0xbffd0001dd000000UL, 0x0000000000000000UL, 0xbe8000eeu },
        new object[] { "midpoint-3e8000ee-quad-ulp+1-positive", 0x3ffd0001dd000000UL, 0x0000000000000001UL, 0x3e8000efu },
        new object[] { "midpoint-3e8000ee-quad-ulp+1-negative", 0xbffd0001dd000000UL, 0x0000000000000001UL, 0xbe8000efu },
        new object[] { "midpoint-3f7fffff-quad-ulp-1-positive", 0x3ffefffffeffffffUL, 0xffffffffffffffffUL, 0x3f7fffffu },
        new object[] { "midpoint-3f7fffff-quad-ulp-1-negative", 0xbffefffffeffffffUL, 0xffffffffffffffffUL, 0xbf7fffffu },
        new object[] { "midpoint-3f7fffff-quad-ulp+0-positive", 0x3ffeffffff000000UL, 0x0000000000000000UL, 0x3f800000u },
        new object[] { "midpoint-3f7fffff-quad-ulp+0-negative", 0xbffeffffff000000UL, 0x0000000000000000UL, 0xbf800000u },
        new object[] { "midpoint-3f7fffff-quad-ulp+1-positive", 0x3ffeffffff000000UL, 0x0000000000000001UL, 0x3f800000u },
        new object[] { "midpoint-3f7fffff-quad-ulp+1-negative", 0xbffeffffff000000UL, 0x0000000000000001UL, 0xbf800000u },
        new object[] { "midpoint-3f800000-quad-ulp-1-positive", 0x3fff000000ffffffUL, 0xffffffffffffffffUL, 0x3f800000u },
        new object[] { "midpoint-3f800000-quad-ulp-1-negative", 0xbfff000000ffffffUL, 0xffffffffffffffffUL, 0xbf800000u },
        new object[] { "midpoint-3f800000-quad-ulp+0-positive", 0x3fff000001000000UL, 0x0000000000000000UL, 0x3f800000u },
        new object[] { "midpoint-3f800000-quad-ulp+0-negative", 0xbfff000001000000UL, 0x0000000000000000UL, 0xbf800000u },
        new object[] { "midpoint-3f800000-quad-ulp+1-positive", 0x3fff000001000000UL, 0x0000000000000001UL, 0x3f800001u },
        new object[] { "midpoint-3f800000-quad-ulp+1-negative", 0xbfff000001000000UL, 0x0000000000000001UL, 0xbf800001u },
        new object[] { "midpoint-3f800001-quad-ulp-1-positive", 0x3fff000002ffffffUL, 0xffffffffffffffffUL, 0x3f800001u },
        new object[] { "midpoint-3f800001-quad-ulp-1-negative", 0xbfff000002ffffffUL, 0xffffffffffffffffUL, 0xbf800001u },
        new object[] { "midpoint-3f800001-quad-ulp+0-positive", 0x3fff000003000000UL, 0x0000000000000000UL, 0x3f800002u },
        new object[] { "midpoint-3f800001-quad-ulp+0-negative", 0xbfff000003000000UL, 0x0000000000000000UL, 0xbf800002u },
        new object[] { "midpoint-3f800001-quad-ulp+1-positive", 0x3fff000003000000UL, 0x0000000000000001UL, 0x3f800002u },
        new object[] { "midpoint-3f800001-quad-ulp+1-negative", 0xbfff000003000000UL, 0x0000000000000001UL, 0xbf800002u },
        new object[] { "midpoint-40000000-quad-ulp-1-positive", 0x4000000000ffffffUL, 0xffffffffffffffffUL, 0x40000000u },
        new object[] { "midpoint-40000000-quad-ulp-1-negative", 0xc000000000ffffffUL, 0xffffffffffffffffUL, 0xc0000000u },
        new object[] { "midpoint-40000000-quad-ulp+0-positive", 0x4000000001000000UL, 0x0000000000000000UL, 0x40000000u },
        new object[] { "midpoint-40000000-quad-ulp+0-negative", 0xc000000001000000UL, 0x0000000000000000UL, 0xc0000000u },
        new object[] { "midpoint-40000000-quad-ulp+1-positive", 0x4000000001000000UL, 0x0000000000000001UL, 0x40000001u },
        new object[] { "midpoint-40000000-quad-ulp+1-negative", 0xc000000001000000UL, 0x0000000000000001UL, 0xc0000001u },
        new object[] { "midpoint-4b000000-quad-ulp-1-positive", 0x4016000000ffffffUL, 0xffffffffffffffffUL, 0x4b000000u },
        new object[] { "midpoint-4b000000-quad-ulp-1-negative", 0xc016000000ffffffUL, 0xffffffffffffffffUL, 0xcb000000u },
        new object[] { "midpoint-4b000000-quad-ulp+0-positive", 0x4016000001000000UL, 0x0000000000000000UL, 0x4b000000u },
        new object[] { "midpoint-4b000000-quad-ulp+0-negative", 0xc016000001000000UL, 0x0000000000000000UL, 0xcb000000u },
        new object[] { "midpoint-4b000000-quad-ulp+1-positive", 0x4016000001000000UL, 0x0000000000000001UL, 0x4b000001u },
        new object[] { "midpoint-4b000000-quad-ulp+1-negative", 0xc016000001000000UL, 0x0000000000000001UL, 0xcb000001u },
        new object[] { "midpoint-4b000001-quad-ulp-1-positive", 0x4016000002ffffffUL, 0xffffffffffffffffUL, 0x4b000001u },
        new object[] { "midpoint-4b000001-quad-ulp-1-negative", 0xc016000002ffffffUL, 0xffffffffffffffffUL, 0xcb000001u },
        new object[] { "midpoint-4b000001-quad-ulp+0-positive", 0x4016000003000000UL, 0x0000000000000000UL, 0x4b000002u },
        new object[] { "midpoint-4b000001-quad-ulp+0-negative", 0xc016000003000000UL, 0x0000000000000000UL, 0xcb000002u },
        new object[] { "midpoint-4b000001-quad-ulp+1-positive", 0x4016000003000000UL, 0x0000000000000001UL, 0x4b000002u },
        new object[] { "midpoint-4b000001-quad-ulp+1-negative", 0xc016000003000000UL, 0x0000000000000001UL, 0xcb000002u },
        new object[] { "midpoint-7f7ffffe-quad-ulp-1-positive", 0x407efffffcffffffUL, 0xffffffffffffffffUL, 0x7f7ffffeu },
        new object[] { "midpoint-7f7ffffe-quad-ulp-1-negative", 0xc07efffffcffffffUL, 0xffffffffffffffffUL, 0xff7ffffeu },
        new object[] { "midpoint-7f7ffffe-quad-ulp+0-positive", 0x407efffffd000000UL, 0x0000000000000000UL, 0x7f7ffffeu },
        new object[] { "midpoint-7f7ffffe-quad-ulp+0-negative", 0xc07efffffd000000UL, 0x0000000000000000UL, 0xff7ffffeu },
        new object[] { "midpoint-7f7ffffe-quad-ulp+1-positive", 0x407efffffd000000UL, 0x0000000000000001UL, 0x7f7fffffu },
        new object[] { "midpoint-7f7ffffe-quad-ulp+1-negative", 0xc07efffffd000000UL, 0x0000000000000001UL, 0xff7fffffu },
        new object[] { "midpoint-7f7fffff-quad-ulp-1-positive", 0x407efffffeffffffUL, 0xffffffffffffffffUL, 0x7f7fffffu },
        new object[] { "midpoint-7f7fffff-quad-ulp-1-negative", 0xc07efffffeffffffUL, 0xffffffffffffffffUL, 0xff7fffffu },
        new object[] { "midpoint-7f7fffff-quad-ulp+0-positive", 0x407effffff000000UL, 0x0000000000000000UL, 0x7f800000u },
        new object[] { "midpoint-7f7fffff-quad-ulp+0-negative", 0xc07effffff000000UL, 0x0000000000000000UL, 0xff800000u },
        new object[] { "midpoint-7f7fffff-quad-ulp+1-positive", 0x407effffff000000UL, 0x0000000000000001UL, 0x7f800000u },
        new object[] { "midpoint-7f7fffff-quad-ulp+1-negative", 0xc07effffff000000UL, 0x0000000000000001UL, 0xff800000u },
        new object[] { "review-midpoint-plus-2^-100-positive", 0x3fff000001000000UL, 0x0000000000001000UL, 0x3f800001u },
        new object[] { "review-midpoint-plus-2^-100-negative", 0xbfff000001000000UL, 0x0000000000001000UL, 0xbf800001u },
        new object[] { "review-midpoint-minus-2^-100-positive", 0x3fff000000ffffffUL, 0xfffffffffffff000UL, 0x3f800000u },
        new object[] { "review-midpoint-minus-2^-100-negative", 0xbfff000000ffffffUL, 0xfffffffffffff000UL, 0xbf800000u },
        new object[] { "quad-epsilon-positive", 0x0000000000000000UL, 0x0000000000000001UL, 0x00000000u },
        new object[] { "quad-epsilon-negative", 0x8000000000000000UL, 0x0000000000000001UL, 0x80000000u },
        new object[] { "quad-max-subnormal-positive", 0x0000ffffffffffffUL, 0xffffffffffffffffUL, 0x00000000u },
        new object[] { "quad-max-subnormal-negative", 0x8000ffffffffffffUL, 0xffffffffffffffffUL, 0x80000000u },
        new object[] { "quad-tiny-positive", 0x3e0b000000000000UL, 0x0000000000000000UL, 0x00000000u },
        new object[] { "quad-tiny-negative", 0xbe0b000000000000UL, 0x0000000000000000UL, 0x80000000u },
        new object[] { "quad-huge-positive", 0x41f3000000000000UL, 0x0000000000000000UL, 0x7f800000u },
        new object[] { "quad-huge-negative", 0xc1f3000000000000UL, 0x0000000000000000UL, 0xff800000u },
        new object[] { "infinity-positive", 0x7fff000000000000UL, 0x0000000000000000UL, 0x7f800000u },
        new object[] { "infinity-negative", 0xffff000000000000UL, 0x0000000000000000UL, 0xff800000u },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Round_once_at_the_binary32_boundary(string name, ulong high, ulong low, uint expected)
    {
        _ = name;
        var value = Float128.FromBits(((UInt128)high << 64) | low);
        BitConverter.SingleToUInt32Bits(Float128.ToSingle(value)).ShouldBe(expected);
        BitConverter.SingleToUInt32Bits((float)value).ShouldBe(expected);
        BitConverter.SingleToUInt32Bits(float.CreateChecked(value)).ShouldBe(expected);
    }

    [Fact]
    public void A_double_intermediate_erases_the_bit_above_a_binary32_midpoint()
    {
        var value = Float128.FromBits(((UInt128)0x3fff << 112) | (UInt128.One << 88) | (UInt128.One << 12));
        BitConverter.SingleToUInt32Bits((float)Float128.ToDouble(value)).ShouldBe(0x3f800000u);
        BitConverter.SingleToUInt32Bits(Float128.ToSingle(value)).ShouldBe(0x3f800001u);
        BitConverter.SingleToUInt32Bits(Float128.ToSingle(Float128.Negate(value))).ShouldBe(0xbf800001u);
    }

    [Fact]
    public void Zig_floatCast_uses_the_direct_rounding_helper()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-single-" + Guid.NewGuid().ToString("N") + ".zig");
        try
        {
            File.WriteAllText(path, "pub fn narrow(q: f128) f32 { return @floatCast(q); } pub fn main() u8 { return 42; }");
            Compiler.EmitCSharp(new[] { path }).ShouldContain("Float128.ToSingle(q)");
        }
        finally { File.Delete(path); }
    }
}

using System;
using DotCC.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ZigTestingEqualityRuntimeTests
{
    private enum Width : byte { Narrow = 1, Wide = 2 }

    [Fact]
    public void Typed_enum_and_scalar_assertions_preserve_source_equality_and_error_codes()
    {
        ZigTesting.expectEqual(Width.Narrow, Width.Narrow).IsErr.ShouldBeFalse();
        ZigTesting.expectEqual(Width.Narrow, Width.Wide).Code.ShouldBe(ZigTesting.TestExpectedEqual);
        ZigTesting.expectEqual(float.NaN, float.NaN).Code.ShouldBe(ZigTesting.TestExpectedEqual);
        ZigTesting.expectEqual(double.NaN, double.NaN).Code.ShouldBe(ZigTesting.TestExpectedEqual);
        ZigTesting.expectEqual(Half.NaN, Half.NaN).Code.ShouldBe(ZigTesting.TestExpectedEqual);
        ZigTesting.expectEqual(Float128.NaN, Float128.NaN).Code.ShouldBe(ZigTesting.TestExpectedEqual);
        ZigTesting.expectEqual(0f, -0f).IsErr.ShouldBeFalse();
        ZigTesting.expectEqual(0d, -0d).IsErr.ShouldBeFalse();
        ZigTesting.expectEqual((Half)0, (Half)(-0d)).IsErr.ShouldBeFalse();
        ZigTesting.expectEqual(Float128.Zero, Float128.NegativeZero).IsErr.ShouldBeFalse();
        ZigTesting.expectEqual((CBool)true, (CBool)true).IsErr.ShouldBeFalse();
        ZigTesting.expectEqual((CBool)true, (CBool)false).Code.ShouldBe(ZigTesting.TestExpectedEqual);
        ZigTesting.expectEqual(ulong.MaxValue, ulong.MaxValue).IsErr.ShouldBeFalse();
        ZigTesting.expectEqual(UInt128.MaxValue, UInt128.MaxValue - 1).Code.ShouldBe(ZigTesting.TestExpectedEqual);
    }

    [Fact]
    public void Warm_enum_comparison_does_not_box_either_operand()
    {
        for (var i = 0; i < 1000; i++) ZigTesting.expectEqual(Width.Wide, Width.Wide);
        var before = GC.GetAllocatedBytesForCurrentThread();
        var code = 0;
        for (var i = 0; i < 10000; i++)
            code |= ZigTesting.expectEqual(Width.Wide, Width.Wide).Code;
        (GC.GetAllocatedBytesForCurrentThread() - before).ShouldBe(0);
        code.ShouldBe(0);
    }
}

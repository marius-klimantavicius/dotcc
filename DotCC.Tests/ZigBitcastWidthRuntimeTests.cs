using System;
using DotCC.Libc;
using Shouldly;
using Xunit;
namespace DotCC.Tests;

public sealed unsafe class ZigBitcastWidthRuntimeTests
{
    [Fact]
    public void Transport_masks_each_physical_lane_and_extends_the_logical_sign_bit()
    {
        uint* padded = stackalloc uint[] { 0xff563412, 0xffabcdef };
        ZigBitcastWidth.Array<ulong>(padded, 2, 24, 4, 48, false).ShouldBe(0xabcdef563412UL);
        byte* lanes = stackalloc byte[] { 0xf9, 0xfa, 0xfb };
        ZigBitcastWidth.Array<ushort>(lanes, 3, 3, 1, 9, false).ShouldBe((ushort)209);
        foreach (var bits in new[] { 1, 3, 7, 9, 24, 33, 65, 128 })
        {
            var mask = bits == 128 ? UInt128.MaxValue : ((UInt128)1 << bits) - 1;
            ZigBitcastWidth.Scalar<UInt128, Int128>(mask, bits, true).ShouldBe((Int128)(-1));
            var sign = (UInt128)1 << (bits - 1);
            var expected = unchecked((Int128)sign);
            if (bits < 128) expected = -(Int128)sign;
            ZigBitcastWidth.Scalar<UInt128, Int128>(sign, bits, true).ShouldBe(expected);
            ZigBitcastWidth.Scalar<Int128, UInt128>((Int128)(-1), bits, false).ShouldBe(mask);
        }
        ZigBitcastWidth.Scalar<float, uint>(-0.0f, 32, false).ShouldBe(0x80000000u);
        var nan = ZigBitcastWidth.Scalar<uint, float>(0x7fc12345u, 32, false);
        BitConverter.SingleToUInt32Bits(nan).ShouldBe(0x7fc12345u);
    }

    [Fact]
    public void Warm_transport_allocates_no_managed_storage_across_a_compacting_collection()
    {
        byte* source = stackalloc byte[] { 0x12, 0x34, 0x56 };
        for (var i = 0; i < 2048; i++) _ = ZigBitcastWidth.Array<uint>(source, 3, 8, 1, 24, false);
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        var before = GC.GetAllocatedBytesForCurrentThread();
        ulong total = 0;
        for (var i = 0; i < 10000; i++)
        {
            total += ZigBitcastWidth.Array<uint>(source, 3, 8, 1, 24, false);
            total += ZigBitcastWidth.Scalar<int, uint>(-1, 24, false);
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        allocated.ShouldBe(0);
        total.ShouldBe(10000UL * (0x563412u + 0xffffffu));
    }
}

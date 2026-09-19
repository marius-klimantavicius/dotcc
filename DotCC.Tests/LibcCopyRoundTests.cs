using System;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;
using RuntimeLibc = DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcCopyRoundTests
{
    [Fact]
    public void Cursor_copy_returns_written_terminator_without_touching_following_bytes()
    {
        byte* destination=stackalloc byte[16]; memset(destination,0xa5,16UL);
        ((nint)stpcpy(destination,L("abc\0"u8))).ShouldBe((nint)(destination+3));
        destination[3].ShouldBe((byte)0); destination[4].ShouldBe((byte)0xa5);
        ((nint)stpcpy(destination+3,L("\0"u8))).ShouldBe((nint)(destination+3));
        ((nint)stpcpy(destination+3,L("xy\0"u8))).ShouldBe((nint)(destination+5));
        strcmp(destination,L("abcxy\0"u8)).ShouldBe(0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Allocated_copies_are_independent_freeable_and_preserve_errno(bool debug)
    {
        bool previous=RuntimeLibc._dbgHeap; RuntimeLibc._dbgHeap=debug;
        byte* first=null; byte* second=null; byte* empty=null;
        try
        {
            byte* original=stackalloc byte[]{(byte)'a',(byte)'b',(byte)'c',0};
            errno=123; first=strdup(original); second=strndup(original,2UL); empty=strndup(original,0UL);
            ((nint)first).ShouldNotBe(0); ((nint)second).ShouldNotBe(0); ((nint)empty).ShouldNotBe(0);
            errno.ShouldBe(123); original[0]=(byte)'X';
            GC.Collect(2,GCCollectionMode.Forced,true,true);
            strcmp(first,L("abc\0"u8)).ShouldBe(0); strcmp(second,L("ab\0"u8)).ShouldBe(0); empty[0].ShouldBe((byte)0);
            first[1]=(byte)'Y'; original[1].ShouldBe((byte)'b'); second[1].ShouldBe((byte)'b');
        }
        finally { free(first);free(second);free(empty);RuntimeLibc._dbgHeap=previous; }
    }

    [Fact]
    public void Bound_limits_reads_but_large_bound_does_not_pad_the_allocation()
    {
        byte* raw=stackalloc byte[]{0x80,0xff,0x41};
        byte* copy=strndup(raw,3UL);
        try { memcmp(copy,raw,3UL).ShouldBe(0);copy[3].ShouldBe((byte)0); } finally { free(copy); }
        copy=strndup(L("small\0"u8),ulong.MaxValue);
        try { strcmp(copy,L("small\0"u8)).ShouldBe(0); } finally { free(copy); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Allocation_failure_sets_errno_before_any_copy(bool debug)
    {
        bool previous=RuntimeLibc._dbgHeap;RuntimeLibc._dbgHeap=debug;
        try
        {
            errno=123;
            ((nint)RuntimeLibc.DuplicateStringBytes(null,ulong.MaxValue)).ShouldBe(0);
            errno.ShouldBe(ENOMEM);
            // Fits the native size_t addition but cannot be allocated. Exercises
            // native allocation failure and debug overhead rejection, not OOM stress.
            errno=123;
            ((nint)RuntimeLibc.DuplicateStringBytes(null,ulong.MaxValue-128)).ShouldBe(0);
            errno.ShouldBe(ENOMEM);
        }
        finally { RuntimeLibc._dbgHeap=previous; }
    }

    [Theory]
    [InlineData(-3.5,-4.0)] [InlineData(-2.5,-2.0)] [InlineData(-1.5,-2.0)]
    [InlineData(-0.5,-0.0)] [InlineData(-0.25,-0.0)] [InlineData(0.5,0.0)]
    [InlineData(1.5,2.0)] [InlineData(2.5,2.0)] [InlineData(3.5,4.0)]
    [InlineData(4503599627370496.0,4503599627370496.0)]
    public void Default_rounding_matches_native_ties_even_bits(double input,double expected)
    {
        BitConverter.DoubleToInt64Bits(rint(input)).ShouldBe(BitConverter.DoubleToInt64Bits(expected));
        BitConverter.SingleToInt32Bits(rintf((float)input)).ShouldBe(BitConverter.SingleToInt32Bits((float)expected));
        BitConverter.SingleToInt32Bits(rint((float)input)).ShouldBe(BitConverter.SingleToInt32Bits((float)expected));
    }

    [Fact]
    public void Nonfinite_values_signed_zero_and_unordered_operands_keep_their_classification()
    {
        foreach(double value in new[]{0.0,BitConverter.Int64BitsToDouble(long.MinValue),double.PositiveInfinity,double.NegativeInfinity})
        {
            BitConverter.DoubleToInt64Bits(rint(value)).ShouldBe(BitConverter.DoubleToInt64Bits(value));
            BitConverter.SingleToInt32Bits(rintf((float)value)).ShouldBe(BitConverter.SingleToInt32Bits((float)value));
        }
        foreach(double nan in new[]{double.NaN,BitConverter.Int64BitsToDouble(0x7ff8000000000042L)})
        {
            double.IsNaN(rint(nan)).ShouldBeTrue();float.IsNaN(rintf((float)nan)).ShouldBeTrue();
            isunordered(nan,1.0).ShouldBe(1);isunordered(1.0,nan).ShouldBe(1);isunordered(nan,nan).ShouldBe(1);
            isunordered((float)nan,1f).ShouldBe(1);
        }
        isunordered(double.NegativeInfinity,double.PositiveInfinity).ShouldBe(0);
        isunordered(-0.0,0.0).ShouldBe(0);
    }
}

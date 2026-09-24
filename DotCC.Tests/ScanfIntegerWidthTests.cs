using System;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class ScanfIntegerWidthTests
{
    [Fact]
    public void Unsigned_long_keeps_all_64_bits_and_matches_literal_separators()
    {
        ulong a=0, b=0; uint version=0;
        fixed(byte* input="4294967296,18446744073709551615:19\0"u8)
        fixed(byte* format="%lu,%lu:%u\0"u8)
            sscanf(input,format).Read(&a).Read(&b).Read(&version).Done().ShouldBe(3);
        a.ShouldBe(4294967296UL); b.ShouldBe(ulong.MaxValue); version.ShouldBe(19U);
    }
    [Fact]
    public void Signed_long_and_byte_hex_observe_width_without_overwriting_neighbors()
    {
        long value=0; byte* bytes=stackalloc byte[3]; bytes[0]=17;bytes[1]=0;bytes[2]=29;
        fixed(byte* input="-9223372036854775808 FFA\0"u8)
        fixed(byte* format="%lld %02hhX\0"u8)
            sscanf(input,format).Read(&value).Read(bytes+1).Done().ShouldBe(2);
        value.ShouldBe(long.MinValue);bytes[0].ShouldBe((byte)17);bytes[1].ShouldBe((byte)255);bytes[2].ShouldBe((byte)29);
    }
    [Fact]
    public void Literal_mismatch_stops_all_subsequent_assignments()
    {
        uint a=7,b=8,c=9;
        fixed(byte* input="11-22,33\0"u8)
        fixed(byte* format="%u,%u,%u\0"u8)
            sscanf(input,format).Read(&a).Read(&b).Read(&c).Done().ShouldBe(1);
        a.ShouldBe(11U);b.ShouldBe(8U);c.ShouldBe(9U);
    }
    [Fact]
    public void Typed_size_t_allocator_addresses_preserve_owner_and_original_on_failure()
    {
        using var owner=new RuntimeContext();using var binding=owner.Enter();
        delegate*<ulong,void*> allocate=&malloc;
        delegate*<ulong,ulong,void*> zero=&calloc;
        delegate*<void*,ulong,void*> resize=&realloc;
        byte* value=(byte*)allocate(16);((nint)value).ShouldNotBe(0);value[0]=42;
        ((nint)allocate(ulong.MaxValue-127)).ShouldBe(0);
        ((nint)zero(ulong.MaxValue,2)).ShouldBe(0);
        ((nint)resize(value,ulong.MaxValue-127)).ShouldBe(0);
        owner.NativeAllocations.Count.ShouldBe(1);value[0].ShouldBe((byte)42);
        byte* grown=(byte*)resize(value,32);((nint)grown).ShouldNotBe(0);grown[0].ShouldBe((byte)42);
        free(grown);owner.NativeAllocations.ShouldBeEmpty();
    }
}

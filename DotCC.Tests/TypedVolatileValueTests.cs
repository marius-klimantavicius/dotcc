using System.Runtime.InteropServices;
using DotCC.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed class TypedVolatileValueTests
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ThreeBytes { public byte A, B, C; }
    private struct LargeValue { public long A, B, C; }

    [Fact]
    public void Odd_sized_volatile_values_copy_every_byte_and_do_not_alias()
    {
        var source = new ThreeBytes { A = 11, B = 22, C = 33 };
        var loaded = global::DotCC.Libc.Libc.VolatileValue.Load(ref source);
        loaded.A.ShouldBe((byte)11);
        loaded.B.ShouldBe((byte)22);
        loaded.C.ShouldBe((byte)33);
        loaded.B = 99;
        source.B.ShouldBe((byte)22);
        var stored = global::DotCC.Libc.Libc.VolatileValue.Store(ref source, loaded);
        source.B.ShouldBe((byte)99);
        stored.C.ShouldBe((byte)33);
        stored.C = 88;
        source.C.ShouldBe((byte)33);
    }

    [Fact]
    public void Aggregate_volatile_values_keep_all_words_and_value_update_semantics()
    {
        var source = new LargeValue { A = long.MinValue, B = 0x123456789ABCDEF, C = long.MaxValue };
        var old = global::DotCC.Libc.Libc.VolatileValue.Update(ref source, 7L,
            static (value, amount) => new LargeValue { A = value.A + amount, B = value.B, C = value.C - amount }, true);
        old.A.ShouldBe(long.MinValue);
        old.C.ShouldBe(long.MaxValue);
        source.A.ShouldBe(long.MinValue + 7);
        source.B.ShouldBe(0x123456789ABCDEF);
        source.C.ShouldBe(long.MaxValue - 7);
    }

    [Fact]
    public void Boolean_and_enum_fast_paths_preserve_the_typed_value()
    {
        CBool value = 29;
        ((int)global::DotCC.Libc.Libc.VolatileValue.Load(ref value)).ShouldBe(1);
        global::DotCC.Libc.Libc.VolatileValue.Store(ref value, (CBool)0);
        ((int)global::DotCC.Libc.Libc.VolatileValue.Load(ref value)).ShouldBe(0);
        var level = WideEnum.High;
        global::DotCC.Libc.Libc.VolatileValue.Load(ref level).ShouldBe(WideEnum.High);
    }

    private enum WideEnum : ulong { High = 0xF123456789ABCDEF }
}

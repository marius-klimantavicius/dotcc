using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DotCC.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class GnuIntrinsicTests
{
    [Theory]
    [InlineData("__sync_add_and_fetch((int*)0)", "requires 2 arguments")]
    [InlineData("__sync_add_and_fetch((float*)0, 1)", "integer or pointer object")]
    [InlineData("__sync_add_and_fetch((const int*)0, 1)", "const object")]
    [InlineData("__atomic_load_n((int*)0, 3)", "invalid memory order")]
    [InlineData("__atomic_load_n((int*)0, 6)", "invalid memory order")]
    [InlineData("__atomic_store_n((int*)0, 0, 2)", "invalid memory order")]
    [InlineData("__atomic_compare_exchange_n((int*)0, (int*)0, 1, 0, 0, 2)", "stronger than success")]
    [InlineData("__atomic_compare_exchange_n((int*)0, (short*)0, 1, 0, 5, 0)", "same type")]
    [InlineData("__builtin_bswap32((void*)0)", "arithmetic argument")]
    [InlineData("__builtin_clz()", "requires 1 arguments")]
    [InlineData("__builtin_ctzl(1, 2)", "requires 1 arguments")]
    [InlineData("__builtin_popcountll((void*)0)", "arithmetic argument")]
    public void Invalid_builtin_arguments_fail_during_binding(string expression, string diagnostic)
    {
        var exception = Should.Throw<CompileException>(() => Emit("int main(void) { " + expression + "; return 0; }"));
        exception.Message.ShouldContain(diagnostic);
    }

    [Fact]
    public void Builtin_results_retain_integer_width_and_pointer_type()
    {
        var code = Emit("""
            unsigned long f(unsigned long *p) { return __sync_add_and_fetch(p, 1); }
            void *g(void **p) { return __atomic_load_n(p, __ATOMIC_RELAXED); }
            unsigned long h(unsigned long value) { return __builtin_bswap64(value); }
            int main(void) { return 0; }
            """);
        code.ShouldContain("Atomic.AddFetch(ref *(ulong*)(p), unchecked((ulong)(1)))");
        code.ShouldContain("GnuAtomic.Load(ref *(nint*)(p), (int)(0))");
        code.ShouldContain("ReverseEndianness(unchecked((ulong)(value)))");
        code.ShouldNotContain("__sync_add_and_fetch(");
        code.ShouldNotContain("__atomic_load_n(");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Counters
    {
        public byte Before;
        public byte Byte;
        public byte After;
        public short Short;
        public long Wide;
    }
    private sealed class CounterBox { public Counters Values; }

    [Fact]
    public void Narrow_atomics_preserve_neighbors_and_updates_under_contention()
    {
        var box = new CounterBox { Values = new Counters { Before = 0x55, After = 0xaa } };
        Parallel.For(0, 4, _ =>
        {
            for (int i = 0; i < 5000; ++i)
            {
                Atomic.FetchAdd(ref box.Values.Byte, (byte)1);
                Atomic.AddFetch(ref box.Values.Short, (short)1);
                Atomic.FetchAdd(ref box.Values.Wide, 1L);
            }
        });
        box.Values.Before.ShouldBe((byte)0x55);
        box.Values.After.ShouldBe((byte)0xaa);
        box.Values.Byte.ShouldBe(unchecked((byte)20000));
        box.Values.Short.ShouldBe((short)20000);
        box.Values.Wide.ShouldBe(20000L);
    }

    [Fact]
    public void Compare_exchange_reports_observed_bits_and_updates_expected_on_failure()
    {
        short value = 17, expected = 5;
        Atomic.ValueCompareExchange(ref value, (short)20, (short)5).ShouldBe((short)17);
        value.ShouldBe((short)17);
        GnuAtomic.CompareExchange(ref value, ref expected, (short)20, false, 5, 0).ShouldBeFalse();
        expected.ShouldBe((short)17);
        GnuAtomic.CompareExchange(ref value, ref expected, (short)20, true, 4, 2).ShouldBeTrue();
        value.ShouldBe((short)20);
        expected.ShouldBe((short)17);
        float minusZero = -0.0f;
        Atomic.CompareExchangeMatches(ref minusZero, 1.0f, 0.0f).ShouldBeFalse();
        BitConverter.SingleToInt32Bits(minusZero).ShouldBe(int.MinValue);
    }

    [Fact]
    public void Dynamic_invalid_memory_order_fails_before_modifying_storage()
    {
        int value = 7;
        Should.Throw<ArgumentOutOfRangeException>(() => GnuAtomic.Store(ref value, 9, 2));
        value.ShouldBe(7);
    }

    private static string Emit(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-gnu-intrinsic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "main.c");
        File.WriteAllText(path, source);
        try { return Compiler.EmitCSharp(new[] { path }); }
        finally { Directory.Delete(dir, recursive: true); }
    }
}

using System;
using System.IO;
using System.Text.RegularExpressions;
using DotCC.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class C11AtomicOrderTests
{
    [Theory]
    [InlineData("return atomic_load_explicit(&value, mark());", "Load", 1)]
    [InlineData("atomic_store_explicit(&value, 2, mark()); return 0;", "Store", 1)]
    [InlineData("return atomic_exchange_explicit(&value, 2, mark());", "Exchange", 1)]
    [InlineData("return atomic_compare_exchange_strong_explicit(&value, &expected, 2, mark(), mark());", "CompareExchange", 2)]
    [InlineData("return atomic_compare_exchange_weak_explicit(&value, &expected, 2, mark(), mark());", "CompareExchange", 2)]
    [InlineData("return atomic_fetch_add_explicit(&value, 2, mark());", "FetchAdd", 1)]
    [InlineData("return atomic_fetch_sub_explicit(&value, 2, mark());", "FetchSub", 1)]
    [InlineData("return atomic_fetch_and_explicit(&value, 2, mark());", "FetchAnd", 1)]
    [InlineData("return atomic_fetch_or_explicit(&value, 2, mark());", "FetchOr", 1)]
    [InlineData("return atomic_fetch_xor_explicit(&value, 2, mark());", "FetchXor", 1)]
    [InlineData("return atomic_flag_test_and_set_explicit(&flag, mark());", "Exchange", 1)]
    [InlineData("atomic_flag_clear_explicit(&flag, mark()); return 0;", "Store", 1)]
    [InlineData("atomic_thread_fence(mark()); return 0;", "ThreadFence", 1)]
    [InlineData("atomic_signal_fence(mark()); return 0;", "SignalFence", 1)]
    public void Explicit_scalar_operations_keep_every_order_argument_once(string operation, string helper, int orders)
    {
        var path = Write("""
            #include <stdatomic.h>
            int mark(void) { return memory_order_seq_cst; }
            """ + "int action(void) { atomic_int value = 1; int expected = 1; atomic_flag flag = ATOMIC_FLAG_INIT; "
            + operation + " }");
        try
        {
            var code = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib);
            code.ShouldContain("GnuAtomic." + helper + "(");
            // One method declaration plus the actual source order arguments.
            Regex.Matches(code, @"\bmark\(\)").Count.ShouldBe(orders + 1);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Bool_load_store_keep_plain_access_while_enum_uses_atomic_storage()
    {
        var path = Write("""
            #include <stdatomic.h>
            enum Phase { Start = 3, End = 7 };
            _Bool load_bool(atomic_bool *p, int order) { return atomic_load_explicit(p, order); }
            void store_bool(atomic_bool *p, _Bool value, int order) { atomic_store_explicit(p, value, order); }
            enum Phase load_enum(_Atomic(enum Phase) *p, int order) { return atomic_load_explicit(p, order); }
            void store_enum(_Atomic(enum Phase) *p, enum Phase value, int order) { atomic_store_explicit(p, value, order); }
            """);
        try
        {
            var code = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib);
            code.ShouldContain("GnuAtomic.PlainLoad(ref *(p), (int)(order))");
            code.ShouldContain("GnuAtomic.PlainStore(ref *(p), (CBool)(value), (int)(order))");
            code.ShouldContain("GnuAtomic.Load(ref *(p), (int)(order))");
            code.ShouldContain("GnuAtomic.Store(ref *(p), (Phase)(value), (int)(order))");
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Invalid_orders_are_rejected_before_mutating_scalar_or_expected_storage()
    {
        int value = 11, expected = 7;
        Should.Throw<ArgumentOutOfRangeException>(() => GnuAtomic.Store(ref value, 99, 2));
        Should.Throw<ArgumentOutOfRangeException>(() => GnuAtomic.Load(ref value, 3));
        Should.Throw<ArgumentOutOfRangeException>(() => GnuAtomic.Exchange(ref value, 99, 6));
        Should.Throw<ArgumentOutOfRangeException>(() => GnuAtomic.CompareExchange(ref value, ref expected, 99, false, 3, 2));
        value.ShouldBe(11);
        expected.ShouldBe(7);
        GnuAtomic.CompareExchange(ref value, ref expected, 99, false, 4, 2).ShouldBeFalse();
        expected.ShouldBe(11);
        GnuAtomic.CompareExchange(ref value, ref expected, 99, true, 4, 2).ShouldBeTrue();
        value.ShouldBe(99);
        expected.ShouldBe(11);
    }

    [Fact]
    public void Plain_fallback_retains_value_semantics_and_validates_before_access()
    {
        bool value = false;
        GnuAtomic.PlainStore(ref value, true, 3);
        GnuAtomic.PlainLoad(ref value, 2).ShouldBeTrue();
        Should.Throw<ArgumentOutOfRangeException>(() => GnuAtomic.PlainStore(ref value, false, 2));
        Should.Throw<ArgumentOutOfRangeException>(() => GnuAtomic.PlainLoad(ref value, 4));
        value.ShouldBeTrue();
    }

    [Fact]
    public void Fence_guards_accept_all_C11_orders_and_reject_invalid_values()
    {
        for (int order = 0; order <= 5; order++)
        {
            GnuAtomic.ThreadFence(order);
            GnuAtomic.SignalFence(order);
        }
        foreach (int order in new[] { -1, 6 })
        {
            Should.Throw<ArgumentOutOfRangeException>(() => GnuAtomic.ThreadFence(order));
            Should.Throw<ArgumentOutOfRangeException>(() => GnuAtomic.SignalFence(order));
        }
    }

    private static string Write(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-c11-order-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        return path;
    }
}

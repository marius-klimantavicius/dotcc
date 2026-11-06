using System;
using System.IO;
using DotCC.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigExactShiftTests
{
    [Theory]
    [InlineData("@shlExact(@as(u8, 128), 1)", "overflow")]
    [InlineData("@shlExact(@as(i8, 64), 1)", "overflow")]
    [InlineData("@shlExact(@as(i8, -65), 1)", "overflow")]
    [InlineData("@shlExact(@as(u3, 2), 2)", "overflow")]
    [InlineData("@shlExact(@shlExact(@as(u3, 1), 1), 2)", "overflow")]
    [InlineData("@shlExact(@as(i3, 1), 2)", "overflow")]
    [InlineData("@shlExact(@as(u8, 1), -1)", "shift amount")]
    [InlineData("@shlExact(@as(u8, 0), 8)", "shift amount")]
    [InlineData("@shlExact(@as(u3, 0), 3)", "shift amount")]
    [InlineData("@shlExact(@as(u64, 0), 4294967296)", "shift amount")]
    [InlineData("@shlExact(@as(u128, 1), 128)", "shift amount")]
    [InlineData("@shlExact(@as(u128, 1), 340282366920938463463374607431768211455)", "shift amount")]
    [InlineData("@shlExact(@as(u128, 170141183460469231731687303715884105728), 1)", "overflow")]
    [InlineData("@shlExact(@as(f32, 1), 1)", "fixed-width scalar integer")]
    [InlineData("@shlExact(1, 2)", "fixed-width scalar integer")]
    [InlineData("@shlExact(@as(u3, 1) + @as(u5, 1), 1)", "known logical integer width")]
    [InlineData("@shlExact(@as(u8, 1))", "expects (value, shift)")]
    public void Invalid_or_unsupported_constant_shifts_fail_explicitly(string expression, string diagnostic)
        => WithSource("pub fn main() u8 { return @intCast(" + expression + "); }", path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain(diagnostic));

    [Theory]
    [InlineData("u8", "u3", "128", "1", "overflow")]
    [InlineData("i8", "u3", "-65", "1", "overflow")]
    [InlineData("u3", "u2", "2", "2", "overflow")]
    [InlineData("u3", "u2", "0", "3", "shift amount")]
    public void Interpreter_known_operands_preserve_logical_width_and_errors(string type, string countType, string value, string count, string diagnostic)
        => WithSource($"fn shift(value: {type}, count: {countType}) {type} {{ return @shlExact(value, count); }} comptime {{ _ = shift({value}, {count}); }} pub fn main() u8 {{ return 42; }}", path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain(diagnostic));

    [Fact]
    public void Managed_runtime_checks_ranges_without_process_traps()
    {
        ZigMath.ShlExact<byte>(1, 7, 8).ShouldBe((byte)128);
        ZigMath.ShlExact<sbyte>(-1, 7, 8).ShouldBe((sbyte)-128);
        ZigMath.ShlExact<byte>(1, 2, 3).ShouldBe((byte)4);
        ZigMath.ShlExact<sbyte>(-1, 2, 3).ShouldBe((sbyte)-4);
        ZigMath.ShlExact<ulong>(1, 63, 64).ShouldBe(1UL << 63);
        ZigMath.ShlExact<UInt128>(1, 127, 128).ShouldBe((UInt128)1 << 127);
        ZigMath.ShlExact<Int128>(-1, 127, 128).ShouldBe(Int128.MinValue);
        Should.Throw<OverflowException>(() => ZigMath.ShlExact<byte>(128, 1, 8));
        Should.Throw<OverflowException>(() => ZigMath.ShlExact<sbyte>(64, 1, 8));
        Should.Throw<OverflowException>(() => ZigMath.ShlExact<sbyte>(-65, 1, 8));
        Should.Throw<OverflowException>(() => ZigMath.ShlExact<byte>(2, 2, 3));
        Should.Throw<OverflowException>(() => ZigMath.ShlExact<sbyte>(1, 2, 3));
        Should.Throw<OverflowException>(() => ZigMath.ShlExact<byte>(8, 0, 3));
        Should.Throw<OverflowException>(() => ZigMath.ShlExact<UInt128>(UInt128.MaxValue, 1, 128));
        Should.Throw<ArgumentOutOfRangeException>(() => ZigMath.ShlExact<byte>(0, -1, 8));
        Should.Throw<ArgumentOutOfRangeException>(() => ZigMath.ShlExact<byte>(0, 8, 8));
        Should.Throw<ArgumentOutOfRangeException>(() => ZigMath.ShlExact<ulong>(0, (Int128)1 << 64, 64));
        Should.Throw<NotSupportedException>(() => ZigMath.ShlExact<byte>(0, 0, 129));
        Should.Throw<NotSupportedException>(() => ZigMath.ShlExact<byte>(0, 0, 9));
    }

    [Fact]
    public void An_untyped_named_constant_does_not_acquire_its_carrier_width()
        => WithSource("pub fn main() u8 { const value = 1; return @shlExact(value, 2); }", path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("known logical integer width"));

    private static void WithSource(string source, Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-exact-shift-unit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, source);
            test(path);
        }
        finally { Directory.Delete(directory, true); }
    }
}

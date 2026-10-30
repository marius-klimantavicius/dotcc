using System;
using System.Collections.Generic;
using System.IO;
using DotCC.Backends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigDivCeilTests
{
    [Theory]
    [InlineData("@divCeil(@as(u8, 7), 0)", "division by zero")]
    [InlineData("@divCeil(@as(u8, 7), -1)", "does not fit")]
    [InlineData("@divCeil(@as(i8, -128), -1)", "overflow")]
    [InlineData("@divCeil(@as(i16, -32768), -1)", "overflow")]
    [InlineData("@divCeil(@as(i64, -9223372036854775808), -1)", "overflow")]
    [InlineData("@divCeil(@as(i128, -170141183460469231731687303715884105728), -1)", "overflow")]
    [InlineData("@divCeil(@as(i3, -4), -1)", "overflow")]
    [InlineData("@divCeil(@divCeil(@as(i3, -4), 1), -1)", "overflow")]
    [InlineData("@divCeil(@as(f64, 7.0), 3.0)", "integer operands only")]
    [InlineData("@divCeil(7)", "expects (a, b)")]
    public void Known_invalid_operands_and_unsupported_float_do_not_emit(string expression, string diagnostic)
    {
        WithSource("pub fn main() u8 { return @intCast(" + expression + "); }", path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain(diagnostic));
    }

    [Fact]
    public void A_known_invalid_divisor_is_rejected_with_a_runtime_numerator()
    {
        WithSource("fn calc(a: u8) u8 { return @divCeil(a, -1); } pub fn main() u8 { return calc(7); }",
            path => Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("does not fit"));
    }

    [Fact]
    public void An_untyped_quotient_can_exceed_the_literal_carriers_range()
    {
        WithSource("pub fn main() u8 { const value = @divCeil(2147483647, 1) + 1; return @intCast(@divCeil(value, 2147483647)); }",
            path => Compiler.EmitObject(path).ShouldNotBeNullOrWhiteSpace());
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void Interpreter_restores_unsigned128_bit_patterns(long divisor)
    {
        var ir = new IrBuilder(null, new CSharpNameLegalizer());
        var symbol = new Symbol { Name = "high", Kind = SymKind.Var, Type = CType.UInt128 };
        ir.ComptimeGlobals[symbol] = new IrBuilder.CtInt(-1, CType.UInt128);
        var right = new Symbol { Name = "divisor", Kind = SymKind.Var, Type = CType.UInt128 };
        ir.ComptimeGlobals[right] = new IrBuilder.CtInt(divisor, CType.UInt128);
        var call = new Call("ZigMath.DivCeil", new List<CExpr> {
            new VarRef(symbol) { Type = CType.UInt128 }, new VarRef(right) { Type = CType.UInt128 },
            new LitInt("128", 128) { Type = CType.Int },
        }) { Type = CType.UInt128 };
        ir.ConstEval128(call).ShouldBe(divisor == -2 ? (Int128)2 : Int128.MinValue);
    }

    private static void WithSource(string source, Action<string> test)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-divceil-" + Guid.NewGuid().ToString("N") + ".zig");
        File.WriteAllText(path, source);
        try { test(path); }
        finally { File.Delete(path); }
    }
}

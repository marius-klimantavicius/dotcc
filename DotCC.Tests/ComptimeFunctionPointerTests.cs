using DotCC.Backends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ComptimeFunctionPointerTests
{
    [Fact]
    public void Ordinary_addresses_roundtrip_symbolically_and_compare_by_symbol()
    {
        var ir = new IrBuilder(null, new CSharpNameLegalizer());
        var type = new CType.Func(CType.Int, [CType.Int], false);
        var symbol = new Symbol { Name = "callback", TargetName = "first__callback", Kind = SymKind.Func, Type = type };
        var other = new Symbol { Name = "callback", TargetName = "second__callback", Kind = SymKind.Func, Type = type };
        var reference = new VarRef(symbol) { Type = type };
        var address = new Unary(UnOp.AddrOf, reference) { Type = new CType.Pointer(type) };
        var opaque = new Cast(new CType.Pointer(CType.Void), address) { Type = new CType.Pointer(CType.Void) };
        var restored = new Cast(type, opaque) { Type = type };
        var opaqueValue = ir.EvalComptimeValue(opaque).ShouldBeOfType<IrBuilder.CtFunction>();
        var splicedCast = ir.SpliceComptimeValue(opaqueValue).ShouldBeOfType<Cast>();
        splicedCast.Target.ShouldBe(opaque.Type);
        splicedCast.Operand.ShouldBeOfType<VarRef>().Sym.ShouldBeSameAs(symbol);
        var value = ir.EvalComptimeValue(restored).ShouldBeOfType<IrBuilder.CtFunction>();
        IrBuilder.ComptimeDigest(value).ShouldNotBe(IrBuilder.ComptimeDigest(new IrBuilder.CtFunction(other, type)));
        value.Function.ShouldBeSameAs(symbol);
        ir.SpliceComptimeValue(value).ShouldBeOfType<VarRef>().Sym.ShouldBeSameAs(symbol);
        ir.EvalComptimeValue(new Binary(BinOp.Eq, reference, restored) { Type = CType.Bool })
            .ShouldBeOfType<IrBuilder.CtBool>().Value.ShouldBeTrue();
        ir.EvalComptimeValue(new Binary(BinOp.Ne, reference, new VarRef(other) { Type = type }) { Type = CType.Bool })
            .ShouldBeOfType<IrBuilder.CtBool>().Value.ShouldBeTrue();
        ir.EvalComptimeValue(new Cast(new CType.Optional(type), reference) { Type = new CType.Optional(type) })
            .ShouldBeOfType<IrBuilder.CtFunction>().Function.ShouldBeSameAs(symbol);
    }

    [Fact]
    public void Symbolic_addresses_never_become_numbers_records_or_arithmetic_results()
    {
        var ir = new IrBuilder(null, new CSharpNameLegalizer());
        var type = new CType.Func(CType.Int, [], false);
        var symbol = new Symbol { Name = "callback", Kind = SymKind.Func, Type = type };
        var reference = new VarRef(symbol) { Type = type };
        foreach (var target in new CType[] { CType.Int, CType.ULong, CType.Double, new CType.Named("Record") })
            ir.EvalComptimeValue(new Cast(target, reference) { Type = target }).ShouldBeNull();
        ir.EvalComptimeValue(new Binary(BinOp.Add, reference, new LitInt("1", 1) { Type = CType.Int }) { Type = type }).ShouldBeNull();
        ir.EvalComptimeValue(new Binary(BinOp.Lt, reference, reference) { Type = CType.Bool }).ShouldBeNull();
        ir.EvalComptimeValue(new Cast(CType.Bool, reference) { Type = CType.Bool })
            .ShouldBeOfType<IrBuilder.CtBool>().Value.ShouldBeTrue();
    }

    [Fact]
    public void A_signature_changing_cast_cannot_invoke_the_original_body()
    {
        var ir = new IrBuilder(null, new CSharpNameLegalizer());
        var type = new CType.Func(CType.Int, [], false);
        var symbol = new Symbol { Name = "callback", Kind = SymKind.Func, Type = type };
        ir.Functions.Add(new FuncDef(symbol, [], new Block([new Return(new LitInt("42", 42) { Type = CType.Int })]), false));
        ir.EvalComptimeValue(new IndirectCall(new VarRef(symbol) { Type = type }, []) { Type = CType.Int })
            .ShouldBeOfType<IrBuilder.CtInt>().Value.ShouldBe(42);
        var wrong = new CType.Func(CType.Double, [], false);
        var cast = new Cast(wrong, new VarRef(symbol) { Type = type }) { Type = wrong };
        ir.EvalComptimeValue(new IndirectCall(cast, []) { Type = CType.Double }).ShouldBeNull();
    }
}

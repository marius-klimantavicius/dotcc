using DotCC.Backends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ZigComptimeVectorLoadTests
{
    [Fact]
    public void Pure_vector_load_reads_only_logical_source_lanes_in_order()
    {
        var unit = new IrBuilder(null, new CSharpNameLegalizer());
        var elements = new IrBuilder.ComptimeValue[] {
            new IrBuilder.CtInt(3, CType.UChar), new IrBuilder.CtInt(4, CType.UChar), new IrBuilder.CtInt(5, CType.UChar) };
        var symbol = new Symbol { Name = "lanes", TargetName = "lanes", Kind = SymKind.Var, Type = new CType.Array(CType.UChar, 3) };
        unit.ComptimeGlobals[symbol] = new IrBuilder.CtArray(elements, CType.UChar, symbol.Type);
        var call = new Call("ZigVec.Load64", [new VarRef(symbol) { Type = symbol.Type }]) { Type = new CType.Vector(CType.UChar, 3) };
        var result = unit.EvalComptimeValue(call).ShouldBeOfType<IrBuilder.CtArray>();
        result.Elems.Length.ShouldBe(3);
        ((IrBuilder.CtInt)result.Elems[0]).Value.ShouldBe(3);
        ((IrBuilder.CtInt)result.Elems[1]).Value.ShouldBe(4);
        ((IrBuilder.CtInt)result.Elems[2]).Value.ShouldBe(5);
        elements[0] = new IrBuilder.CtInt(99, CType.UChar);
        ((IrBuilder.CtInt)result.Elems[0]).Value.ShouldBe(3);
    }

    [Theory]
    [InlineData(false, "out of bounds")]
    [InlineData(true, "undefined")]
    public void Required_loads_reject_missing_or_undefined_source_lanes(bool undefined, string diagnostic)
    {
        var unit = new IrBuilder(null, new CSharpNameLegalizer());
        var type = new CType.Array(CType.UChar, undefined ? 2 : 1);
        var symbol = new Symbol { Name = "lanes", TargetName = "lanes", Kind = SymKind.Var, Type = type };
        IrBuilder.ComptimeValue[] elements = undefined
            ? [new IrBuilder.CtInt(1, CType.UChar), new IrBuilder.CtUndefined(CType.UChar)]
            : [new IrBuilder.CtInt(1, CType.UChar)];
        unit.ComptimeGlobals[symbol] = new IrBuilder.CtArray(elements, CType.UChar, type);
        var call = new Call("ZigVec.Load64", [new VarRef(symbol) { Type = type }]) { Type = new CType.Vector(CType.UChar, 2) };
        unit.EvalComptimeValue(call).ShouldBeNull();
        unit.ComptimeMiss.ShouldNotBeNull().ShouldContain(diagnostic);
    }
}

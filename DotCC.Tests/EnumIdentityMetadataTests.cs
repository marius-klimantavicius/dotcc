using System;
using System.Linq;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class EnumIdentityMetadataTests
{
    [Fact]
    public void Enum_shape_preserves_values_and_underlying_type_but_not_member_order()
    {
        var first = new EnumTypeDef("Token", CType.Int, [new("A", 19), new("B", 23)]);
        var reordered = first with { Members = first.Members.Reverse().ToArray() };
        AggregateIdentities.EnumShape(first).ShouldBe(AggregateIdentities.EnumShape(reordered));
        AggregateIdentities.EnumShape(first).ShouldNotBe(AggregateIdentities.EnumShape(first with { Members = [new("A", 7), new("B", 23)] }));
        AggregateIdentities.EnumShape(first).ShouldNotBe(AggregateIdentities.EnumShape(first with { Underlying = CType.ULong }));
    }

    [Fact]
    public void Enum_kind_and_external_callback_edges_roundtrip_without_type_erasure()
    {
        var shape = AggregateIdentities.EnumShape(new("Token", CType.Int, [new("A", 19)]));
        var node = new AggregateIdentity("Token", "Token", true, true, [], shape, IsEnum: true);
        var type = new CType.Func(CType.Int, [new CType.Pointer(new CType.Enum("Token", CType.Int))], Variadic: false);
        var root = AggregateIdentities.LinkageContract("callback", type);
        root.References.ShouldBe(new[] { "Token" });
        root.Type.ShouldContain("enum(");
        var graph = new AggregateIdentitySet([node], [root]);
        string text = "//!!dotcc-obj aggregate:Token 0 0 " + shape.Signature + "\n" + AggregateIdentities.Serialize(graph);
        var restored = AggregateIdentities.Read(text).ShouldBeOfType<AggregateIdentitySet>();
        restored.Single().IsEnum.ShouldBeTrue();
        restored.Linkage.Single().Type.ShouldBe(root.Type);
    }

    [Fact]
    public void Isolated_enum_layouts_partition_but_a_shared_unused_symbol_rejects_value_changes()
    {
        AggregateIdentitySet Graph(string symbol, long value) => new([
            new("Token", "Token", true, true, [], AggregateIdentities.EnumShape(new("Token", CType.Int, [new("A", value)])), true)],
            [AggregateIdentities.LinkageContract(symbol, new CType.Enum("Token", CType.Int))]);
        var privatePlans = AggregateIdentities.Plan(new (string, System.Collections.Generic.IReadOnlyList<AggregateIdentity>?)[] {
            ("first.c", Graph("first", 19)), ("second.c", Graph("second", 23)) });
        privatePlans[0]["Token"].ShouldNotBe(privatePlans[1]["Token"]);
        Should.Throw<CompileException>(() => AggregateIdentities.Plan(new (string, System.Collections.Generic.IReadOnlyList<AggregateIdentity>?)[] {
            ("first.c", Graph("shared", 19)), ("second.c", Graph("shared", 23)) })).Message.ShouldContain("conflicting aggregate declarations");
    }
}

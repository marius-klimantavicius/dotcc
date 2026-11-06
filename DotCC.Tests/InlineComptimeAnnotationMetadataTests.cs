using DotCC.Backends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class InlineComptimeAnnotationMetadataTests
{
    [Fact]
    public void Source_only_annotations_withhold_equivalence_without_erasing_function_body()
    {
        var unit = new IrBuilder(null, new CSharpNameLegalizer());
        var sourceType = new CType.ZigGenericFunctionType("callback-type");
        // std.Options carries a generic logFn even when the expression reads
        // only a runtime-valued option from its comptime initializer.
        var config = new StructInit([
            new FieldInit("callback", sourceType, new DefaultLit { Type = sourceType }),
            new FieldInit("level", CType.Int, new LitInt("42", 42) { Type = CType.Int }),
        ]) { Type = new CType.Named("Config") };
        var function = Function(new Member(config, "level", false) { Type = CType.Int });
        var originalBody = function.Body;
        var metadata = InlineFunctionMetadata.From(function, unit, false);
        metadata.Shape.ShouldBe("-");
        function.Body.ShouldBeSameAs(originalBody);
        InlineFunctionMetadata.Parse(metadata.Serialize("selected")).Metadata.Shape.ShouldBe("-");
    }

    [Fact]
    public void Ordinary_bodies_still_supply_distinct_equivalence_shapes()
    {
        var unit = new IrBuilder(null, new CSharpNameLegalizer());
        var first = InlineFunctionMetadata.From(Function(new LitInt("40", 40) { Type = CType.Int }), unit, false);
        var second = InlineFunctionMetadata.From(Function(new LitInt("42", 42) { Type = CType.Int }), unit, false);
        first.Shape.ShouldNotBe("-");
        second.Shape.ShouldNotBe("-");
        first.Shape.ShouldNotBe(second.Shape);
    }

    private static FuncDef Function(CExpr value)
        => new(new Symbol { Name = "selected", TargetName = "selected", Kind = SymKind.Func,
                Type = new CType.Func(CType.Int, [], false), Storage = Storage.Static, IsInline = true },
            [], new Block([new Return(value)]), false);
}

using System.Linq;
using DotCC.Layout;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AlignmentLayoutTests
{
    [Fact]
    public void Explicit_member_and_aggregate_alignment_survive_metadata_and_nested_array_layout()
    {
        var document = new OffsetDocument();
        var inner = new LayoutAggregate { Name = "Inner", Alignment = 32 };
        inner.Fields.Add(new LayoutField { Name = "prefix", Type = "p:1:1" });
        inner.Fields.Add(new LayoutField { Name = "aligned", Type = "p:1:1", Alignment = 16 });
        var outer = new LayoutAggregate { Name = "Outer" };
        outer.Fields.Add(new LayoutField { Name = "prefix", Type = "p:1:1" });
        outer.Fields.Add(new LayoutField { Name = "items", Type = "a:2:n:Inner" });
        outer.Fields.Add(new LayoutField { Name = "tail", Type = "p:1:1" });
        document.Aggregates.Add(inner.Name, inner);
        document.Aggregates.Add(outer.Name, outer);
        var restored = OffsetDocument.ReadSource(document.Serialize()).Single();
        var model = new OffsetLayoutModel(name => restored.Aggregates[name]);
        model.Aggregate("Inner").Alignment.ShouldBe(32);
        model.Aggregate("Inner").Size.ShouldBe(32);
        model.Offset("Inner", new[] { "aligned" }).ShouldBe(16);
        model.Aggregate("Outer").Alignment.ShouldBe(32);
        model.Aggregate("Outer").Size.ShouldBe(128);
        model.Offset("Outer", new[] { "items", "[1]", "aligned" }).ShouldBe(80);
        model.Offset("Outer", new[] { "tail" }).ShouldBe(96);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(256)]
    public void Unsupported_alignment_is_rejected_by_shared_model(int alignment)
    {
        var aggregate = new LayoutAggregate { Name = "Invalid", Alignment = alignment };
        aggregate.Fields.Add(new LayoutField { Name = "value", Type = "p:1:1" });
        Should.Throw<OffsetLayoutException>(() => new OffsetLayoutModel(_ => aggregate).Aggregate(aggregate.Name));
    }
}

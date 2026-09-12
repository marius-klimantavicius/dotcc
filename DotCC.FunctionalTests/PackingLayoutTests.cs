using System.Linq;
using DotCC.Layout;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class PackingLayoutTests
{
    [Theory]
    [InlineData(0, 8, 7, 4)]
    [InlineData(1, 6, 5, 1)]
    [InlineData(2, 6, 5, 2)]
    [InlineData(4, 8, 5, 4)]
    [InlineData(8, 8, 5, 4)]
    [InlineData(16, 8, 5, 4)]
    public void Explicit_pack_allows_bitfields_to_cross_declared_units(int pack, int size, int tail, int alignment)
    {
        var aggregate = new LayoutAggregate { Name = "Bits", Pack = pack };
        aggregate.Fields.Add(new LayoutField { Name = "a", Type = "p:4:4", BitWidth = 20 });
        aggregate.Fields.Add(new LayoutField { Name = "b", Type = "p:4:4", BitWidth = 20 });
        aggregate.Fields.Add(new LayoutField { Name = "end", Type = "p:1:1" });
        var document = new OffsetDocument();
        document.Aggregates.Add(aggregate.Name, aggregate);
        var restored = OffsetDocument.ReadSource(document.Serialize()).Single();
        restored.Aggregates[aggregate.Name].Pack.ShouldBe(pack);
        var layout = new OffsetLayoutModel(name => restored.Aggregates[name]).Aggregate(aggregate.Name);
        layout.Size.ShouldBe(size);
        layout.Alignment.ShouldBe(alignment);
        layout.Offsets["end"].ShouldBe(tail);
        if (pack != 0)
        {
            layout.BitFields[1].ShouldBe((2, 3, 4));
            layout.BitFieldScalars.ShouldBeEmpty();
        }
    }

    [Fact]
    public void Sixty_four_bit_packed_member_can_span_nine_bytes()
    {
        var aggregate = new LayoutAggregate { Name = "Wide", Pack = 1 };
        aggregate.Fields.Add(new LayoutField { Name = "prefix", Type = "p:4:4", BitWidth = 3 });
        aggregate.Fields.Add(new LayoutField { Name = "value", Type = "p:8:8", BitWidth = 64 });
        aggregate.Fields.Add(new LayoutField { Name = "tail", Type = "p:4:4", BitWidth = 5 });
        var layout = new OffsetLayoutModel(_ => aggregate).Aggregate(aggregate.Name);
        layout.Size.ShouldBe(9);
        layout.Alignment.ShouldBe(1);
        layout.BitFields[1].ShouldBe((0, 9, 3));
    }

    [Fact]
    public void Zero_width_boundaries_keep_natural_alignment_without_raising_aggregate_alignment()
    {
        var aggregate = new LayoutAggregate { Name = "Zero", Pack = 1 };
        aggregate.Fields.Add(new LayoutField { Name = "prefix", Type = "p:1:1" });
        aggregate.Fields.Add(new LayoutField { Name = "bits", Type = "p:4:4", BitWidth = 5 });
        aggregate.Fields.Add(new LayoutField { Name = "", Type = "p:4:4", BitWidth = 0 });
        aggregate.Fields.Add(new LayoutField { Name = "tail", Type = "p:1:1" });
        var layout = new OffsetLayoutModel(_ => aggregate).Aggregate(aggregate.Name);
        layout.Size.ShouldBe(5);
        layout.Alignment.ShouldBe(1);
        layout.Offsets["tail"].ShouldBe(4);
    }
}

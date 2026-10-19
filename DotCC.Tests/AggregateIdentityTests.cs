using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class AggregateIdentityTests
{
    private static AggregateIdentity Node(string name, string shape, string[]? deps = null, bool exposed = false,
        bool incomplete = false, bool tagged = true) => new(name, name, tagged, exposed, deps ?? Array.Empty<string>(),
            new ObjectAggregateMetadata(incomplete, false, AggregateIdentities.Hash(shape)));

    [Fact]
    public void Conflict_partition_follows_callback_cycles_and_leaves_shared_types_unified()
    {
        var first = new[] { Node("State", "small", new[] { "Codec" }), Node("Codec", "codec", new[] { "State" }), Node("Public", "public", exposed: true) };
        var second = new[] { Node("State", "large", new[] { "Codec" }), Node("Codec", "codec", new[] { "State" }), Node("Public", "public", exposed: true) };
        var plans = AggregateIdentities.Plan(new (string, IReadOnlyList<AggregateIdentity>?)[] { ("a.c", first), ("b.c", second) });
        plans[0].Count.ShouldBe(2); plans[1].Count.ShouldBe(2);
        plans[0]["State"].ShouldNotBe(plans[1]["State"]);
        plans[0].ContainsKey("Public").ShouldBeFalse();
        var reverse = AggregateIdentities.Plan(new (string, IReadOnlyList<AggregateIdentity>?)[] { ("b.c", second), ("a.c", first) });
        reverse[1]["Codec"].ShouldBe(plans[0]["Codec"]);
    }

    [Fact]
    public void Forward_completion_and_equal_shared_definitions_do_not_partition()
    {
        var plans = AggregateIdentities.Plan(new (string, IReadOnlyList<AggregateIdentity>?)[] {
            ("a.c", new[] {Node("State", "opaque", incomplete:true, exposed:true)}),
            ("b.c", new[] {Node("State", "complete", exposed:true)}),
            ("c.c", new[] {Node("State", "complete", exposed:true)}) });
        foreach (var plan in plans) plan.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Shared_abi_and_conflicting_same_origin_anonymous_definitions_remain_rejected(bool exposed)
    {
        var units = new (string, IReadOnlyList<AggregateIdentity>?)[] {
            ("a.c",new[]{Node("State","small",exposed:exposed,tagged:exposed)}),
            ("b.c",new[]{Node("State","large",exposed:exposed,tagged:exposed)}) };
        var error=Should.Throw<CompileException>(()=>AggregateIdentities.Plan(units));
        error.Message.ShouldContain("a.c");error.Message.ShouldContain("b.c");
    }

    [Fact]
    public void Graph_roundtrip_requires_complete_dependencies_and_preserves_exposure()
    {
        var shape=Node("State","value",new[]{"State"},exposed:true);
        var source="//!!dotcc-obj aggregate:State 0 0 "+shape.Shape.Signature+"\n"+AggregateIdentities.Serialize(new[]{shape});
        var graph=AggregateIdentities.Read(source)!;
        graph.Count.ShouldBe(1);graph[0].Exposed.ShouldBeTrue();graph[0].Dependencies.ShouldBe(new[]{"State"});
        Should.Throw<CompileException>(()=>AggregateIdentities.Read(source.Replace(" 1 State\n"," 1 Missing\n",StringComparison.Ordinal)));
        Should.Throw<CompileException>(()=>AggregateIdentities.Read(source.Replace(shape.Shape.Signature,"bad",StringComparison.Ordinal)));
    }

    [Fact]
    public void Relocations_touch_type_tokens_but_preserve_the_ordinary_identifier_namespace()
    {
        var token=AggregateIdentities.Token("State");
        var text=token+" State; // State "+token+"\nstring text=\"State\";";
        AggregateIdentities.Relocate(text,new[]{Node("State","small")},new Dictionary<string,string>{{"State","Private"}})
            .ShouldBe("Private State; // State "+token+"\nstring text=\"State\";");
    }
}

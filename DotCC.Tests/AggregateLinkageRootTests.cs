using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class AggregateLinkageRootTests
{
    private static AggregateIdentity Node(string name, string shape, bool exposed = true,
        bool incomplete = false, params string[] dependencies) => new(name, name, true, exposed, dependencies,
            new(incomplete, false, AggregateIdentities.Hash(shape)));
    private static AggregateLinkageContract Root(string symbol, string name) => new(symbol, "ptr:" + name, [name], []);
    private static AggregateIdentitySet Set(AggregateIdentity[] nodes, params AggregateLinkageContract[] roots) => new(nodes, roots);

    [Fact]
    public void Distinct_external_roots_partition_cycles_by_graph_and_unify_equal_components()
    {
        AggregateIdentitySet Graph(string symbol, string shape) => Set([
            Node("State", shape, dependencies: ["Codec"]), Node("Codec", "callback:State", dependencies: ["State"])], Root(symbol, "Codec"));
        var units = new (string, IReadOnlyList<AggregateIdentity>?)[] {
            ("regex-a.c", Graph("regex_a", "small")), ("zone-a.c", Graph("zone_a", "large")),
            ("regex-b.c", Graph("regex_b", "small")), ("zone-b.c", Graph("zone_b", "large")) };
        var plans = AggregateIdentities.Plan(units);
        plans[0]["State"].ShouldBe(plans[2]["State"]);
        plans[1]["State"].ShouldBe(plans[3]["State"]);
        plans[0]["State"].ShouldNotBe(plans[1]["State"]);
        plans[0]["Codec"].ShouldBe(plans[2]["Codec"]);
        plans[0]["Codec"].ShouldNotBe(plans[1]["Codec"]);
        var reverse = AggregateIdentities.Plan(units.Reverse().ToArray());
        reverse[3]["Codec"].ShouldBe(plans[0]["Codec"]);
    }

    [Fact]
    public void Opaque_exposed_parent_completes_one_unique_graph_despite_unrelated_child_tag()
    {
        var plans = AggregateIdentities.Plan(new (string, IReadOnlyList<AggregateIdentity>?)[] {
            ("opaque.c", Set([Node("Zone", "opaque", incomplete: true)], Root("make_zone", "Zone"))),
            ("zone.c", Set([Node("Zone", "contains:State", dependencies: ["State"]), Node("State", "large")], Root("make_zone", "Zone"))),
            ("regex.c", Set([Node("State", "small")], Root("regex_state", "State"))) });
        plans[0].ContainsKey("Zone").ShouldBeFalse();
        plans[1].ContainsKey("Zone").ShouldBeFalse();
        plans[1]["State"].ShouldNotBe(plans[2]["State"]);
    }

    [Fact]
    public void Shared_parent_root_preserves_transitive_child_conflicts()
    {
        AggregateIdentitySet Graph(string shape) => Set([
            Node("Parent", "pointer:State", dependencies: ["State"]), Node("State", shape)], Root("shared", "Parent"));
        var error = Should.Throw<CompileException>(() => AggregateIdentities.Plan(new (string, IReadOnlyList<AggregateIdentity>?)[] {
            ("a.c", Graph("small")), ("b.c", Graph("large")) }));
        error.Message.ShouldContain("'State'");
        error.Message.ShouldContain("a.c"); error.Message.ShouldContain("b.c");
    }

    [Fact]
    public void Semantic_roots_roundtrip_and_missing_exposure_proof_is_rejected()
    {
        var graph = Set([Node("State", "complete")], Root("unused_external", "State"));
        string text = "//!!dotcc-obj aggregate:State 0 0 " + graph[0].Shape.Signature + "\n" + AggregateIdentities.Serialize(graph);
        var restored = AggregateIdentities.Read(text).ShouldBeOfType<AggregateIdentitySet>();
        restored.Linkage.Single().Symbol.ShouldBe("unused_external");
        var missing = new AggregateIdentitySet(graph.Nodes, []);
        Should.Throw<CompileException>(() => AggregateIdentities.Plan(new (string, IReadOnlyList<AggregateIdentity>?)[] { ("missing.c", missing) }))
            .Message.ShouldContain("incomplete aggregate linkage roots");
    }

    [Fact]
    public void Mixed_legacy_metadata_keeps_the_conservative_exposed_conflict_gate()
    {
        Should.Throw<CompileException>(() => AggregateIdentities.Plan(new (string, IReadOnlyList<AggregateIdentity>?)[] {
            ("old.c", new[] { Node("State", "small") }),
            ("new.c", Set([Node("State", "large")], Root("unrelated", "State"))) }))
            .Message.ShouldContain("conflicting aggregate declarations");
    }

    [Fact]
    public void Unknown_array_bounds_complete_but_two_known_bounds_remain_incompatible()
    {
        AggregateIdentitySet Graph(int? count) => Set([Node("State", "same")],
            AggregateIdentities.LinkageContract("shared_array", new CType.Array(new CType.Named("State"), count)));
        AggregateIdentities.Plan(new (string, IReadOnlyList<AggregateIdentity>?)[] { ("a.c", Graph(null)), ("b.c", Graph(3)) });
        Should.Throw<CompileException>(() => AggregateIdentities.Plan(new (string, IReadOnlyList<AggregateIdentity>?)[] {
            ("a.c", Graph(3)), ("b.c", Graph(4)) })).Message.ShouldContain("conflicting external type contracts");
    }
}

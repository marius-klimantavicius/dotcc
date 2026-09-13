using System;
using System.Linq;
using DotCC.Layout;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class OffsetLayoutTests
{
    [Fact]
    public void object_link_preserves_layout_and_generated_constants()
    {
        var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dotcc-offset-link-" + Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(directory);
        try
        {
            var source = System.IO.Path.Combine(directory, "main.c");
            var fragment = System.IO.Path.Combine(directory, "main.cs");
            System.IO.File.WriteAllText(source, "struct S { char a; double b; }; int main(void) { return offsetof(struct S, b) == 8 ? 0 : 1; }");
            var emitted = DotCC.Compiler.EmitObject(source);
            emitted.ShouldContain("unsafe partial struct S");
            emitted.ShouldContain("dotcc-layout-v1");
            System.IO.File.WriteAllText(fragment, emitted);
            var linked = DotCC.Compiler.LinkObjects(new[] { fragment });
            linked.ShouldNotContain("DOTCC_OFFSET_GENERATOR");
            FixtureRunner.CompileAndRunCapturingExit(linked, Array.Empty<string>()).exit.ShouldBe(0);
        }
        finally { System.IO.Directory.Delete(directory, true); }
    }

    private static OffsetDocument Sample()
    {
        var document = new OffsetDocument();
        var aggregate = new LayoutAggregate { Name = "Sample" };
        aggregate.Fields.Add(new LayoutField { Name = "tag", Type = "p:1:1" });
        aggregate.Fields.Add(new LayoutField { Name = "data", Type = "a:2:p:8:8" });
        document.Aggregates.Add(aggregate.Name, aggregate);
        document.Requests.Add(new OffsetRequest { Name = "__DotccOffset_sample", Aggregate = "Sample", Path = new[] { "data" }, Expected = 8 });
        return document;
    }

    [Fact]
    public void direct_constant_emission_is_deterministic_and_metadata_round_trips()
    {
        var document = Sample();
        var declarations = document.Materialize();
        declarations.ShouldBe(document.Materialize());
        declarations.ShouldContain("public const ulong Value = 8UL;");
        declarations.ShouldContain("public const int Size = 24;");
        declarations.ShouldContain("public const int Alignment = 8;");
        OffsetDocument.ReadSource(document.Serialize()).Single().Serialize().ShouldBe(document.Serialize());
    }

    [Theory]
    [InlineData(EmitMode.File)]
    [InlineData(EmitMode.Csproj)]
    public void ordinary_compilation_needs_no_offset_analyzer(EmitMode mode)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dotcc-offset-direct-" + Guid.NewGuid().ToString("N") + ".c");
        System.IO.File.WriteAllText(path, """
            struct S { char tag; double value; int tail[]; };
            enum Offsets { OFFSET = offsetof(struct S, value) };
            _Static_assert(offsetof(struct S, tail) == 16, "tail offset");
            int main(void) {
                char storage[offsetof(struct S, value)];
                switch (sizeof(storage)) { case OFFSET: return sizeof(struct S) == 16 ? 0 : 1; }
                return 2;
            }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { path }, emit: mode);
            emitted.ShouldNotContain("DOTCC_OFFSET_GENERATOR");
            emitted.ShouldContain("public const ulong Value = 8UL;");
            FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>()).exit.ShouldBe(0);
        }
        finally { System.IO.File.Delete(path); }
    }

    [Theory]
    [InlineData("abi\tunknown\n")]
    [InlineData("abi\tlp64-le-dotcc-v1\nrequest\tinvalid\tUw==\tYQ==\t0\n")]
    [InlineData("abi\tlp64-le-dotcc-v1\nfield\tYQ==\tp:4:4\t-\n")]
    public void malformed_metadata_is_rejected(string body)
    {
        Should.Throw<OffsetLayoutException>(() => OffsetDocument.ReadSource(OffsetDocument.Start + body + OffsetDocument.End).ToArray());
    }

    [Fact]
    public void constant_emission_rejects_disagreement_with_folded_value()
    {
        var document = Sample();
        document.Requests[0].Expected = 12;
        Should.Throw<OffsetLayoutException>(() => document.Materialize()).Message.ShouldContain("disagreement");
    }

    [Fact]
    public void bitfields_share_lowered_storage_but_cannot_be_addressed()
    {
        var aggregate = new LayoutAggregate { Name = "Bits" };
        aggregate.Fields.Add(new LayoutField { Name = "a", Type = "p:4:4", BitWidth = 3 });
        aggregate.Fields.Add(new LayoutField { Name = "b", Type = "p:4:4", BitWidth = 5 });
        aggregate.Fields.Add(new LayoutField { Name = "after", Type = "p:4:4" });
        var model = new OffsetLayoutModel(_ => aggregate);
        model.Offset("Bits", new[] { "after" }).ShouldBe(4);
        model.Aggregate("Bits").Size.ShouldBe(8);
        Should.Throw<OffsetLayoutException>(() => model.Offset("Bits", new[] { "a" })).Message.ShouldContain("bit-field");
    }

    [Fact]
    public void unknown_layout_is_not_silently_zero()
    {
        var model = new OffsetLayoutModel(name => throw new OffsetLayoutException("Unknown aggregate: " + name));
        Should.Throw<OffsetLayoutException>(() => model.Type("n:Missing")).Message.ShouldContain("Missing");
    }
}

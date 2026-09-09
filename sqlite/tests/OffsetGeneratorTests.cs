using System;
using System.Linq;
using DotCC.Layout;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class OffsetGeneratorTests
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
            emitted.ShouldContain("unsafe struct S");
            emitted.ShouldContain("dotcc-layout-v1");
            System.IO.File.WriteAllText(fragment, emitted);
            var linked = DotCC.Compiler.LinkObjects(new[] { fragment });
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

    private static GeneratorDriver Run(string source)
    {
        var parse = new CSharpParseOptions(LanguageVersion.Preview, preprocessorSymbols: new[] { "DOTCC_OFFSET_GENERATOR" });
        var compilation = CSharpCompilation.Create("Offsets", new[] { CSharpSyntaxTree.ParseText(SourceText.From(source), parse) });
        return CSharpGeneratorDriver.Create(new[] { new DotCC.OffsetGenerator.OffsetGenerator().AsSourceGenerator() }, parseOptions: parse)
            .RunGenerators(compilation);
    }

    [Fact]
    public void generator_and_standalone_materializer_are_identical_and_deterministic()
    {
        var document = Sample();
        var source = document.Serialize() + "#if !DOTCC_OFFSET_GENERATOR\n" + document.Materialize() + "#endif\n";
        var first = Run(source).GetRunResult();
        var second = Run(source).GetRunResult();
        first.Diagnostics.ShouldBeEmpty();
        first.GeneratedTrees.Single().ToString().ShouldBe(second.GeneratedTrees.Single().ToString());
        first.GeneratedTrees.Single().ToString().ShouldContain(document.Materialize());
        OffsetDocument.ReadSource(source).Single().Serialize().ShouldBe(document.Serialize());
    }

    [Theory]
    [InlineData("abi\tunknown\n")]
    [InlineData("abi\tlp64-le-dotcc-v1\nrequest\tinvalid\tUw==\tYQ==\t0\n")]
    [InlineData("abi\tlp64-le-dotcc-v1\nfield\tYQ==\tp:4:4\t-\n")]
    public void malformed_metadata_reports_actionable_diagnostic(string body)
    {
        var result = Run(OffsetDocument.Start + body + OffsetDocument.End).GetRunResult();
        result.Diagnostics.ShouldContain(d => d.Id == "DOTCCOFF001" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void generator_cross_checks_compiler_constant()
    {
        var document = Sample();
        document.Requests[0].Expected = 12;
        var result = Run(document.Serialize() + "#if !DOTCC_OFFSET_GENERATOR\n#endif\n").GetRunResult();
        result.Diagnostics.ShouldContain(d => d.Id == "DOTCCOFF001" && d.GetMessage().Contains("disagreement"));
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

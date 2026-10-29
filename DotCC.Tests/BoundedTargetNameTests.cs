using System;
using System.Text;
using DotCC.Backends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class BoundedTargetNameTests
{
    [Theory]
    [InlineData("ghostty_terminal_new", "ghostty_terminal_new")]
    [InlineData("new", "@new")]
    [InlineData("@new", "@new")]
    public void Ordinary_target_names_keep_their_existing_spelling(string source, string target)
        => EmitHelpers.Id(source).ShouldBe(target);

    [Fact]
    public void Complete_source_identity_determines_a_stable_bounded_name()
    {
        var prefix = "function_" + new string('x', 4096);
        var first = EmitHelpers.Id(prefix + "A");
        var second = EmitHelpers.Id(prefix + "B");
        first.ShouldNotBe(second);
        Encoding.UTF8.GetByteCount(first).ShouldBeLessThanOrEqualTo(256);
        first.ShouldBe(EmitHelpers.Id(prefix + "A"));
        EmitHelpers.Id(first).ShouldBe(first);
        new CSharpNameLegalizer().Escape(prefix + "A").ShouldBe(first);
        new CSharpTarget().RenderType(new CType.Named(prefix + "A")).ShouldBe(first);
    }

    [Fact]
    public void Metadata_limit_counts_utf8_bytes_and_prefix_preserves_complete_scalars()
    {
        var source = "type_" + new string('\u03bb', 150);
        var target = EmitHelpers.Id(source);
        target.ShouldNotBe(source);
        Encoding.UTF8.GetByteCount(target).ShouldBeLessThanOrEqualTo(256);
        var strict = new UTF8Encoding(false, true);
        strict.GetString(strict.GetBytes(target)).ShouldBe(target);
        EmitHelpers.Id(new string('x', 256)).ShouldBe(new string('x', 256));
    }

    [Fact]
    public void Authored_external_clr_type_names_keep_their_spelling()
    {
        var name = "Managed" + new string('x', 300);
        var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(),
            externalTypes: new[] { new ExternalTypeOverride(name) });
        options.ExternalTypes[0].Name.ShouldBe(name);
        new CSharpTarget().RenderType(new CType.Named(name) { IsExternal = true }).ShouldBe(name);
        var qualified = "global::Company." + name;
        new CSharpTarget().RenderType(new CType.Named(qualified) { IsExternal = true }).ShouldBe(qualified);
    }
}

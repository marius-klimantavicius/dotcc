#nullable enable

using System;
using System.IO;
using System.Linq;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed partial class CompilerTests
{
    [Fact]
    public void Small_stack_long_parameter_list_preserves_first_and_last_parameters()
    {
        string parameters = string.Join(",", Enumerable.Range(0, 4096).Select(index => "int p" + index));
        var source = WriteTemp("int sum(" + parameters + ") { return p0 + p4095; } int main(void) { return 0; }");
        try
        {
            RunOnSmallStack(() =>
            {
                var emitted = Compiler.EmitCSharp(new[] { source });
                emitted.ShouldContain("int p0, int p1");
                emitted.ShouldContain("int p4094, int p4095)");
                emitted.ShouldContain("return p0 + p4095;");
            });
        }
        finally { File.Delete(source); }
    }

    [Fact]
    public void Small_stack_long_declarator_list_matches_separate_declarations()
    {
        string[] declarators = Enumerable.Range(0, 4096).Select(index => "v" + index + "=" + index).ToArray();
        const string main = "int main(void) { return v0 + v4095; }";
        var source = WriteTemp(string.Concat(declarators.Select(declaration => "int " + declaration + ";")) + main);
        try
        {
            string expected = Compiler.EmitCSharp(new[] { source });
            File.WriteAllText(source, "int " + string.Join(",", declarators) + ";" + main);
            RunOnSmallStack(() => Compiler.EmitCSharp(new[] { source }).ShouldBe(expected));
        }
        finally { File.Delete(source); }
    }

    [Fact]
    public void Small_stack_large_enum_preserves_order_and_explicit_value_dependencies()
    {
        var entries = Enumerable.Range(0, 12000).Select(index => index switch
        {
            0 => "E0 = 7",
            6000 => "E6000 = E5999 + 5",
            _ => "E" + index,
        });
        var source = WriteTemp("enum Values { " + string.Join(",", entries) + ", }; int main(void) { return E11999; }");
        try
        {
            RunOnSmallStack(() =>
            {
                var emitted = Compiler.EmitCSharp(new[] { source });
                emitted.ShouldContain("E0 = 7");
                emitted.ShouldContain("E6000 = 6011");
                emitted.ShouldContain("E11999 = 12010");
                emitted.IndexOf("E0 = 7", StringComparison.Ordinal).ShouldBeLessThan(emitted.IndexOf("E6000 = 6011", StringComparison.Ordinal));
                emitted.IndexOf("E6000 = 6011", StringComparison.Ordinal).ShouldBeLessThan(emitted.IndexOf("E11999 = 12010", StringComparison.Ordinal));
            });
        }
        finally { File.Delete(source); }
    }

    [Theory]
    [InlineData("", "char")]
    [InlineData("u8", "char8_t")]
    [InlineData("u", "char16_t")]
    [InlineData("U", "char32_t")]
    [InlineData("L", "wchar_t")]
    public void Small_stack_adjacent_string_segments_match_one_literal(string prefix, string type)
    {
        const int count = 12000;
        string[] segments = Enumerable.Range(0, count).Select(index => index % 2 == 0 ? "a" : "Z").ToArray();
        string Program(string literal) => "int main(void) { const " + type + " *text = " + literal + "; return (int)text[11999]; }";
        var source = WriteTemp(Program(prefix + "\"" + string.Concat(segments) + "\""));
        try
        {
            // Equal output also checks all bytes/code units, order and the terminator.
            string expected = Compiler.EmitCSharp(new[] { source });
            File.WriteAllText(source, Program(string.Join(" ", segments.Select(segment => prefix + "\"" + segment + "\""))));
            RunOnSmallStack(() => Compiler.EmitCSharp(new[] { source }).ShouldBe(expected));
        }
        finally { File.Delete(source); }
    }
}

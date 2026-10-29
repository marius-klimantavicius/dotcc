using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace DotCC.Tests;

public sealed class AttributedTagTypedefTests
{
    [Theory]
    [InlineData("struct", false)]
    [InlineData("struct", true)]
    [InlineData("union", false)]
    [InlineData("union", true)]
    [InlineData("enum", false)]
    [InlineData("enum", true)]
    public void Tag_namespace_survives_balanced_attributes_in_both_rewriter_paths(string kind, bool typedef)
    {
        var body = kind == "enum" ? "First" : "Number Tag; Tag *next;";
        var source = $"typedef int Number; typedef struct Other Other; typedef {kind} Tag Tag; "
            + (typedef ? "typedef " : "") + kind
            + " __attribute__((packed, aligned(sizeof(Number))))"
            + " __attribute__((aligned(sizeof(struct Other)))) Tag { " + body + " }"
            + (typedef ? " Alias" : "") + "; Tag after;";
        using var lexer = LexerGrammar.C.FromString(source);
        using var rewriter = new TypeNameRewriter(lexer);
        var tokens = new List<(string? Name, string Symbol)>();
        var symbols = C.Definition.SymbolNames.ToDictionary(s => s.ID, s => s.Name);
        while (rewriter.MoveNext())
            tokens.Add((rewriter.Current.Content as string, symbols[rewriter.Current.ID]));

        Assert.Equal(kind == "enum" ? new[] { "ID", "ID", "ID", "TYPE_NAME" }
            : new[] { "ID", "ID", "ID", "ID", "TYPE_NAME", "TYPE_NAME" },
            tokens.Where(t => t.Name == "Tag").Select(t => t.Symbol));
        Assert.Equal(new[] { "ID", "ID", "ID" }, tokens.Where(t => t.Name == "Other").Select(t => t.Symbol));
        Assert.Equal(kind == "enum" ? new[] { "ID", "TYPE_NAME" } : new[] { "ID", "TYPE_NAME", "TYPE_NAME" },
            tokens.Where(t => t.Name == "Number").Select(t => t.Symbol));
    }

    [Theory]
    [InlineData("struct", false)]
    [InlineData("struct", true)]
    [InlineData("union", false)]
    [InlineData("union", true)]
    public void Attributed_definitions_preserve_typedef_parameters_and_member_names(string kind, bool typedef)
    {
        var source = "typedef int Number; typedef " + kind + " Header Header; "
            + (typedef ? "typedef " : "") + kind
            + " __attribute__((__packed__, aligned((sizeof(Number))))) Header { Number Header; Header *next; }"
            + (typedef ? " Header" : "") + "; "
            + "static Number read(Header *Header); Header *after; "
            + "static Number read(Header *Header) { return Header->Header; } Header *final;";
        WithSource(source, path => Assert.NotEmpty(Compiler.EmitObject(path)));
    }

    [Fact]
    public void An_attributed_tag_does_not_declare_a_typedef_name()
    {
        WithSource("struct __attribute__((packed)) Unknown { int value; }; Unknown value;", path =>
            Assert.Throws<CompileException>(() => Compiler.EmitObject(path)));
    }

    private static void WithSource(string source, Action<string> action)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-attributed-tag-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try { action(path); }
        finally { File.Delete(path); }
    }
}

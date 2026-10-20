using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class FunctionOverrideDirectCallTests
{
    [Theory]
    [InlineData(false, SourceSplit.None)]
    [InlineData(false, SourceSplit.Function)]
    [InlineData(true, SourceSplit.None)]
    [InlineData(true, SourceSplit.Function)]
    public void Intrinsic_is_emitted_in_the_actual_bound_direct_caller(bool objects, SourceSplit split)
    {
        WithSource(directory =>
        {
            var path = Path.Combine(directory, "probe.c");
            File.WriteAllText(path, """
                unsigned int selected(const unsigned char *p) { return 123; }
                unsigned int caller(const unsigned char *p) { return selected(p); }
                unsigned int (*address(int unused))(const unsigned char *) { return selected; }
                """);
            var options = Options();
            var files = Emit(path, options, objects, split);
            var body = Body(string.Join("\n", files.Values), "caller");
            body.ShouldContain("BinaryPrimitives.ReadUInt32LittleEndian");
            body.ShouldNotContain("selected(");
            var output = string.Join("\n", files.Values);
            output.ShouldContain(" selected("); // The original wrapper identity remains.
            Regex.IsMatch(output, @"&(?:[\w:.]+\.)?selected\b").ShouldBeTrue();
        });
    }

    [Theory]
    [InlineData(false, SourceSplit.None)]
    [InlineData(false, SourceSplit.Function)]
    [InlineData(true, SourceSplit.None)]
    [InlineData(true, SourceSplit.Function)]
    public void A_later_selected_declaration_updates_existing_bound_call_sites(bool objects, SourceSplit split)
    {
        WithSource(directory =>
        {
            var early = Path.Combine(directory, "early.h");
            var late = Path.Combine(directory, "late.h");
            File.WriteAllText(early, "unsigned int selected(const unsigned char *);\n");
            File.WriteAllText(late, "unsigned int selected(const unsigned char *);\n");
            var path = Path.Combine(directory, "probe.c");
            File.WriteAllText(path, """
                #include "early.h"
                unsigned int caller(const unsigned char *p) { return selected(p); }
                #include "late.h"
                unsigned int selected(const unsigned char *p) { return 123; }
                """);
            var options = Options(late);
            var output = string.Join("\n", Emit(path, options, objects, split).Values);
            Body(output, "caller").ShouldContain("BinaryPrimitives.ReadUInt32LittleEndian");
            Body(output, "caller").ShouldNotContain("selected(");
        });
    }

    [Fact]
    public void Managed_method_direct_calls_continue_using_the_original_typed_wrapper()
    {
        WithSource(directory =>
        {
            var path = Path.Combine(directory, "probe.c");
            File.WriteAllText(path, "unsigned int selected(const unsigned char *); unsigned int caller(const unsigned char *p) { return selected(p); }");
            var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[]
            {
                new FunctionOverride("selected", new("unsigned int", new[] { "const unsigned char *" }),
                    new("managedMethod", "global::Host.Read"), RequireMatch: true)
            });
            var output = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: options);
            Body(output, "caller").ShouldContain("selected(");
            Body(output, "caller").ShouldNotContain("global::Host.Read");
        });
    }

    private static CPreprocessingOptions Options(string? declarationFile = null) => new(Array.Empty<MacroOverride>(), functionOverrides: new[]
    {
        new FunctionOverride("selected", new("unsigned int", new[] { "const unsigned char *" }),
            new("intrinsic", "load.u32.le"), DeclarationFile: declarationFile, RequireMatch: true)
    });

    private static System.Collections.Generic.IReadOnlyDictionary<string, string> Emit(string path, CPreprocessingOptions options, bool objects, SourceSplit split)
    {
        if (!objects) return Compiler.EmitCSharpFiles(new[] { path }, emit: EmitMode.ManagedLib, split: split, preprocessing: options);
        var obj = Path.ChangeExtension(path, ".o");
        File.WriteAllText(obj, Compiler.EmitObject(path, preprocessing: options));
        return Compiler.LinkObjectFiles(new[] { obj }, emit: EmitMode.ManagedLib, split: split);
    }

    private static string Body(string source, string name)
    {
        var match = Regex.Match(source, @"\b" + Regex.Escape(name) + @"\([^)]*\)\s*\{(?<body>[^}]*)\}");
        match.Success.ShouldBeTrue("Missing generated caller method " + name);
        return match.Groups["body"].Value;
    }

    private static void WithSource(Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-intrinsic-direct-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { test(directory); }
        finally { Directory.Delete(directory, true); }
    }
}

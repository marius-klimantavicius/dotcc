using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class EnumExportTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "dotcc-enums-" + Guid.NewGuid().ToString("N"));

    public EnumExportTests() => Directory.CreateDirectory(directory);
    public void Dispose() => Directory.Delete(directory, true);

    private string[] Sources(params string[] sources) => sources.Select((source, index) =>
    {
        var path = Path.Combine(directory, index + ".c");
        File.WriteAllText(path, source);
        return path;
    }).ToArray();

    private string Emit(bool link, string[] sources, CSharpOutputOptions? options = null,
        CPreprocessingOptions? profile = null)
    {
        var paths = Sources(sources);
        var dialect = new CDialect(2023);
        if (link)
        {
            paths = paths.Select(path =>
            {
                var obj = Path.ChangeExtension(path, ".obj");
                File.WriteAllText(obj, Compiler.EmitObject(path, dialect: dialect, preprocessing: profile));
                return obj;
            }).ToArray();
            return Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib, outputOptions: options);
        }
        return Compiler.EmitCSharp(paths, emit: EmitMode.ManagedLib, dialect: dialect,
            outputOptions: options, preprocessing: profile);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Anonymous_and_named_enumerators_are_opt_in_and_shared_values_coalesce(bool link)
    {
        const string header = "enum { TAG_FIRST = -9, TAG_ALIAS = TAG_FIRST, TAG_NEXT, OTHER = 99 }; enum Kind { KIND = 7 };";
        var sources = new[] { header + "int first(void) { enum { TAG_FIRST = 30, LOCAL = 1 }; return TAG_FIRST; }",
            header + "int second(void) { return TAG_NEXT; }" };
        Emit(link, sources).ShouldNotContain("public const int TAG_FIRST");
        var output = Emit(link, sources, new(ExportEnum: new[] { "TAG_*", "K?ND", "TAG_FIRST" }));
        output.Split("public const int TAG_FIRST").Length.ShouldBe(2);
        output.ShouldContain("public const int TAG_FIRST = unchecked((int)(-9L));");
        output.ShouldContain("public const int TAG_ALIAS = unchecked((int)(-9L));");
        output.ShouldContain("public const int TAG_NEXT = unchecked((int)(-8L));");
        output.ShouldContain("public const int KIND = unchecked((int)(7L));");
        output.ShouldNotContain("public const int OTHER");
        output.ShouldNotContain("public const int LOCAL");
        output.ShouldNotContain("enum-constant:");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Fixed_underlying_types_and_csharp_keywords_are_preserved(bool link)
    {
        var output = Emit(link, new[] { "enum Wide : unsigned int { MAX = 4294967295U }; enum { event = 6 };" },
            new(ExportEnum: new[] { "MAX", "event" }));
        output.ShouldContain("public const uint MAX = unchecked((uint)(4294967295L));");
        output.ShouldContain("public const int @event = unchecked((int)(6L));");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Conflicting_values_are_rejected_only_when_selected(bool link)
    {
        var sources = new[] { "enum { TAG = 6 };", "enum { TAG = 8 };" };
        Emit(link, sources).ShouldNotContain("public const int TAG");
        Should.Throw<CompileException>(() => Emit(link, sources, new(ExportEnum: new[] { "TAG" })))
            .Message.ShouldContain("ambiguous --export-enum 'TAG'");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Conflicting_underlying_types_are_rejected(bool link)
    {
        Should.Throw<CompileException>(() => Emit(link,
            new[] { "enum A : unsigned int { TAG = 6 };", "enum B : int { TAG = 6 };" },
            new(ExportEnum: new[] { "TAG" }))).Message.ShouldContain("ambiguous --export-enum 'TAG'");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Each_selector_must_match_a_file_scope_enumerator(bool link)
    {
        Should.Throw<CompileException>(() => Emit(link,
            new[] { "enum { TAG = 6 }; int f(void) { enum { LOCAL = 1 }; return LOCAL; }" },
            new(ExportEnum: new[] { "TAG", "LOCAL" }))).Message.ShouldContain("matched no file-scope enumerators");
    }

    [Theory]
    [InlineData(false, "int TAG(void) { return 0; }")]
    [InlineData(true, "int TAG(void) { return 0; }")]
    [InlineData(false, "int TAG;")]
    [InlineData(true, "int TAG;")]
    [InlineData(false, "struct TAG { int x; };")]
    [InlineData(true, "struct TAG { int x; };")]
    [InlineData(false, "#define TAG 6\nint f(void) { return TAG; }")]
    [InlineData(true, "#define TAG 6\nint f(void) { return TAG; }")]
    public void Export_collisions_are_diagnosed(bool link, string other)
    {
        Should.Throw<CompileException>(() => Emit(link, new[] { "enum { TAG = 6 };", other },
            new(ExportEnum: new[] { "TAG" }))).Message.ShouldContain("conflicts with a generated type or member");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Profile_selectors_survive_objects_and_combine_with_output_options(bool link)
    {
        var path = Path.Combine(directory, "overrides.json");
        File.WriteAllText(path, """{"version":1,"enumExports":["TAG_*"]}""");
        var profile = CPreprocessingOptions.Load(path);
        profile.EnumExports.ShouldBe(new[] { "TAG_*" });
        profile.WithoutReport().EnumExports.ShouldBe(profile.EnumExports);
        profile.ProfileHash.ShouldNotBe(new CPreprocessingOptions(Array.Empty<MacroOverride>()).ProfileHash);
        var output = Emit(link, new[] { "enum { TAG_A = 6, OTHER = 8 };" },
            new(ExportEnum: new[] { "OTHER" }), profile.WithoutReport());
        output.ShouldContain("public const int TAG_A");
        output.ShouldContain("public const int OTHER");
        output.ShouldNotContain("enum-export:");
    }

    [Fact]
    public void Old_objects_require_regeneration_for_exports_from_cli_or_other_objects()
    {
        var paths = Sources("enum { TAG = 6 };");
        var old = Path.Combine(directory, "old.obj");
        File.WriteAllText(old, Compiler.EmitObject(paths[0]).Replace("//!!dotcc-obj enum-constants:1\n", ""));
        Compiler.LinkObjects(new[] { old }, emit: EmitMode.ManagedLib);
        Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { old }, emit: EmitMode.ManagedLib,
            outputOptions: new(ExportEnum: new[] { "TAG" }))).Message.ShouldContain("regenerate objects");
        var current = Path.Combine(directory, "current.obj");
        File.WriteAllText(current, Compiler.EmitObject(paths[0],
            preprocessing: new(Array.Empty<MacroOverride>(), enumExports: new[] { "TAG" })));
        Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { old, current }, emit: EmitMode.ManagedLib))
            .Message.ShouldContain("regenerate objects");
    }

    [Fact]
    public void Selectors_are_validated_and_output_option_is_link_time_only()
    {
        var paths = Sources("enum { TAG = 6 };");
        Should.Throw<CompileException>(() => Compiler.EmitObject(paths[0], outputOptions: new(ExportEnum: new[] { "TAG" })))
            .Message.ShouldContain("link time");
        Should.Throw<CompileException>(() => new CPreprocessingOptions(Array.Empty<MacroOverride>(), enumExports: new[] { "TAG[" }))
            .Message.ShouldContain("invalid enumExports pattern");
        Should.Throw<CompileException>(() => Emit(false, new[] { "enum { TAG = 6 };" }, new(ExportEnum: new[] { "TAG[" })))
            .Message.ShouldContain("invalid --export-enum pattern");
    }
}

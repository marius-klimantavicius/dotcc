using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class FieldTypeNameTests
{
    private static void WithSource(string source, Action<string, string> action)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-field-names-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var path = Path.Combine(dir, "test.c");
            File.WriteAllText(path, source);
            action(path, Path.Combine(dir, "overrides.json"));
        }
        finally { Directory.Delete(dir, true); }
    }
    private static string Emit(string path, params FieldTypeNameOverride[] rules) => Compiler.EmitCSharp(new[] { path },
        emit: EmitMode.ManagedLib, preprocessing: new CPreprocessingOptions(Array.Empty<MacroOverride>(), fieldTypeNames: rules));

    [Fact]
    public void Json_profile_combines_macros_and_field_names_and_tracks_dependencies_and_hash()
    {
        WithSource("#define SIZE 1\ntypedef struct Tag { union { struct { int items[SIZE]; } DATA; }; } Event;", (path, profile) =>
        {
            File.WriteAllText(profile, """
                {"version":1,"macroOverrides":[{"name":"SIZE","replacement":"3"}],
                 "fieldTypeNames":[{"field":"Event::DATA","name":"EventData","requireMatch":true}]}
                """);
            using var report = new StringWriter();
            var options = CPreprocessingOptions.Load(profile, report: report);
            var output = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: options);
            output.ShouldContain("partial struct EventData");
            output.ShouldContain("fixed int items[3]");
            output.ShouldContain("public ref EventData DATA");
            report.ToString().ShouldContain("field-type-name");
            Compiler.EmitDependencyRule(path, new[] { "test.o" }, includeSystemHeaders: true, preprocessing: options).ShouldContain(profile);
            options.WithoutReport().ProfileHash.ShouldBe(options.ProfileHash);
            new CPreprocessingOptions(new[] { new MacroOverride("SIZE", "3") }).ProfileHash.ShouldNotBe(options.ProfileHash);
            options.FieldTypeNames[0].Field.ShouldBe("Event.DATA");
        });
    }

    [Fact]
    public void Object_links_reject_inconsistent_names_for_shared_types()
    {
        WithSource("", (path, _) =>
        {
            var dir = Path.GetDirectoryName(path)!;
            File.WriteAllText(Path.Combine(dir, "shared.h"), "typedef struct { struct { int x; } DATA; } Event;");
            File.WriteAllText(path, "#include \"shared.h\"\nint first(Event *p) { return p->DATA.x; }");
            var second = Path.Combine(dir, "second.c");
            File.WriteAllText(second, "#include \"shared.h\"\nint second(Event *p) { return p->DATA.x; }");
            var firstObject = Path.Combine(dir, "first.o"); var secondObject = Path.Combine(dir, "second.o");
            CPreprocessingOptions Options(string name) => new(Array.Empty<MacroOverride>(), fieldTypeNames: new[] { new FieldTypeNameOverride("Event.DATA", name) });
            File.WriteAllText(firstObject, Compiler.EmitObject(path, preprocessing: Options("FirstData")));
            File.WriteAllText(secondObject, Compiler.EmitObject(second, preprocessing: Options("SecondData")));
            Should.Throw<CompileException>(() => Compiler.LinkObjectFiles(new[] { firstObject, secondObject }, emit: EmitMode.ManagedLib));
        });
    }

    [Theory]
    [InlineData("Missing.DATA", "Renamed", "requireMatch")]
    [InlineData("Event.MISSING", "Renamed", "path not found")]
    [InlineData("Event.number", "Renamed", "anonymous struct or union")]
    [InlineData("Event.pointer", "Renamed", "anonymous struct or union")]
    [InlineData("Event.array", "Renamed", "anonymous struct or union")]
    [InlineData("Event.named", "Renamed", "anonymous struct or union")]
    [InlineData("Event.DATA", "Tag", "conflicts")]
    [InlineData("Event.DATA", "Event", "conflicts")]
    [InlineData("Event.DATA", "value", "conflicts with a member")]
    [InlineData("Event.DATA", "System", "conflicts")]
    public void Invalid_semantic_selectors_fail(string field, string name, string error)
    {
        WithSource("""
            struct Named { int n; };
            typedef struct Tag {
                union { struct { int value; } DATA; };
                int number; struct { int x; } *pointer;
                struct { int x; } array[2]; struct Named named;
            } Event;
            """, (path, _) => Should.Throw<CompileException>(() => Emit(path, new FieldTypeNameOverride(field, name, true))).Message.ShouldContain(error));
    }

    [Fact]
    public void Aliases_select_one_identity_and_conflicting_names_or_collisions_fail()
    {
        WithSource("typedef struct Tag { union { struct { int value; } DATA; struct { int other; } SECOND; }; } Event; typedef Event Alias;", (path, _) =>
        {
            Emit(path, new("Alias.DATA", "Payload"), new("Tag.DATA", "Payload")).ShouldContain("struct Payload");
            Should.Throw<CompileException>(() => Emit(path, new("Event.DATA", "Payload"), new("Tag.DATA", "Other")))
                .Message.ShouldContain("conflicting names");
            Should.Throw<CompileException>(() => Emit(path, new("Event.DATA", "Payload"), new("Event.SECOND", "Payload")))
                .Message.ShouldContain("collision");
        });
    }

    [Fact]
    public void Nested_paths_and_union_types_can_be_named_together()
    {
        WithSource("typedef struct { struct { union { int a; long b; } value; } DATA; } Event;", (path, _) =>
        {
            var output = Emit(path, new("Event.DATA", "Payload"), new("Event.DATA.value", "ValueUnion"));
            output.ShouldContain("struct Payload"); output.ShouldContain("struct ValueUnion");
            output.ShouldContain("public ValueUnion value;");
        });
    }

    [Theory]
    [InlineData("{\"version\":1,\"fieldTypeNames\":[{\"field\":\"Event.DATA\",\"name\":\"Name\",\"typo\":true}]}")]
    [InlineData("{\"version\":1,\"fieldTypeNames\":[{\"field\":\"Event.DATA\",\"name\":\"Name\",\"name\":\"Other\"}]}")]
    [InlineData("{\"version\":1,\"fieldTypeNames\":[{\"field\":\"Event.DATA\",\"name\":\"Name\"},{\"field\":\"Event::DATA\",\"name\":\"Name\"}]}")]
    [InlineData("{\"version\":1,\"fieldTypeNames\":[{\"field\":\"Event\",\"name\":\"Name\"}]}")]
    [InlineData("{\"version\":1,\"fieldTypeNames\":[{\"field\":\"Event.DATA\",\"name\":\"class\"}]}")]
    [InlineData("{\"version\":1,\"fieldTypeNames\":null}")]
    public void Invalid_profile_is_rejected(string json)
    {
        WithSource("", (_, profile) => { File.WriteAllText(profile, json); Should.Throw<CompileException>(() => CPreprocessingOptions.Load(profile)); });
    }

    [Fact]
    public void Optional_absent_root_is_reported_and_type_only_profile_is_valid()
    {
        WithSource("int f(void) { return 1; }", (path, profile) =>
        {
            File.WriteAllText(profile, """{"version":1,"fieldTypeNames":[{"field":"Missing.DATA","name":"Payload"}]}""");
            using var report = new StringWriter();
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: CPreprocessingOptions.Load(profile, report: report));
            report.ToString().ShouldContain("field-type-name-unmatched");
        });
    }
}

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class SemanticFunctionOverrideTests
{
    private const string Definition = "#include <stdint.h>\nint32_t read_int32(const uint8_t *p) { return 17; }";

    private static FunctionOverride LoadRule(bool required = true) => new("read_int32",
        new FunctionSignature("int32_t", new[] { "const uint8_t *" }),
        new FunctionOverrideTarget("intrinsic", "load.i32.le"), RequireMatch: required);

    private static CPreprocessingOptions Options(params FunctionOverride[] rules) =>
        new(Array.Empty<MacroOverride>(), functionOverrides: rules);

    private static void WithFiles(Action<string> action)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-functions-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { action(directory); }
        finally { Directory.Delete(directory, true); }
    }

    private static string Source(string directory, string contents, string name = "probe.c")
    {
        var path = Path.Combine(directory, name);
        File.WriteAllText(path, contents);
        return path;
    }

    [Fact]
    public void Profile_loads_function_rules_and_preserves_hash_when_report_is_removed()
    {
        WithFiles(directory =>
        {
            var path = Source(directory, Definition);
            var profile = Source(directory, """
                {"version":1,"functionOverrides":[{
                  "name":"read_int32",
                  "signature":{"returnType":"int32_t","parameterTypes":["const uint8_t *"],"variadic":false},
                  "target":{"kind":"intrinsic","name":"load.i32.le"},"requireMatch":true
                }]}
                """, "overrides.json");
            using var report = new StringWriter();
            var options = CPreprocessingOptions.Load(profile, report: report);
            options.HasOverrides.ShouldBeTrue();
            options.WithoutReport().ProfileHash.ShouldBe(options.ProfileHash);
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: options)
                .ShouldContain("BinaryPrimitives.ReadInt32LittleEndian");
            Compiler.EmitDependencyRule(path, new[] { "probe.o" }, false, preprocessing: options.WithoutReport())
                .ShouldContain(profile);
            report.ToString().ShouldContain("read_int32");
            report.ToString().ShouldContain("load.i32.le");
            report.ToString().ShouldContain("probe.c");
        });
    }

    [Fact]
    public void Field_type_rebinding_reports_one_semantic_selection()
    {
        WithFiles(directory =>
        {
            var path = Source(directory, Definition + "\ntypedef struct { struct { int value; } DATA; } Event;");
            using var report = new StringWriter();
            var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), report: report,
                fieldTypeNames: new[] { new FieldTypeNameOverride("Event.DATA", "EventData", RequireMatch: true) },
                functionOverrides: new[] { LoadRule() });
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: options)
                .ShouldContain("partial struct EventData");
            report.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries).Count(line =>
            {
                using var doc = JsonDocument.Parse(line);
                return doc.RootElement.GetProperty("event").GetString() == "function-override";
            }).ShouldBe(1);
        });
    }

    [Theory]
    [InlineData("\"target\":{\"kind\":\"intrinsic\",\"name\":\"unknown\"}")]
    [InlineData("\"target\":{\"kind\":\"managedMethod\",\"method\":\"Host.Read(p); evil()\"}")]
    [InlineData("\"target\":{\"kind\":\"managedMethod\",\"method\":\"Host.Read\",\"extra\":true}")]
    [InlineData("\"target\":{\"kind\":\"intrinsic\",\"name\":\"load.i32.le\"},\"unknown\":true")]
    public void Invalid_profile_targets_and_unknown_fields_are_rejected(string target)
    {
        WithFiles(directory =>
        {
            var profile = Source(directory,
                "{\"version\":1,\"functionOverrides\":[{\"name\":\"read_int32\",\"signature\":{\"returnType\":\"int32_t\",\"parameterTypes\":[\"const uint8_t *\"]}," + target + "}]}",
                "overrides.json");
            Should.Throw<CompileException>(() => CPreprocessingOptions.Load(profile));
        });
    }

    [Fact]
    public void Canonical_typedefs_and_redeclarations_select_one_function()
    {
        WithFiles(directory =>
        {
            var path = Source(directory, """
                #include <stdint.h>
                typedef int32_t Result;
                typedef uint8_t Octet;
                Result read_int32(const Octet *);
                Result read_int32(const Octet *renamed) { return 991; }
                """);
            var output = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: Options(LoadRule()));
            output.ShouldContain("BinaryPrimitives.ReadInt32LittleEndian");
            output.ShouldNotContain("return 991;");
        });
    }

    [Fact]
    public void Header_selectors_resolve_against_profile_directory_and_ignore_line_aliases()
    {
        WithFiles(directory =>
        {
            Source(directory, "#line 71 \"invented-header.h\"\n" + Definition, "original.h");
            var path = Source(directory, "#include \"original.h\"\nint caller(void) { return 0; }");
            var profile = Source(directory, """
                {"version":1,"functionOverrides":[{
                  "name":"read_int32","translationUnit":"probe.c","declarationFile":"original.h",
                  "signature":{"returnType":"int32_t","parameterTypes":["const uint8_t *"]},
                  "target":{"kind":"intrinsic","name":"load.i32.le"},"requireMatch":true
                }]}
                """, "overrides.json");
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: CPreprocessingOptions.Load(profile))
                .ShouldContain("ReadInt32LittleEndian");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib,
                preprocessing: Options(LoadRule() with { DeclarationFile = Path.Combine(directory, "invented-header.h") })));
        });
    }

    [Fact]
    public void Selected_body_is_parsed_but_unsupported_implementation_is_not_lowered()
    {
        WithFiles(directory =>
        {
            var path = Source(directory, """
                #include <stdint.h>
                #include <setjmp.h>
                int32_t read_int32(const uint8_t *p) { jmp_buf env; return 1 + setjmp(env); }
                """);
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib));
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: Options(LoadRule()))
                .ShouldContain("ReadInt32LittleEndian");
            File.WriteAllText(path, Definition + "\nthis is not valid C !");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: Options(LoadRule())));
        });
    }

    [Theory]
    [InlineData("uint32_t read_int32(const uint8_t *p) { return 17; }")]
    [InlineData("int32_t read_int32(const uint8_t *p, int n) { return 17; }")]
    [InlineData("int32_t read_int32(const uint16_t *p) { return 17; }")]
    public void A_selected_name_with_signature_drift_fails(string definition)
    {
        WithFiles(directory =>
        {
            var path = Source(directory, "#include <stdint.h>\n" + definition);
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: Options(LoadRule())))
                .Message.ShouldContain("signature");
        });
    }

    [Theory]
    [InlineData("const volatile uint8_t *")]
    [InlineData("const _Atomic(uint8_t) *")]
    public void The_byte_load_does_not_claim_volatile_or_atomic_access(string parameterType)
    {
        WithFiles(directory =>
        {
            var path = Source(directory, "#include <stdint.h>\nint32_t read_int32(" + parameterType + "p) { return 0; }");
            var rule = LoadRule() with { Signature = new FunctionSignature("int32_t", new[] { parameterType }) };
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: Options(rule)));
        });
    }

    [Fact]
    public void A_noreturn_declaration_cannot_be_replaced_by_a_returning_target()
    {
        WithFiles(directory =>
        {
            var path = Source(directory, Definition.Replace("int32_t read_int32", "_Noreturn int32_t read_int32"));
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib,
                preprocessing: Options(LoadRule()))).Message.ShouldContain("noreturn");
        });
    }

    [Fact]
    public void Required_matches_are_per_invocation_and_optional_absence_is_allowed()
    {
        WithFiles(directory =>
        {
            var present = Source(directory, Definition);
            var absent = Source(directory, "int unrelated(void) { return 42; }", "other.c");
            var options = Options(LoadRule());
            Compiler.EmitCSharp(new[] { present }, emit: EmitMode.ManagedLib, preprocessing: options).ShouldContain("ReadInt32LittleEndian");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { absent }, emit: EmitMode.ManagedLib, preprocessing: options))
                .Message.ShouldContain("requireMatch");
            Compiler.EmitCSharp(new[] { absent }, emit: EmitMode.ManagedLib, preprocessing: Options(LoadRule(false))).ShouldContain("return 42;");
        });
    }

    [Fact]
    public void Duplicate_rules_cannot_replace_the_same_symbol_twice()
    {
        WithFiles(directory =>
        {
            var path = Source(directory, Definition);
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: Options(LoadRule(), LoadRule())));
        });
    }

    [Fact]
    public void Same_named_internal_functions_require_translation_unit_selection()
    {
        WithFiles(directory =>
        {
            var a = Source(directory, Definition.Replace("int32_t read_int32", "static int32_t read_int32") +
                "\nint first(void) { return 1; }", "first.c");
            var b = Source(directory, Definition.Replace("int32_t read_int32", "static int32_t read_int32") +
                "\nint second(void) { return 2; }", "second.c");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { a, b }, emit: EmitMode.ManagedLib, preprocessing: Options(LoadRule())));
            Compiler.EmitCSharp(new[] { a, b }, emit: EmitMode.ManagedLib, preprocessing: Options(LoadRule() with { TranslationUnit = a, Linkage = "internal" }))
                .ShouldContain("ReadInt32LittleEndian");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Static_selection_uses_full_paths_even_when_translation_unit_basenames_match(bool objectLink)
    {
        WithFiles(directory =>
        {
            var firstDirectory = Path.Combine(directory, "first");
            var secondDirectory = Path.Combine(directory, "second");
            Directory.CreateDirectory(firstDirectory);
            Directory.CreateDirectory(secondDirectory);
            var a = Source(firstDirectory, Definition.Replace("int32_t read_int32", "static int32_t read_int32") +
                "\nint first(const uint8_t *p) { return read_int32(p); }");
            var b = Source(secondDirectory, Definition.Replace("int32_t read_int32", "static int32_t read_int32").Replace("return 17", "return 99") +
                "\nint second(const uint8_t *p) { return read_int32(p); }");
            var options = Options(LoadRule(false) with { TranslationUnit = a, Linkage = "internal" });
            var paths = new[] { a, b };
            if (objectLink)
                paths = paths.Select(path => Source(Path.GetDirectoryName(path)!, Compiler.EmitObject(path, preprocessing: options), "probe.o")).ToArray();
            var output = objectLink
                ? Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib)
                : Compiler.EmitCSharp(paths, emit: EmitMode.ManagedLib, preprocessing: options);
            output.ShouldContain("ReadInt32LittleEndian");
            output.ShouldContain("return 99;");
            output.ShouldNotContain("return 17;");
        });
    }

    [Theory]
    [InlineData("intrinsic", "load.i32.le")]
    [InlineData("managedMethod", "global::Host.Read")]
    public void Wat_rejects_CSharp_only_replacements(string kind, string value)
    {
        WithFiles(directory =>
        {
            var path = Source(directory, Definition);
            var rule = LoadRule() with { Target = new FunctionOverrideTarget(kind, value) };
            Should.Throw<CompileException>(() => Compiler.EmitWat(new[] { path }, preprocessing: Options(rule)))
                .Message.ShouldContain("semantic function override");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Object_link_rejects_replacement_conflicts(bool otherReplacement)
    {
        WithFiles(directory =>
        {
            var definition = Source(directory, Definition);
            var declaration = Source(directory, "#include <stdint.h>\nint32_t read_int32(const uint8_t *);\nint user(const uint8_t *p) { return read_int32(p); }", "user.c");
            var a = Source(directory, Compiler.EmitObject(definition, preprocessing: otherReplacement ? Options(LoadRule() with
                { Target = new FunctionOverrideTarget("managedMethod", "global::Host.Read") }) : null), "definition.o");
            var b = Source(directory, Compiler.EmitObject(declaration, preprocessing: Options(LoadRule())), "user.o");
            Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { a, b }, emit: EmitMode.ManagedLib));
        });
    }

    [Fact]
    public void Legacy_object_without_semantic_contract_cannot_replace_an_overridden_definition()
    {
        WithFiles(directory =>
        {
            var path = Source(directory, Definition);
            var oldObject = Compiler.EmitObject(path);
            oldObject = string.Join("\n", oldObject.Split('\n').Where(line => !line.StartsWith("//!!dotcc-obj function-contract:", StringComparison.Ordinal)));
            var original = Source(directory, oldObject, "original.o");
            var replaced = Source(directory, Compiler.EmitObject(path, preprocessing: Options(LoadRule())), "replaced.o");
            Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { original, replaced }, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("conflicting semantic function override");
        });
    }
}

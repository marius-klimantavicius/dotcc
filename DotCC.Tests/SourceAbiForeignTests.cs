using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace DotCC.Tests;

[Collection("SourceAbiForeign")]
public sealed class SourceAbiForeignTests : IDisposable
{
    private const string ModelPrefix = "//!!dotcc-obj source-abi:";
    private const string ProofPrefix = "//!!dotcc-obj foreign-abi:";
    private const string RestrictionPrefix = "//!!dotcc-obj foreign-abi-restriction:";
    private const string OrdinaryImport = "int foreign_count(int n); int main(void) { return foreign_count(7); }";
    private readonly string directory = Path.Combine(Path.GetTempPath(), "dotcc-source-abi-foreign-" + Guid.NewGuid().ToString("N"));
    private int nextFile;

    public SourceAbiForeignTests() => Directory.CreateDirectory(directory);
    public void Dispose() => Directory.Delete(directory, recursive: true);

    private static CPreprocessingOptions Profile() => new(Array.Empty<MacroOverride>(),
        sourceAbi: new(CLongDoubleRepresentation.X87Storage16));

    // These tests emit source only. As in ImportModeTests, archive paths need not
    // exist: the archive is opened by the later NativeAOT link, not by emission.
    private ImportOptions Native(bool archive) => archive
        ? new(Array.Empty<string>(), Array.Empty<string>(), new[] { Path.Combine(directory, "libsourceabi.a") })
        : new(new[] { "sourceabi_foreign" }, Array.Empty<string>(), Array.Empty<string>());

    private string Write(string extension, string text)
    {
        var path = Path.Combine(directory, "unit" + nextFile++ + extension);
        File.WriteAllText(path, text);
        return path;
    }

    private string Object(string source, bool storage = true)
        => Compiler.EmitObject(Write(".c", source), preprocessing: storage ? Profile() : null);

    private string Link(ImportOptions? imports, EmitMode mode, params string[] fragments)
        => Compiler.LinkObjects(fragments.Select(fragment => Write(".cs", fragment)).ToArray(),
            imports: imports, emit: mode);

    private string Emit(string source, bool objects, ImportOptions? imports = null,
        EmitMode mode = EmitMode.File)
        => objects ? Link(imports, mode, Object(source))
            : Compiler.EmitCSharp(new[] { Write(".c", source) }, preprocessing: Profile(), imports: imports, emit: mode);

    private static void AssertForeignAbiRejected(Action action, string symbol)
    {
        var error = Assert.ThrowsAny<CompileException>(action);
        Assert.Contains("storage/layout only", error.Message);
        Assert.Contains("native x87", error.Message);
        Assert.Contains(symbol, error.Message);
    }

    private static void AssertBinding(string output, string symbol, bool archive)
    {
        if (archive)
        {
            Assert.Contains("static unsafe class DotCcStaticImports", output);
            Assert.Contains("EntryPoint = \"" + symbol + "\"", output);
        }
        else
        {
            Assert.Contains("static unsafe class DotCcImports", output);
            Assert.Contains("TryResolveExport(__libs, \"" + symbol + "\", out __p)", output);
        }
    }

    public static IEnumerable<object[]> RestrictedImports()
    {
        var sources = new[]
        {
            "void foreign_value(long double value); void exercise(long double value) { foreign_value(value); }",
            "long double foreign_value(void); long double exercise(void) { return foreign_value(); }",
            "struct Payload { long double value; }; void foreign_value(struct Payload value); " +
                "void exercise(struct Payload value) { foreign_value(value); }",
            "typedef const long double Sample; struct Leaf { Sample values[2]; }; " +
                "struct Payload { struct Leaf inner; int tag; }; void foreign_value(struct Payload value); " +
                "void exercise(struct Payload value) { foreign_value(value); }",
            "union Payload { long double value; unsigned char bytes[16]; }; " +
                "void foreign_value(union Payload value); void exercise(union Payload value) { foreign_value(value); }",
            "struct Payload { long double values[2]; }; struct Payload foreign_value(void); " +
                "struct Payload exercise(void) { return foreign_value(); }",
        };
        foreach (bool objects in new[] { false, true })
        foreach (bool archive in new[] { false, true })
        foreach (var source in sources)
            yield return new object[] { objects, archive, source + " int main(void) { return 0; }" };
    }

    [Theory]
    [MemberData(nameof(RestrictedImports))]
    public void Selected_native_imports_reject_scalar_and_aggregate_storage_values(bool objects, bool archive, string source)
        => AssertForeignAbiRejected(() => Emit(source, objects, Native(archive)), "foreign_value");

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Unused_restricted_declarations_do_not_poison_an_ordinary_native_import(bool objects, bool archive)
    {
        var output = Emit("""
            struct Payload { long double values[2]; };
            long double unused_scalar(long double value);
            struct Payload unused_aggregate(struct Payload value);
            int foreign_count(int n);
            int main(void) { return foreign_count(7); }
            """, objects, Native(archive));
        AssertBinding(output, "foreign_count", archive);
        Assert.DoesNotContain("EntryPoint = \"unused_", output);
        Assert.DoesNotContain("TryResolveExport(__libs, \"unused_", output);
    }

    public static IEnumerable<object[]> PointerImports()
    {
        var sources = new[]
        {
            "long double *foreign_value(long double *value); " +
                "long double *exercise(long double *value) { return foreign_value(value); }",
            "struct Payload { long double value; }; struct Payload *foreign_value(struct Payload *value); " +
                "struct Payload *exercise(struct Payload *value) { return foreign_value(value); }",
            // By-value records containing only addresses do not carry x87 values.
            "struct Payload { const long double *value; struct Payload *next; }; " +
                "struct Payload foreign_value(struct Payload value); " +
                "struct Payload exercise(struct Payload value) { return foreign_value(value); }",
            "void foreign_value(long double values[2]); " +
                "void exercise(long double *values) { foreign_value(values); }",
        };
        foreach (var route in new[] { (Objects: false, Archive: false), (Objects: false, Archive: true), (Objects: true, Archive: false) })
        foreach (var source in sources)
            yield return new object[] { route.Objects, route.Archive, source + " int main(void) { return 0; }" };
    }

    [Theory]
    [MemberData(nameof(PointerImports))]
    public void Raw_pointers_array_parameters_and_records_of_pointers_can_be_imported(bool objects, bool archive, string source)
        => AssertBinding(Emit(source, objects, Native(archive)), "foreign_value", archive);

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Managed_cross_translation_unit_definitions_win_before_native_import_selection(bool objects, bool archive)
    {
        const string header = "struct Payload { long double values[2]; }; ";
        const string consumer = """
            long double identity(long double value);
            struct Payload copy_payload(struct Payload value);
            void exercise(long double value, struct Payload payload) {
                long double copied = identity(value);
                struct Payload other = copy_payload(payload);
            }
            int main(void) { return 0; }
            """;
        const string provider = """
            long double identity(long double value) { return value; }
            struct Payload copy_payload(struct Payload value) { return value; }
            """;
        var output = objects
            ? Link(Native(archive), EmitMode.File, Object(header + consumer), Object(header + provider))
            : Compiler.EmitCSharp(new[] { Write(".c", header + consumer), Write(".c", header + provider) },
                preprocessing: Profile(), imports: Native(archive));
        Assert.Contains(" identity(", output);
        Assert.Contains(" copy_payload(", output);
        Assert.DoesNotContain("static unsafe class DotCcImports", output);
        Assert.DoesNotContain("static unsafe class DotCcStaticImports", output);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Restricted_unresolved_calls_can_be_emitted_without_selecting_a_native_binding(bool objects)
    {
        var output = Emit("""
            struct Payload { long double value; };
            struct Payload foreign_value(struct Payload value);
            struct Payload exercise(struct Payload value) { return foreign_value(value); }
            int main(void) { return 0; }
            """, objects);
        Assert.Contains("foreign_value(", output);
        Assert.DoesNotContain("static unsafe class DotCcImports", output);
    }

    [Theory]
    [InlineData("void exercise(void *h, struct Payload value) { void (*fn)(struct Payload) = (void (*)(struct Payload))dlsym(h, \"foreign_value\"); fn(value); }")]
    [InlineData("struct Payload exercise(void *h) { return ((struct Payload (*)(void))dlsym(h, \"foreign_value\"))(); }")]
    public void Native_function_pointer_calls_also_reject_aggregate_storage_values(string source)
        => AssertForeignAbiRejected(() => Object("#include <dlfcn.h>\nstruct Payload { long double values[2]; }; " + source),
            "native function pointer");

    [Fact]
    public void An_unused_native_function_pointer_with_a_restricted_signature_is_allowed()
        => Assert.NotEmpty(Object("""
            #include <dlfcn.h>
            struct Payload { long double value; };
            void exercise(void *h) {
                struct Payload (*fn)(void) = (struct Payload (*)(void))dlsym(h, "foreign_value");
                (void)fn;
            }
            """));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Missing_storage_proof_is_rejected_when_an_unresolved_import_acquires_a_native_binding(bool archive)
    {
        var fragment = WithoutLines(Object(OrdinaryImport), ProofPrefix);
        Assert.Contains(ModelPrefix + "x87-storage16", fragment);
        Assert.NotEmpty(Link(null, EmitMode.File, fragment));

        // The very same old storage object remains usable if a managed object
        // resolves its import. Its model marker alone must not ban linking.
        var managed = Link(Native(archive), EmitMode.File, fragment, Object("int foreign_count(int n) { return n; }"));
        Assert.DoesNotContain("static unsafe class DotCcImports", managed);
        Assert.DoesNotContain("static unsafe class DotCcStaticImports", managed);

        var error = Assert.ThrowsAny<CompileException>(() => Link(Native(archive), EmitMode.File, fragment));
        Assert.Contains("lacks foreign ABI proof", error.Message);
        Assert.Contains("regenerate objects", error.Message);
    }

    [Theory]
    [InlineData("missing-model", "source ABI model")]
    [InlineData("invalid-model", "unsupported source ABI model")]
    [InlineData("invalid-version", "invalid foreign ABI proof version")]
    [InlineData("duplicate-version", "invalid foreign ABI proof version")]
    [InlineData("invalid-symbol", "invalid foreign ABI symbol")]
    public void Invalid_object_proof_cannot_enable_an_active_native_import(string fault, string diagnostic)
    {
        var fragment = Object(OrdinaryImport);
        fragment = fault switch
        {
            "missing-model" => WithoutLines(fragment, ModelPrefix),
            "invalid-model" => WithoutLines(fragment, ModelPrefix) + ModelPrefix + "unknown\n",
            "invalid-version" => WithoutLines(fragment, ProofPrefix) + ProofPrefix + "999\n",
            "duplicate-version" => fragment + ProofPrefix + "1\n",
            "invalid-symbol" => fragment + RestrictionPrefix + "import ! ! x87-storage16\n",
            _ => throw new ArgumentOutOfRangeException(nameof(fault)),
        };
        var error = Assert.ThrowsAny<CompileException>(() => Link(Native(false), EmitMode.File, fragment));
        Assert.Contains(diagnostic, error.Message);
        Assert.Contains("regenerate objects", error.Message);
    }

    [Theory]
    [InlineData(OrdinaryImport, "foreign_count")]
    [InlineData("long double foreign_value(long double value); long double exercise(long double value) { return foreign_value(value); } int main(void) { return 0; }", "foreign_value")]
    public void Ordinary_legacy_objects_keep_their_native_import_behavior(string source, string symbol)
    {
        var fragment = Object(source, storage: false);
        Assert.Contains(ModelPrefix + "binary64", fragment);
        fragment = WithoutLines(WithoutLines(fragment, ModelPrefix), ProofPrefix);
        Assert.DoesNotContain(RestrictionPrefix, fragment);
        AssertBinding(Link(Native(false), EmitMode.File, fragment), symbol, archive: false);
    }

    [Theory]
    [InlineData("long double foreign_value(long double value) { return value; }")]
    [InlineData("long double stored; long double foreign_value(void) { return stored; }")]
    [InlineData("struct Payload { long double value; }; void foreign_value(struct Payload value) { }")]
    [InlineData("struct Payload { long double values[2]; }; struct Payload foreign_value(struct Payload value) { return value; }")]
    public void Direct_shared_library_exports_guard_the_actual_native_boundary(string source)
    {
        var managed = Emit(source, objects: false, mode: EmitMode.ManagedLib);
        Assert.Contains(" foreign_value(", managed);
        Assert.DoesNotContain("[UnmanagedCallersOnly(EntryPoint = \"foreign_value\"", managed);
        AssertForeignAbiRejected(() => Emit(source, objects: false, mode: EmitMode.SharedLib), "foreign_value");
    }

    [Theory]
    [InlineData("long double *foreign_value(long double *value) { return value; }")]
    [InlineData("struct Payload { long double value; }; struct Payload *foreign_value(struct Payload *value) { return value; }")]
    [InlineData("static long double identity(long double value) { return value; } int foreign_value(void) { return 7; }")]
    public void Direct_shared_libraries_still_export_pointer_and_ordinary_signatures(string source)
    {
        var output = Emit(source, objects: false, mode: EmitMode.SharedLib);
        Assert.Contains("[UnmanagedCallersOnly(EntryPoint = \"foreign_value\"", output);
        Assert.DoesNotContain("[UnmanagedCallersOnly(EntryPoint = \"identity\"", output);
    }

    [Fact]
    public void A_variadic_definition_that_will_not_be_exported_does_not_trigger_the_export_guard()
    {
        var output = Emit("void foreign_value(long double value, ...) { }", objects: false, mode: EmitMode.SharedLib);
        Assert.Contains(" foreign_value(", output);
        Assert.DoesNotContain("[UnmanagedCallersOnly(EntryPoint = \"foreign_value\"", output);
    }

    [Theory]
    [InlineData("long double foreign_value(long double value) { return value; }")]
    [InlineData("struct Payload { long double values[2]; }; struct Payload foreign_value(struct Payload value) { return value; }")]
    public void Object_shared_library_linking_does_not_invent_native_exports_for_managed_definitions(string source)
    {
        // Object linking currently supplies no export wrappers to the shared
        // shell. A definition restriction records its type, not export intent.
        var output = Emit(source, objects: true, mode: EmitMode.SharedLib);
        Assert.Contains(" foreign_value(", output);
        Assert.DoesNotContain("[UnmanagedCallersOnly(EntryPoint = \"foreign_value\"", output);
    }

    private static string WithoutLines(string fragment, string prefix)
    {
        var lines = fragment.ReplaceLineEndings("\n").Split('\n');
        Assert.Contains(lines, line => line.StartsWith(prefix, StringComparison.Ordinal));
        return string.Join("\n", lines.Where(line => !line.StartsWith(prefix, StringComparison.Ordinal)));
    }
}

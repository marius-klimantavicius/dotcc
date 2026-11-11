using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class SourceIdentityTests
{
    private static readonly CSharpOutputOptions COnly = new(Runtime: RuntimeProfile.C);
    private static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    private static string Write(string root, string relative, string source)
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, source);
        return path;
    }
    private static void WithDirectory(Action<string> action)
    {
        var root = Path.Combine(Path.GetTempPath(), "dotcc-source-identity-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try { action(root); }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Relocation_and_repeat_emission_preserve_objects_and_linked_sources(bool objects)
    {
        WithDirectory(directory =>
        {
            string Emit(string checkout)
            {
                var root = Path.Combine(directory, checkout);
                Write(root, "include/shared.h", "typedef struct { int value; struct { int nested; } part; } Shared;\n");
                var a = Write(root, "left/same.c", "#include <shared.h>\nstatic int data; static int f(void) { static int local; return ++data + ++local; }\nShared shared; int a(void) { return f(); }");
                var b = Write(root, "right/same.c", "#include <shared.h>\nstatic int data; static int f(void) { static int local; return ++data + ++local; }\nextern Shared shared; int b(void) { return f() + shared.part.nested; }");
                var inputs = new[] { a, b };
                string Verify(string emitted)
                {
                    Regex.Matches(emitted, @"struct __Anon[A-F0-9]{64}\b").Count.ShouldBe(1);
                    Regex.Matches(emitted, @"struct Shared\b").Count.ShouldBe(1);
                    WithAssembly(emitted, _ => { });
                    return emitted;
                }
                if (!objects) return Verify(Compiler.EmitCSharp(inputs, includeDirs: new[] { Path.Combine(root, "include") },
                    emit: EmitMode.ManagedLib, sourceRoot: root, outputOptions: COnly));
                var fragments = inputs.Select(path => Compiler.EmitObject(path, includeDirs: new[] { Path.Combine(root, "include") }, sourceRoot: root)).ToArray();
                fragments[0].ShouldContain("f__unit_" + Hash("left/same.c"));
                fragments[1].ShouldContain("f__unit_" + Hash("right/same.c"));
                var paths = fragments.Select((text, index) => Write(root, "obj/" + index + ".o", text)).ToArray();
                var result = Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib, outputOptions: COnly);
                // Compare both object contents and final output, even with different object locations.
                return string.Join('\n', fragments) + Verify(result);
            }
            var first = Emit("original");
            first.ShouldBe(Emit("original"));
            first.ShouldBe(Emit("relocated"));
            first.ShouldNotContain(directory);
        });
    }

    [Fact]
    public void Header_origin_is_canonical_through_different_include_spellings_and_single_or_multiple_units()
    {
        WithDirectory(root =>
        {
            Write(root, "include/types.h", "typedef struct { struct { int x; } nested; } Shared;\n");
            var a = Write(root, "a/unit.c", "#include \"../include/types.h\"\nShared a;\n");
            var b = Write(root, "b/unit.c", "#include <types.h>\nextern Shared a; int b(void) { return a.nested.x; }\n");
            var dirs = new[] { Path.Combine(root, "include") };
            var single = Compiler.EmitCSharp(new[] { a }, emit: EmitMode.ManagedLib, includeDirs: dirs, sourceRoot: root, outputOptions: COnly);
            var multiple = Compiler.EmitCSharp(new[] { a, b }, emit: EmitMode.ManagedLib, includeDirs: dirs, sourceRoot: root, outputOptions: COnly);
            string[] Names(string text) => Regex.Matches(text, @"struct (__Anon[A-F0-9]{64})\b").Select(match => match.Groups[1].Value).ToArray();
            Names(single).Length.ShouldBe(1);
            Names(single).ShouldBe(Names(multiple));
            var objectA = Compiler.EmitObject(a, includeDirs: dirs, sourceRoot: root);
            var objectB = Compiler.EmitObject(b, includeDirs: dirs, sourceRoot: root);
            Names(objectA).ShouldBe(Names(objectB));
            string[] Typedefs(string text) => text.Split('\n').Where(line => line.StartsWith(AnonymousTypedefNames.MetadataPrefix, StringComparison.Ordinal)).ToArray();
            Typedefs(objectA).ShouldBe(Typedefs(objectB));
        });
    }

    [Fact]
    public void Legacy_default_suffix_is_the_full_absolute_path_hash_and_is_still_linkable()
    {
        WithDirectory(root =>
        {
            var a = Write(root, "a/unit.c", "static int helper(void) { return 7; } int a(void) { return helper(); }");
            var b = Write(root, "b/unit.c", "static int helper(void) { return 9; } int b(void) { return helper(); }");
            var legacy = Compiler.EmitObject(a);
            legacy.ShouldBe(Compiler.EmitObject(a, sourceRoot: null));
            legacy.ShouldContain("helper__unit_" + Hash(Path.GetFullPath(a)));
            legacy.ShouldNotContain("source-identity:");
            var stable = Compiler.EmitObject(b, sourceRoot: root);
            var linked = Compiler.LinkObjects(new[] { Write(root, "a.o", legacy), Write(root, "b.o", stable) },
                emit: EmitMode.ManagedLib, outputOptions: COnly);
            linked.ShouldContain("helper__unit_" + Hash("b/unit.c"));
            Invoke(linked, "a").ShouldBe(7);
            Invoke(linked, "b").ShouldBe(9);
        });
    }

    [Fact]
    public void Object_group_hash_retains_input_order_NUL_framing_case_and_full_length()
    {
        WithDirectory(root =>
        {
            var a = Write(root, "Left/A.c", "static int helper(void) { return 1; } int a(void) { return helper(); }");
            var b = Write(root, "Right/B.c", "int b(void) { return 2; }");
            var forward = Compiler.EmitCSharp(new[] { a, b }, emit: EmitMode.Object, sourceRoot: root);
            var reverse = Compiler.EmitCSharp(new[] { b, a }, emit: EmitMode.Object, sourceRoot: root);
            forward.ShouldContain("__unit_" + Hash("Left/A.c\0Right/B.c"));
            reverse.ShouldContain("__unit_" + Hash("Right/B.c\0Left/A.c"));
            forward.ShouldNotContain("__unit_" + Hash("Left/A.cRight/B.c"));
            var dotPath = Path.Combine(root, "Left", "..", "Left", ".", "A.c");
            Compiler.EmitObject(dotPath, sourceRoot: Path.Combine(root, ".")).ShouldBe(Compiler.EmitObject(a, sourceRoot: root));
        });
    }

    [Theory]
    [InlineData("/src/project", "/src/project/a/../Sub/Unit.c", false, "Sub/Unit.c")]
    [InlineData("/src/project", "/src/project/a\\b.c", false, "a\\b.c")]
    [InlineData("/", "/Case/Unit.c", false, "Case/Unit.c")]
    [InlineData("C:\\src\\project", "c:/src/project/a/../Sub/Unit.c", true, "Sub/Unit.c")]
    [InlineData("C:\\src\\project\\", "C:\\src\\project\\Sub\\.\\Unit.c", true, "Sub/Unit.c")]
    [InlineData("\\\\host\\share\\project", "\\\\HOST\\SHARE\\project\\sub\\Unit.c", true, "sub/Unit.c")]
    public void Lexical_platform_contract(string root, string path, bool windows, string expected)
        => SourceIdentityPaths.Relative(root, path, windows).ShouldBe(expected);

    [Theory]
    [InlineData("/src/project", "/src/project-other/unit.c", false)]
    [InlineData("/src/project", "/src/project/../unit.c", false)]
    [InlineData("/src/project", "/SRC/project/unit.c", false)]
    [InlineData("C:\\src", "D:\\src\\unit.c", true)]
    [InlineData("C:\\src", "C:\\src-other\\unit.c", true)]
    [InlineData("\\\\host\\share\\src", "\\\\host\\other\\src\\unit.c", true)]
    public void Lexical_outside_root_and_different_volumes_are_rejected(string root, string path, bool windows)
        => Should.Throw<CompileException>(() => SourceIdentityPaths.Relative(root, path, windows)).Message.ShouldContain("outside --source-root");

    [Fact]
    public void Unix_backslash_filename_does_not_alias_a_subdirectory()
    {
        if (OperatingSystem.IsWindows()) return;
        WithDirectory(root =>
        {
            var a = Write(root, "a\\unit.c", "static int f(void) { return 1; }");
            var b = Write(root, "a/unit.c", "static int f(void) { return 1; }");
            Compiler.EmitObject(a, sourceRoot: root).ShouldContain("__unit_" + Hash("a\\unit.c"));
            Compiler.EmitObject(b, sourceRoot: root).ShouldContain("__unit_" + Hash("a/unit.c"));
        });
    }

    [Fact]
    public void Outside_sources_and_actually_included_disk_headers_fail_with_common_root_guidance()
    {
        WithDirectory(directory =>
        {
            var source = Write(directory, "source/unit.c", "#include <outside.h>\nint a(void) { return 1; }");
            Write(directory, "headers/outside.h", "typedef struct { int x; } Outside;");
            var root = Path.GetDirectoryName(source)!;
            Should.Throw<CompileException>(() => Compiler.EmitObject(source, includeDirs: new[] { Path.Combine(directory, "headers") }, sourceRoot: root))
                .Message.ShouldContain("stage sources and included disk headers");
            Should.Throw<CompileException>(() => Compiler.EmitObject(source, sourceRoot: Path.Combine(directory, "headers")))
                .Message.ShouldContain("outside --source-root");
            // Unused -I entries do not become identity inputs. Embedded headers need no staging.
            File.WriteAllText(source, "#include <stdlib.h>\nint a(void) { return abs(-1); }");
            Compiler.EmitObject(source, includeDirs: new[] { Path.Combine(directory, "headers") }, sourceRoot: root).ShouldContain("int a(");
            Should.Throw<CompileException>(() => Compiler.EmitObject(source, sourceRoot: "")).Message.ShouldContain("nonempty");
            Should.Throw<CompileException>(() => Compiler.EmitObject(source, sourceRoot: Path.Combine(directory, "missing"))).Message.ShouldContain("does not exist");
        });
    }

    [Fact]
    public void Duplicate_logical_units_from_distinct_per_unit_roots_are_errors_even_for_identical_static_only_objects()
    {
        WithDirectory(directory =>
        {
            var a = Write(directory, "first/same.c", "static int helper(void) { return 1; }");
            var b = Write(directory, "second/same.c", "static int helper(void) { return 1; }");
            var objects = new[] { a, b }.Select((path, index) => Write(directory, index + ".o",
                Compiler.EmitObject(path, sourceRoot: Path.GetDirectoryName(path)))).ToArray();
            var error = Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib));
            error.Message.ShouldContain("duplicate logical source unit 'same.c'");
            foreach (var path in objects) error.Message.ShouldContain(path);
            error.Message.ShouldContain("one common --source-root");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { a, a }, emit: EmitMode.Object, sourceRoot: directory))
                .Message.ShouldContain("duplicate logical source unit");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Same_basename_units_keep_callback_binding_and_independent_static_global_and_local_storage(bool objects)
    {
        WithDirectory(root =>
        {
            var inputs = new[] { "first", "second" }.Select((folder, index) => Write(root, folder + "/unit.c", $$"""
                static int data;
                static int helper(void) { static int local; return ++data * 10 + ++local + {{index * 100}}; }
                static int (*callback)(void) = helper;
                int {{folder}}(void) { return callback(); }
                """)).ToArray();
            var emitted = objects ? Compiler.LinkObjects(inputs.Select((path, index) => Write(root, index + ".o",
                Compiler.EmitObject(path, sourceRoot: root))).ToArray(), emit: EmitMode.ManagedLib, outputOptions: COnly)
                : Compiler.EmitCSharp(inputs, emit: EmitMode.ManagedLib, sourceRoot: root, outputOptions: COnly);
            WithAssembly(emitted, assembly =>
            {
                int Call(string name) => (int)assembly.GetType("DotCcLib", true)!.GetMethod(name)!.Invoke(null, null)!;
                Call("first").ShouldBe(11);
                Call("second").ShouldBe(111);
                Call("first").ShouldBe(22);
                Call("second").ShouldBe(122);
            });
        });
    }

    [Fact]
    public void Shared_header_anonymous_types_and_managed_imports_remain_canonical_across_relocation()
    {
        WithDirectory(directory =>
        {
            string Library(string checkout, out string consumerObject)
            {
                var root = Path.Combine(directory, checkout);
                Write(root, "include/api.h", "typedef struct { int value; } Shared;\nShared get(void);\n");
                var provider = Write(root, "src/provider.c", "#include <api.h>\nShared get(void) { Shared result = { 42 }; return result; }");
                var consumer = Write(root, "src/consumer.c", "#include <api.h>\nint probe(void) { return get().value; }");
                var dirs = new[] { Path.Combine(root, "include") };
                var obj = Write(root, "provider.o", Compiler.EmitObject(provider, includeDirs: dirs, sourceRoot: root));
                consumerObject = Write(root, "consumer.o", Compiler.EmitObject(consumer, includeDirs: dirs, sourceRoot: root));
                var files = Compiler.LinkObjectFiles(new[] { obj }, emit: EmitMode.ManagedLib, className: "Provider",
                    outputOptions: new(Runtime: RuntimeProfile.C, NestTypes: true, ExportManagedSymbols: true));
                Write(root, "Dotcc.ManagedSymbols.json", files["Dotcc.ManagedSymbols.json"]);
                return files.Single(pair => pair.Key.EndsWith(".cs", StringComparison.Ordinal)).Value;
            }
            var provider = Library("original", out var originalConsumer);
            var relocatedProvider = Library("relocated", out var relocatedConsumer);
            provider.ShouldBe(relocatedProvider);
            var originalRoot = Path.GetDirectoryName(originalConsumer)!;
            var manifest = Path.Combine(originalRoot, "Dotcc.ManagedSymbols.json");
            var consumer = Compiler.LinkObjects(new[] { relocatedConsumer }, emit: EmitMode.ManagedLib, className: "Consumer",
                outputOptions: new(Runtime: RuntimeProfile.C, NestTypes: true, ManagedReferences: new[] { manifest }));
            consumer.ShouldContain("Provider.Shared");
            consumer.ShouldNotContain("struct Shared");
            var providerImage = Compile(provider);
            var consumerImage = Compile(consumer, MetadataReference.CreateFromImage(providerImage));
            var context = new AssemblyLoadContext("source-identity-import-" + Guid.NewGuid(), isCollectible: true);
            try
            {
                using var providerStream = new MemoryStream(providerImage);
                context.LoadFromStream(providerStream);
                using var consumerStream = new MemoryStream(consumerImage);
                var assembly = context.LoadFromStream(consumerStream);
                ((int)assembly.GetType("Consumer", true)!.GetMethod("probe")!.Invoke(null, null)!).ShouldBe(42);
            }
            finally { context.Unload(); }
        });
    }

    [Fact]
    public void Physical_override_selectors_and_line_aliases_keep_their_meaning()
    {
        WithDirectory(root =>
        {
            var header = Write(root, "include/original.h", "#line 71 \"invented.h\"\nunsigned int selected(const unsigned char *p) { return 123; }");
            var source = Write(root, "src/probe.c", "#include <original.h>\nunsigned int caller(const unsigned char *p) { return selected(p); }");
            var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[]
            {
                new FunctionOverride("selected", new("unsigned int", new[] { "const unsigned char *" }),
                    new("intrinsic", "load.u32.le"), TranslationUnit: source, DeclarationFile: header, RequireMatch: true)
            });
            Compiler.EmitObject(source, includeDirs: new[] { Path.GetDirectoryName(header)! }, sourceRoot: root, preprocessing: options)
                .ShouldContain("BinaryPrimitives.ReadUInt32LittleEndian");
        });
    }

    [Fact]
    public void Native_import_candidates_and_old_objects_keep_the_same_link_behavior()
    {
        WithDirectory(root =>
        {
            var source = Write(root, "src/unit.c", "int native_function(int); static int f(int value) { return native_function(value); } int main(void) { return f(7); }");
            var legacy = Write(root, "legacy.o", Compiler.EmitObject(source));
            var stable = Write(root, "stable.o", Compiler.EmitObject(source, sourceRoot: root));
            var imports = new ImportOptions(new[] { "test-library" }, Array.Empty<string>(), Array.Empty<string>());
            var oldLinked = Compiler.LinkObjects(new[] { legacy }, imports: imports, outputOptions: COnly);
            var newLinked = Compiler.LinkObjects(new[] { stable }, imports: imports, outputOptions: COnly);
            newLinked.ShouldContain("native_function");
            newLinked.ShouldContain("test-library");
            oldLinked.Replace(Hash(Path.GetFullPath(source)), Hash("src/unit.c")).ShouldBe(newLinked);
        });
    }

    [Fact]
    public void Unsupported_Zig_and_corrupt_stable_object_metadata_fail_explicitly()
    {
        WithDirectory(root =>
        {
            var zig = Write(root, "unit.zig", "pub fn probe() i32 { return 1; }");
            Should.Throw<CompileException>(() => Compiler.EmitObject(zig, sourceRoot: root)).Message.ShouldContain("currently supports C");
            var source = Write(root, "unit.c", "int a(void) { return 1; }");
            var text = Compiler.EmitObject(source, sourceRoot: root);
            foreach (var corrupted in new[] { text.Replace("root-relative-v1", "root-relative-v999"),
                Regex.Replace(text, @"//!!dotcc-obj logical-unit:[^\n]+\n", ""),
                Regex.Replace(text, @"//!!dotcc-obj logical-unit:[^\n]+", "//!!dotcc-obj logical-unit:***") })
            {
                var obj = Write(root, "corrupt.o", corrupted);
                Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib)).Message.ShouldContain("invalid");
            }
        });
    }

    private static byte[] Compile(string source, params MetadataReference[] additionalReferences)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path)).Cast<MetadataReference>().Concat(additionalReferences);
        var compilation = CSharpCompilation.Create("SourceIdentity_" + Guid.NewGuid().ToString("N"),
            new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        using var image = new MemoryStream();
        var result = compilation.Emit(image);
        result.Success.ShouldBeTrue(string.Join('\n', result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        return image.ToArray();
    }
    private static void WithAssembly(string source, Action<System.Reflection.Assembly> action)
    {
        var context = new AssemblyLoadContext("source-identity-" + Guid.NewGuid(), isCollectible: true);
        try { using var stream = new MemoryStream(Compile(source)); action(context.LoadFromStream(stream)); }
        finally { context.Unload(); }
    }
    private static int Invoke(string source, string name)
    {
        var result = 0;
        WithAssembly(source, assembly => result = (int)assembly.GetType("DotCcLib", true)!.GetMethod(name)!.Invoke(null, null)!);
        return result;
    }
}

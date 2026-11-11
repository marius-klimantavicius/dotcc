using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ManagedSymbolReferenceTests
{
    [Fact]
    public void Export_is_json_and_writer_migrates_and_removes_owned_metadata()
    {
        WithLibraries("int answer(void) { return 42; }", "int answer(void); int run(void) { return answer(); }",
            (manifest, _) =>
            {
                Path.GetFileName(manifest).ShouldBe("Dotcc.ManagedSymbols.json");
                var json = File.ReadAllText(manifest);
                using var document = System.Text.Json.JsonDocument.Parse(json);
                document.RootElement.GetProperty("Version").GetInt32().ShouldBe(1);
                document.RootElement.GetProperty("Owner").GetString().ShouldBe("Engine.Core");
                var directory = Path.Combine(Path.GetDirectoryName(manifest)!, "project");
                Compiler.WriteCSharpFiles(directory, new System.Collections.Generic.Dictionary<string, string>
                {
                    ["Core.cs"] = "public static class Core {}",
                    ["Dotcc.ManagedSymbols.cs"] = "//!!dotcc-managed-symbols:1 " + json
                });
                File.WriteAllText(Path.Combine(directory, "app.json"), "{}");
                Compiler.WriteCSharpFiles(directory, new System.Collections.Generic.Dictionary<string, string>
                {
                    ["Core.cs"] = "public static class Core {}",
                    [Compiler.ManagedSymbolsFile] = json
                });
                File.Exists(Path.Combine(directory, "Dotcc.ManagedSymbols.cs")).ShouldBeFalse();
                File.ReadAllText(Path.Combine(directory, Compiler.ManagedSymbolsFile)).ShouldBe(json);
                File.ReadAllLines(Path.Combine(directory, "Dotcc.SourceFiles.txt")).ShouldContain(Compiler.ManagedSymbolsFile);
                Compiler.WriteCSharpFiles(directory, new System.Collections.Generic.Dictionary<string, string>
                {
                    ["Core.cs"] = "public static class Core {}"
                });
                File.Exists(Path.Combine(directory, Compiler.ManagedSymbolsFile)).ShouldBeFalse();
                File.Exists(Path.Combine(directory, "app.json")).ShouldBeTrue();
            });
    }

    [Fact]
    public void Legacy_comment_manifest_remains_readable()
    {
        WithLibraries("struct item { int value; }; void take(struct item *p) { }",
            "struct item; void take(struct item *); void bridge(struct item *p) { take(p); }",
            (manifest, consumer) =>
            {
                var legacy = Path.ChangeExtension(manifest, ".cs");
                var compact = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(manifest))!.ToJsonString();
                File.WriteAllText(legacy, "//!!dotcc-managed-symbols:1 " + compact + "\n");
                var files = Compiler.LinkObjectFiles([consumer], emit: EmitMode.ManagedLib,
                    outputOptions: Options with { ManagedReferences = [legacy] });
                string.Join("\n", files.Values).ShouldContain("using item = global::Engine.Core.item;");
            });
    }

    [Theory]
    [InlineData("not json", "invalid managed symbol JSON")]
    [InlineData("{", "invalid managed symbol JSON")]
    [InlineData("null", "invalid or incompatible")]
    [InlineData("{\"Version\":2}", "invalid or incompatible")]
    public void Malformed_or_incompatible_manifest_is_rejected(string text, string error)
    {
        WithLibraries("int answer(void) { return 42; }", "int answer(void); int run(void) { return answer(); }",
            (manifest, consumer) =>
            {
                File.WriteAllText(manifest, text);
                Should.Throw<CompileException>(() => Compiler.LinkObjectFiles([consumer], emit: EmitMode.ManagedLib,
                    outputOptions: Options with { ManagedReferences = [manifest] })).Message.ShouldContain(error);
            });
    }

    [Fact]
    public void Dependent_library_uses_canonical_aggregate_enum_callback_and_runtime_owners()
    {
        WithLibraries("""
            typedef struct item { int value; } item;
            enum mode { FIRST, SECOND };
            struct context { item value; _Bool (*callback)(item *); enum mode mode; };
            _Bool call(struct context *c) { return c->callback(&c->value); }
            """, """
            typedef struct item { int value; } item;
            enum mode { FIRST, SECOND };
            struct context { item value; _Bool (*callback)(item *); enum mode mode; };
            _Bool call(struct context *);
            _Bool bridge(struct context *c) { return call(c); }
            _Bool (*saved)(struct context *) = call;
            """, (manifest, consumer) =>
        {
            var files = Compiler.LinkObjectFiles([consumer], emit: EmitMode.ManagedLib, className: "Consumer", namespaceName: "Client",
                outputOptions: Options with { ManagedReferences = [manifest] });
            var source = string.Join("\n", files.Values);
            source.ShouldContain("using static global::Engine.Core;");
            source.ShouldContain("global::Engine.Core.item");
            source.ShouldContain("global::Engine.Core.context");
            source.ShouldContain("global::Engine.Core.mode");
            source.ShouldNotContain("struct context\n");
            source.ShouldNotContain("struct CBool\n");
            source.ShouldContain("using static global::Engine.Core.Libc;");
            source.ShouldContain("global::Engine.Core;");
        });
    }

    [Fact]
    public void Different_complete_member_layout_is_rejected_before_emission()
    {
        WithLibraries("struct item { int value; }; void take(struct item *p) { }",
            "struct item { long value; }; void take(struct item *); void bridge(struct item *p) { take(p); }",
            (manifest, consumer) => Should.Throw<CompileException>(() =>
                Compiler.LinkObjectFiles([consumer], emit: EmitMode.ManagedLib,
                    outputOptions: Options with { ManagedReferences = [manifest] }))
                .Message.ShouldContain("incompatible managed reference aggregate 'item'"));
    }

    [Fact]
    public void Incomplete_consumer_declaration_imports_complete_canonical_type()
    {
        WithLibraries("struct item { int value; }; void take(struct item *p) { }",
            "struct item; void take(struct item *); void bridge(struct item *p) { take(p); }",
            (manifest, consumer) => string.Join("\n", Compiler.LinkObjectFiles([consumer], emit: EmitMode.ManagedLib,
                outputOptions: Options with { ManagedReferences = [manifest] }).Values)
                .ShouldContain("using item = global::Engine.Core.item;"));
    }

    [Fact]
    public void Null_manifest_type_and_function_entries_report_compile_errors()
    {
        WithLibraries("struct item { int value; }; void take(struct item *p) { }",
            "struct item; void take(struct item *); void bridge(struct item *p) { take(p); }",
            (manifest, consumer) =>
            {
                var original = File.ReadAllText(manifest);
                foreach (var mutation in new[] { "type", "function" })
                {
                    var json = System.Text.Json.Nodes.JsonNode.Parse(original)!;
                    if (mutation == "type") json["Types"]!["item"] = null;
                    else json["Functions"] = new System.Text.Json.Nodes.JsonArray((System.Text.Json.Nodes.JsonNode?)null);
                    File.WriteAllText(manifest, json.ToJsonString());
                    Should.Throw<CompileException>(() => Compiler.LinkObjectFiles([consumer], emit: EmitMode.ManagedLib,
                        outputOptions: Options with { ManagedReferences = [manifest] })).Message.ShouldContain("invalid or incompatible");
                }
            });
    }

    private static readonly CSharpOutputOptions Options = new(NestTypes: true, Runtime: RuntimeProfile.C);

    private static void WithLibraries(string core, string consumer, Action<string, string> check)
    {
        var root = Path.Combine(Path.GetTempPath(), "dotcc-managed-symbols-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string Emit(string name, string source)
            {
                var path = Path.Combine(root, name + ".c");
                File.WriteAllText(path, source);
                var obj = Path.Combine(root, name + ".cs");
                File.WriteAllText(obj, Compiler.EmitCSharp([path], emit: EmitMode.Object));
                return obj;
            }
            var coreObject = Emit("core", core);
            var consumerObject = Emit("consumer", consumer);
            var files = Compiler.LinkObjectFiles([coreObject], emit: EmitMode.ManagedLib, className: "Core", namespaceName: "Engine",
                outputOptions: Options with { ExportManagedSymbols = true });
            var manifest = Path.Combine(root, Compiler.ManagedSymbolsFile);
            File.WriteAllText(manifest, files[Compiler.ManagedSymbolsFile]);
            check(manifest, consumerObject);
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}

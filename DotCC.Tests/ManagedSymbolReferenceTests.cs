using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ManagedSymbolReferenceTests
{
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
                    const string marker = "//!!dotcc-managed-symbols:1 ";
                    var json = System.Text.Json.Nodes.JsonNode.Parse(original[marker.Length..])!;
                    if (mutation == "type") json["Types"]!["item"] = null;
                    else json["Functions"] = new System.Text.Json.Nodes.JsonArray((System.Text.Json.Nodes.JsonNode?)null);
                    File.WriteAllText(manifest, marker + json.ToJsonString());
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

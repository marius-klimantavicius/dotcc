using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Fact]
    public void Companion_imports_core_types_and_bool_callbacks_without_a_second_runtime()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-managed-reference-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            const string header = """
                typedef struct { int value; } item;
                enum mode { FIRST, SECOND };
                struct context { item value; _Bool (*callback)(item *); enum mode mode; };
                _Bool call(struct context *c);
                """;
            string Emit(string name, string source)
            {
                var path = Path.Combine(directory, name + ".c");
                File.WriteAllText(path, source);
                var obj = Path.ChangeExtension(path, ".cs");
                File.WriteAllText(obj, Compiler.EmitObject(path));
                return obj;
            }
            File.WriteAllText(Path.Combine(directory, "model.h"), header);
            var options = new CSharpOutputOptions(NestTypes: true, Runtime: RuntimeProfile.C);
            var core = Compiler.LinkObjectFiles([Emit("core", "#include \"model.h\"\n" + "_Bool call(struct context *c) { return c->callback(&c->value); }")],
                emit: EmitMode.ManagedLib, className: "Core", namespaceName: "Engine", outputOptions: options with { ExportManagedSymbols = true });
            var manifest = Path.Combine(directory, "Dotcc.ManagedSymbols.json");
            File.WriteAllText(manifest, core["Dotcc.ManagedSymbols.json"]);
            var companion = Compiler.LinkObjectFiles([Emit("companion", "#include \"model.h\"\n" + """
                static _Bool positive(item *p) { return p->value == 42; }
                _Bool (*saved)(struct context *) = call;
                int run(void) {
                    struct context c = { .value = {42}, .callback = positive, .mode = SECOND };
                    return call(&c) && saved(&c) && c.mode == SECOND ? c.value.value : -1;
                }
                """)], emit: EmitMode.ManagedLib, className: "Companion", namespaceName: "Client", split: SourceSplit.Size, splitSize: 100,
                outputOptions: options with { ManagedReferences = [manifest] });
            var references = RuntimeReferences();
            var coreImage = Compile("ManagedReferenceCore_" + Guid.NewGuid().ToString("N"), string.Join("\n", core.Where(file => file.Key.EndsWith(".cs", StringComparison.Ordinal)).Select(file => file.Value)), references);
            references.Add(MetadataReference.CreateFromImage(coreImage));
            // Split emission is compiled as separate trees: aliases must be
            // available in both the owner shell and every function file.
            var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("ManagedReferenceConsumer_" + Guid.NewGuid().ToString("N"),
                companion.Values.Select(source => ParseSource(source)), references,
                new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics));
            var context = new AssemblyLoadContext("managed-reference-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            context.LoadFromStream(new MemoryStream(coreImage));
            image.Position = 0;
            var assembly = context.LoadFromStream(image);
            assembly.GetType("Client.Companion", true)!.GetMethod("run")!.Invoke(null, null).ShouldBe(42);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}

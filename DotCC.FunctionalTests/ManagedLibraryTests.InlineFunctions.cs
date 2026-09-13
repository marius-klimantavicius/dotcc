using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false, false, SourceSplit.None)]
    [InlineData(false, true, SourceSplit.Function)]
    [InlineData(true, false, SourceSplit.Size)]
    [InlineData(true, true, SourceSplit.Function)]
    public void Inline_exports_execute_with_distinct_callbacks_and_state(bool link, bool nested, SourceSplit split)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-inline-consumer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "shared.h"), """
                typedef int (*Callback)(int);
                typedef struct Pair { int x; int y; } Pair;
                struct Hidden;
                typedef struct Holder { int value; struct Hidden* context; } Holder;
                static inline int auto_value(Holder* p) { return p->value; }
                static inline int leaf(int x) { return x + 1; }
                static inline int api_sum(Pair* p) {
                    int result = 0;
                    for (int i = 0; i < p->y; ++i) result += leaf(p->x);
                    return result;
                }
                static inline int api_recursive(int x) { return x ? api_recursive(x - 1) : 42; }
                static inline int callback(int x) { return x + 7; }
                static inline int state(void) { static int STATE; return ++STATE; }
                static inline int variant(void) { return VALUE; }
                static inline int variant_caller(void) { return variant(); }
                """);
            var paths = new[] { Path.Combine(dir, "first.c"), Path.Combine(dir, "second.c") };
            File.WriteAllText(paths[0], """
                #define STATE first_counter
                #define VALUE 11
                #include "shared.h"
                inline int external_inline(int x) { return x + 5; }
                Callback first_saved = callback;
                Callback first_pointer(void) { return first_saved; }
                int first(Pair* p) { return api_sum(p) + variant_caller(); }
                int first_state(void) { return state(); }
                static inline int api_solo(int x) { return x * 3; }
                Callback solo_saved = api_solo;
                Callback solo_pointer(void) { return solo_saved; }
                int same_solo(void) { return solo_saved == &api_solo; }
                """);
            File.WriteAllText(paths[1], """
                #define STATE second_counter
                #define VALUE 17
                #include "shared.h"
                struct Hidden { long spare; };
                int external_inline(int x);
                Callback external_saved = external_inline;
                Callback external_pointer(void) { return external_saved; }
                Callback second_saved = &callback;
                Callback second_pointer(void) { return second_saved; }
                int second(Pair* p) { return api_sum(p) + variant_caller(); }
                int second_state(void) { return state(); }
                """);
            if (link)
                paths = paths.Select(p => { var obj = Path.ChangeExtension(p, ".o"); File.WriteAllText(obj, Compiler.EmitObject(p)); return obj; }).ToArray();
            var options = new CSharpOutputOptions(NestTypes: nested, Runtime: RuntimeProfile.C,
                DeduplicateInline: true, ExportInline: new[] { "api_*" });
            var files = link
                ? Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Example", split: split, outputOptions: options)
                : Compiler.EmitCSharpFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Example", split: split, outputOptions: options);
            var references = RuntimeReferences();
            var compilation = CSharpCompilation.Create("InlineLibrary" + Guid.NewGuid().ToString("N"),
                files.Select(f => CSharpSyntaxTree.ParseText(f.Value, path: f.Key)), references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            references.Add(MetadataReference.CreateFromImage(image.ToArray()));
            var consumer = Compile("InlineConsumer" + Guid.NewGuid().ToString("N"), """
                using Example;
                using static Example.Api;
                public static unsafe class Consumer {
                    public static bool Check() {
                        Pair pair = new Pair { x = 4, y = 3 };
                        Holder holder = new Holder { value = 23 };
                        var a = Api.first_pointer();
                        var b = Api.second_pointer();
                        var solo = Api.solo_pointer();
                        var external = Api.external_pointer();
                        return Api.auto_value(&holder) == 23 && Api.api_sum(&pair) == 15 && Api.first(&pair) == 26 && Api.second(&pair) == 32
                            && Api.api_recursive(5) == 42 && a != b && a(3) == 10 && b(4) == 11
                            && a == Api.first_pointer() && b == Api.second_pointer()
                            && Api.first_state() == 1 && Api.first_state() == 2 && Api.second_state() == 1
                            && external(7) == 12 && Api.external_inline(7) == 12
                            && Api.api_solo(7) == 21 && solo(7) == 21 && Api.same_solo() == 1;
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("inline-consumer" + Guid.NewGuid(), isCollectible: false);
            image.Position = 0;
            context.LoadFromStream(image);
            var assembly = context.LoadFromStream(new MemoryStream(consumer));
            assembly.GetType("Consumer")!.GetMethod("Check")!.Invoke(null, null).ShouldBe(true);
        }
        finally { Directory.Delete(dir, true); }
    }
}

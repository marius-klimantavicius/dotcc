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
    [InlineData(false, true, SourceSplit.Size)]
    [InlineData(true, false, SourceSplit.Function)]
    [InlineData(true, true, SourceSplit.Size)]
    public void Field_type_names_preserve_cross_unit_layout_refs_and_partial_extensions(bool objects, bool nested, SourceSplit split)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-field-name-run-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "event.h"), """
                #include <stddef.h>
                #define VALUE 1
                typedef struct EventTag {
                    int kind;
                    union {
                        struct { int count; long value; } CONNECTED;
                        union { long raw; unsigned char bytes[8]; } DATA;
                    };
                } EVENT;
                """);
            var one = Path.Combine(dir, "one.c"); var two = Path.Combine(dir, "two.c");
            File.WriteAllText(one, """
                #include "event.h"
                static inline long get(EVENT *p) { return p->CONNECTED.value; }
                long inspect(EVENT *p) { return get(p) + p->CONNECTED.count; }
                unsigned long size(void) { return sizeof(EVENT); }
                unsigned long offset(void) { return offsetof(EVENT, CONNECTED.value); }
                int initialize(void) { EVENT e = { .CONNECTED = { .count = VALUE, .value = 9 } }; return inspect(&e); }
                """);
            File.WriteAllText(two, """
                #include "event.h"
                static inline long get(EVENT *p) { return p->CONNECTED.value; }
                void set(EVENT *p) { p->CONNECTED.value = get(p) + VALUE; }
                """);
            var profile = Path.Combine(dir, "overrides.json");
            File.WriteAllText(profile, """
                {"version":1,"macroOverrides":[{"name":"VALUE","replacement":"3","requireMatch":true}],
                 "fieldTypeNames":[{"field":"EVENT.CONNECTED","name":"ConnectedData","requireMatch":true},
                                   {"field":"EVENT.DATA","name":"RawData","requireMatch":true}]}
                """);
            var preprocessing = CPreprocessingOptions.Load(profile);
            var paths = new[] { one, two };
            if (objects)
                paths = paths.Select(p => { var obj = Path.ChangeExtension(p, ".o"); File.WriteAllText(obj, Compiler.EmitObject(p, preprocessing: preprocessing)); return obj; }).ToArray();
            var options = new CSharpOutputOptions(NestTypes: nested, Runtime: RuntimeProfile.C, DeduplicateInline: true);
            var files = objects
                ? Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Example", split: split, outputOptions: options)
                : Compiler.EmitCSharpFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Example", split: split, outputOptions: options, preprocessing: preprocessing);
            const string partial = "public partial struct ConnectedData { public override string ToString() => $\"{count}:{value}\"; }";
            var custom = "namespace Example { " + (nested ? "public static partial class Api { " : "") + partial + (nested ? " }" : "") + " }";
            var references = RuntimeReferences();
            var compilation = CSharpCompilation.Create("NamedTypes" + Guid.NewGuid().ToString("N"),
                files.Select(f => ParseSource(f.Value, path: f.Key, cancellationToken: TestContext.Current.CancellationToken))
                    .Append(ParseSource(custom, cancellationToken: TestContext.Current.CancellationToken)), references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            references.Add(MetadataReference.CreateFromImage(image.ToArray()));
            var consumer = Compile("NamedConsumer" + Guid.NewGuid().ToString("N"), """
                using Example;
                using static Example.Api;
                public static unsafe class Consumer {
                    private sealed class Box { public EventTag Event; }
                    public static int Run() {
                        EventTag e = default;
                        ConnectedData data = new ConnectedData { count = 4, value = 10 };
                        e.CONNECTED = data;
                        Api.set(&e);
                        if (Api.inspect(&e) != 17 || e.CONNECTED.ToString() != "4:13") return 1;
                        if (Api.size() != 24 || sizeof(EventTag) != 24 || sizeof(ConnectedData) != 16 || Api.offset() != 16) return 2;
                        if (Api.initialize() != 12) return 3;
                        var box = new Box();
                        ref ConnectedData flags = ref box.Event.CONNECTED;
                        System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                        flags.count = 8;
                        if (box.Event.CONNECTED.count != 8) return 4;
                        RawData raw = default;
                        raw.raw = 123;
                        e.DATA = raw;
                        if (e.DATA.raw != 123 || sizeof(RawData) != 8) return 5;
                        return 0;
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("field-names" + Guid.NewGuid(), isCollectible: false);
            image.Position = 0; context.LoadFromStream(image);
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer")!.GetMethod("Run")!.Invoke(null, null).ShouldBe(0);
        }
        finally { Directory.Delete(dir, true); }
    }
}

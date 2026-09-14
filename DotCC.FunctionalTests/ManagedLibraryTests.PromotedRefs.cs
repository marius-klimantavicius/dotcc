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
    [InlineData(true, false, SourceSplit.Function)]
    [InlineData(false, true, SourceSplit.None)]
    [InlineData(true, true, SourceSplit.Size)]
    public void Promoted_aggregate_refs_and_partial_extensions_preserve_storage(bool objects, bool nested, SourceSplit split)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-promoted-refs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var path = Path.Combine(dir, "settings.c");
            File.WriteAllText(path, """
                typedef struct {
                    union {
                        unsigned long IsSetFlags;
                        struct {
                            unsigned long PeerBidiStreamCount : 1;
                            unsigned long PeerUnidiStreamCount : 1;
                            unsigned long Reserved : 62;
                        } IsSet;
                    };
                    unsigned short PeerUnidiStreamCount;
                } QUIC_SETTINGS;
                struct Envelope { union { struct { QUIC_SETTINGS Settings; }; unsigned long raw; }; };
                struct ConstValue { const int value; };
                struct VolatileValue { volatile int value; };
                struct Restricted { union { struct ConstValue frozen; struct VolatileValue observed; }; };
                int inspect(QUIC_SETTINGS *p) { return p->IsSetFlags == 2 && p->PeerUnidiStreamCount == 10; }
                unsigned long size(void) { return sizeof(QUIC_SETTINGS); }
                """);
            if (objects)
            {
                var obj = Path.Combine(dir, "settings.o");
                File.WriteAllText(obj, Compiler.EmitObject(path));
                path = obj;
            }
            var options = new CSharpOutputOptions(NestTypes: nested, Runtime: RuntimeProfile.C);
            var files = objects
                ? Compiler.LinkObjectFiles(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Example", split: split, outputOptions: options)
                : Compiler.EmitCSharpFiles(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Example", split: split, outputOptions: options);
            const string extension = """
                public partial struct QUIC_SETTINGS {
                    public override bool Equals(object? other) => other is QUIC_SETTINGS value &&
                        IsSetFlags == value.IsSetFlags && PeerUnidiStreamCount == value.PeerUnidiStreamCount;
                    public override int GetHashCode() => System.HashCode.Combine(IsSetFlags, PeerUnidiStreamCount);
                    public override string ToString() => $"{IsSetFlags}:{PeerUnidiStreamCount}";
                }
                """;
            var custom = "namespace Example { " + (nested ? "public static partial class Api { " : "")
                + extension + (nested ? " }" : "") + " }";
            var references = RuntimeReferences();
            var compilation = CSharpCompilation.Create("PromotedLibrary" + Guid.NewGuid().ToString("N"),
                files.Select(f => ParseSource(f.Value, path: f.Key, cancellationToken: TestContext.Current.CancellationToken))
                    .Append(ParseSource(custom, path: "CustomSettings.cs", cancellationToken: TestContext.Current.CancellationToken)), references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            references.Add(MetadataReference.CreateFromImage(image.ToArray()));
            var consumer = Compile("PromotedConsumer" + Guid.NewGuid().ToString("N"), """
                using Example;
                using static Example.Api;
                public static unsafe class Consumer {
                    private sealed class Box { public QUIC_SETTINGS Settings; }
                    public static int Run() {
                        QUIC_SETTINGS settings = default(QUIC_SETTINGS);
                        settings.IsSet.PeerUnidiStreamCount = 1;
                        settings.PeerUnidiStreamCount = (ushort)10;
                        if (Api.inspect(&settings) != 1 || Api.size() != 16 || sizeof(QUIC_SETTINGS) != 16) return 1;
                        if ((byte*)&settings.PeerUnidiStreamCount - (byte*)&settings != 8) return 2;
                        if (settings.ToString() != "2:10" || !settings.Equals(settings)) return 3;
                        var copy = settings;
                        copy.IsSet.PeerBidiStreamCount = 1;
                        if (settings.IsSetFlags != 2 || copy.IsSetFlags != 3 || copy.Equals(settings)) return 4;
                        copy.IsSet = settings.IsSet;
                        if (!copy.Equals(settings) || copy.GetHashCode() != settings.GetHashCode()) return 5;
                        Envelope envelope = default;
                        envelope.Settings.IsSet.PeerUnidiStreamCount = 1;
                        envelope.Settings.PeerUnidiStreamCount = 10;
                        fixed (QUIC_SETTINGS* embedded = &envelope.Settings)
                            if (Api.inspect(embedded) != 1) return 6;
                        var box = new Box();
                        var array = new QUIC_SETTINGS[2];
                        ref var boxedFlags = ref box.Settings.IsSet;
                        ref var arrayFlags = ref array[1].IsSet;
                        System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                        boxedFlags.PeerUnidiStreamCount = 1;
                        arrayFlags.PeerBidiStreamCount = 1;
                        if (box.Settings.IsSetFlags != 2 || array[1].IsSetFlags != 1 || array[0].IsSetFlags != 0) return 7;
                        if (!typeof(QUIC_SETTINGS).GetProperty("IsSet")!.PropertyType.IsByRef) return 8;
                        if (typeof(Restricted).GetProperty("frozen")!.PropertyType.IsByRef || typeof(Restricted).GetProperty("frozen")!.CanWrite) return 9;
                        if (typeof(Restricted).GetProperty("observed")!.PropertyType.IsByRef) return 10;
                        return 0;
                    }
                }
                """, references);
            var context = new AssemblyLoadContext("promoted-refs" + Guid.NewGuid(), isCollectible: false);
            image.Position = 0;
            context.LoadFromStream(image);
            var assembly = context.LoadFromStream(new MemoryStream(consumer));
            assembly.GetType("Consumer")!.GetMethod("Run")!.Invoke(null, null).ShouldBe(0);
        }
        finally { Directory.Delete(dir, true); }
    }
}

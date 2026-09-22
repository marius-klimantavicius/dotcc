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
    private static FunctionOverride LittleEndianFunctionRule() => new("read_int32",
        new FunctionSignature("int32_t", new[] { "const uint8_t *" }),
        new FunctionOverrideTarget("intrinsic", "load.i32.le"), RequireMatch: true);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Registered_authored_structs_preserve_layout_and_managed_function_signatures(bool objectLink)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-typed-host-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "probe.c");
            File.WriteAllText(path, """
                #include <stddef.h>
                ManagedHandle increment(ManagedHandle);
                struct Envelope { char prefix; ManagedHandle handle; char tail; };
                int run(void) { ManagedHandle h = 41; return (int)increment(h); }
                int layout(void) {
                    return sizeof(ManagedHandle) * 1000 + offsetof(struct Envelope, handle) * 100 +
                        offsetof(struct Envelope, tail) * 10 + sizeof(struct Envelope);
                }
                """);
            var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(),
                externalTypes: new[] { new ExternalTypeOverride("ManagedHandle", new ExternalTypeLayout(4, 4)) },
                functionOverrides: new[] { new FunctionOverride("increment", new FunctionSignature("ManagedHandle", new[] { "ManagedHandle" }),
                    new FunctionOverrideTarget("managedMethod", "global::Host.Increment"), RequireMatch: true) });
            if (objectLink)
            {
                var obj = Path.ChangeExtension(path, ".o");
                File.WriteAllText(obj, Compiler.EmitObject(path, preprocessing: options));
                path = obj;
            }
            var generated = objectLink
                ? Compiler.LinkObjects(new[] { path }, emit: EmitMode.ManagedLib, className: "Api")
                : Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", preprocessing: options);
            generated.ShouldNotContain("struct ManagedHandle");
            const string host = """
                public struct ManagedHandle {
                    public int Value;
                    public static implicit operator ManagedHandle(int value) => new ManagedHandle { Value = value };
                    public static explicit operator int(ManagedHandle handle) => handle.Value;
                }
                public static class Host {
                    public static ManagedHandle Increment(ManagedHandle value) => (int)value + 1;
                }
                """;
            var compilation = CSharpCompilation.Create("TypedHost_" + Guid.NewGuid().ToString("N"),
                new[] { ParseSource(generated, cancellationToken: TestContext.Current.CancellationToken), ParseSource(host, cancellationToken: TestContext.Current.CancellationToken) }, RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("typed-host-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            var api = assembly.GetType("Api")!;
            ((int)api.GetMethod("run")!.Invoke(null, null)!).ShouldBe(42);
            ((int)api.GetMethod("layout")!.Invoke(null, null)!).ShouldBe(4492);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false, SourceSplit.None)]
    [InlineData(false, SourceSplit.Function)]
    [InlineData(true, SourceSplit.None)]
    [InlineData(true, SourceSplit.Function)]
    public void Semantic_function_load_preserves_direct_indirect_and_side_effecting_calls(bool objectLink, SourceSplit split)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-function-run-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var definition = Path.Combine(directory, "definition.c");
            var consumer = Path.Combine(directory, "consumer.c");
            File.WriteAllText(definition, """
                #include <stdint.h>
                int32_t read_int32(const uint8_t *p) {
                    return (int32_t)((uint32_t)p[0] | ((uint32_t)p[1] << 8) |
                        ((uint32_t)p[2] << 16) | ((uint32_t)p[3] << 24));
                }
                int direct(void) {
                    uint8_t bytes[5] = { 0xab, 0x78, 0x56, 0x34, 0x92 };
                    return read_int32(bytes + 1);
                }
                int argument_once(void) {
                    uint8_t bytes[6] = { 0xab, 0x78, 0x56, 0x34, 0x92, 0 };
                    uint8_t *p = bytes + 1;
                    int value = read_int32(p++);
                    return (p == bytes + 2) && (value == (int32_t)0x92345678u);
                }
                int reads_after_write(void) {
                    uint8_t bytes[4] = { 1, 0, 0, 0 };
                    int first = read_int32(bytes);
                    bytes[0] = 7;
                    return first * 10 + read_int32(bytes);
                }
                """);
            File.WriteAllText(consumer, """
                #include <stdint.h>
                int32_t read_int32(const uint8_t *);
                typedef int32_t (*Reader)(const uint8_t *);
                Reader readers[] = { read_int32, &read_int32 };
                int32_t other(const uint8_t *p) { return 73; }
                int indirect(void) {
                    uint8_t bytes[5] = { 0xab, 0x78, 0x56, 0x34, 0x92 };
                    return readers[1](bytes + 1);
                }
                int same_address(void) { return readers[0] == readers[1] && readers[0] == &read_int32; }
                int unrelated_names(void) {
                    uint8_t bytes[4] = { 0, 0, 0, 0 };
                    Reader read_int32 = other;
                    struct Holder { int read_int32; } holder = { 9 };
                    return read_int32(bytes) + holder.read_int32;
                }
                """);
            var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[] { LittleEndianFunctionRule() });
            var paths = new[] { definition, consumer };
            if (objectLink)
                paths = paths.Select(path =>
                {
                    var obj = Path.ChangeExtension(path, ".o");
                    File.WriteAllText(obj, Compiler.EmitObject(path, preprocessing: options));
                    return obj;
                }).ToArray();
            var files = objectLink
                ? Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Probe.Functions", split: split)
                : Compiler.EmitCSharpFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Probe.Functions", split: split, preprocessing: options);
            string.Join("\n", files.Values).ShouldContain("BinaryPrimitives.ReadInt32LittleEndian");
            var compilation = CSharpCompilation.Create("FunctionOverrides_" + Guid.NewGuid().ToString("N"),
                files.Select(file => ParseSource(file.Value, path: file.Key)), RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("function-overrides-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            var api = assembly.GetType("Probe.Functions.Api")!;
            int Run(string method) => (int)api.GetMethod(method)!.Invoke(null, null)!;
            Run("direct").ShouldBe(unchecked((int)0x92345678));
            Run("indirect").ShouldBe(unchecked((int)0x92345678));
            Run("same_address").ShouldBe(1);
            Run("argument_once").ShouldBe(1);
            Run("reads_after_write").ShouldBe(17);
            Run("unrelated_names").ShouldBe(82);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Declaration_only_overrides_call_authored_managed_methods_including_void(bool objectLink)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-host-functions-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "probe.c");
            File.WriteAllText(path, """
                #include <stdint.h>
                int32_t read_int32(const uint8_t *);
                void record(int value);
                int run(void) {
                    uint8_t bytes[4] = { 1, 2, 3, 4 };
                    int32_t (*reader)(const uint8_t *) = read_int32;
                    int value = reader(bytes);
                    record(value);
                    return read_int32(bytes);
                }
                """);
            var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[]
            {
                LittleEndianFunctionRule() with { Target = new FunctionOverrideTarget("managedMethod", "global::Host.ByteIo.Read") },
                new FunctionOverride("record", new FunctionSignature("void", new[] { "int" }),
                    new FunctionOverrideTarget("managedMethod", "global::Host.ByteIo.Record"), RequireMatch: true)
            });
            if (objectLink)
            {
                var obj = Path.ChangeExtension(path, ".o");
                File.WriteAllText(obj, Compiler.EmitObject(path, preprocessing: options));
                path = obj;
            }
            var generated = objectLink
                ? Compiler.LinkObjects(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Probe.Functions")
                : Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Probe.Functions", preprocessing: options);
            const string host = """
                namespace Host {
                    public static unsafe class ByteIo {
                        public static int Calls;
                        public static int Recorded;
                        public static int Read(byte* p) { Calls++; return p[0] + p[1] * 10 + p[2] * 100 + p[3] * 1000; }
                        public static void Record(int value) { Recorded = value; }
                    }
                }
                """;
            var compilation = CSharpCompilation.Create("HostFunctions_" + Guid.NewGuid().ToString("N"),
                new[] { ParseSource(generated, cancellationToken: TestContext.Current.CancellationToken), ParseSource(host, cancellationToken: TestContext.Current.CancellationToken) }, RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("host-functions-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            ((int)assembly.GetType("Probe.Functions.Api")!.GetMethod("run")!.Invoke(null, null)!).ShouldBe(4321);
            var helper = assembly.GetType("Host.ByteIo")!;
            ((int)helper.GetField("Recorded")!.GetValue(null)!).ShouldBe(4321);
            ((int)helper.GetField("Calls")!.GetValue(null)!).ShouldBe(2);
        }
        finally { Directory.Delete(directory, true); }
    }
}

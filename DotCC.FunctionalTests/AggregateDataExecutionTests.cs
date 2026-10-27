using System.Buffers.Binary;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AggregateDataExecutionTests
{
    private delegate ReadOnlySpan<byte> PayloadGetter();

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Constexpr_references_use_actual_storage_when_legacy_cache_has_wide_intermediates(bool objects, bool aggregate)
    {
        WithDirectory(directory =>
        {
            string path = Path.Combine(directory, "constexpr.c");
            File.WriteAllText(path, """
                #include <stdio.h>
                constexpr long k = 0xffffffffu + 1u;
                constexpr long selected = (0u - 1u) > 0u ? 7 : 9;
                struct Row { long value; long selected; };
                static struct Row rows[128] = {{k, selected}, [127] = {k, selected}};
                int main(void) {
                    printf("constexpr=%ld,%ld rows=%ld,%ld tail=%ld,%ld\n", k, selected,
                        rows[0].value, rows[0].selected, rows[127].value, rows[127].selected);
                    return k != 0 || selected != 7 || rows[0].value != k || rows[127].value != k
                        || rows[0].selected != selected || rows[127].selected != selected;
                }
                """);
            var dialect = CDialect.Parse("c23");
            if (objects)
            {
                string obj = path + ".o";
                File.WriteAllText(obj, Compiler.EmitObject(path, dialect: dialect,
                    outputOptions: new(AggregateData: aggregate)));
                path = obj;
            }
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, AggregateData: aggregate && !objects);
            string source = objects
                ? Compiler.LinkObjects([path], emit: EmitMode.Csproj, outputOptions: options)
                : Compiler.EmitCSharp([path], emit: EmitMode.Csproj, dialect: dialect, outputOptions: options);
            source.ShouldNotContain("// aggregate-data:");
            var result = FixtureRunner.CompileAndRunCapturingExit(source, []);
            result.exit.ShouldBe(0);
            result.stdout.ReplaceLineEndings("\n").ShouldBe("constexpr=0,7 rows=0,7 tail=0,7\n");
        });
    }

    [Theory]
    [InlineData("aggregate-byte-data", false, false)]
    [InlineData("aggregate-byte-data", false, true)]
    [InlineData("aggregate-byte-data", true, false)]
    [InlineData("aggregate-byte-data", true, true)]
    [InlineData("aggregate-byte-data-fallback", false, false)]
    [InlineData("aggregate-byte-data-fallback", false, true)]
    [InlineData("aggregate-byte-data-fallback", true, false)]
    [InlineData("aggregate-byte-data-fallback", true, true)]
    public void Native_bytes_and_fallback_semantics_execute_from_source_and_objects(string fixture, bool objects, bool aggregate)
    {
        WithDirectory(directory =>
        {
            string fixtureDirectory = FixtureDirectory(fixture);
            string[] paths = Directory.GetFiles(fixtureDirectory, "*.c").Order(StringComparer.Ordinal).ToArray();
            if (objects) paths = EmitObjects(paths, directory, aggregate, instance: false);
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, AggregateData: aggregate && !objects);
            string source = objects
                ? Compiler.LinkObjects(paths, emit: EmitMode.Csproj, outputOptions: options)
                : Compiler.EmitCSharp(paths, includeDirs: [fixtureDirectory], emit: EmitMode.Csproj, outputOptions: options);
            source.Contains("// aggregate-data:", StringComparison.Ordinal)
                .ShouldBe(aggregate && fixture == "aggregate-byte-data");
            if (aggregate && fixture == "aggregate-byte-data") AssertFixtureTablesUsePayloads(source);
            var result = FixtureRunner.CompileAndRunCapturingExit(source, []);
            result.exit.ShouldBe(0);
            result.stdout.ReplaceLineEndings("\n").ShouldBe(
                File.ReadAllText(Path.Combine(fixtureDirectory, "expected-stdout.txt")).ReplaceLineEndings("\n"));
        });
    }

    [Theory]
    [InlineData(false, "static")]
    [InlineData(true, "static")]
    [InlineData(false, "context")]
    [InlineData(true, "context")]
    [InlineData(false, "instance")]
    [InlineData(true, "instance")]
    public void Aggregate_storage_locals_and_tls_keep_owner_thread_and_mutable_identity(bool objects, string mode)
    {
        WithDirectory(directory =>
        {
            string[] paths = [Path.Combine(FixtureDirectory("aggregate-byte-data"), "data.c")];
            if (objects) paths = EmitObjects(paths, directory, aggregate: true, instance: mode == "instance");
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, NestTypes: true,
                StateContext: mode == "context", InstanceMethods: mode == "instance", AggregateData: !objects);
            var files = objects
                ? Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "AggregateState",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 2048)
                : Compiler.EmitCSharpFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "AggregateState",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 2048);
            string owner = mode == "instance" ? "first" : "Api";
            string setup = mode == "instance" ? "using var first = new Api(); using var binding = first.__DotCcEnter();"
                : mode == "context" ? "using var first = Api.__DotCcCreateContext(); using var binding = first.Enter();" : "";
            string workerBinding = mode == "instance" ? "using var workerBinding = first.__DotCcEnter();"
                : mode == "context" ? "using var workerBinding = first.Enter();" : "";
            string second = mode == "static" ? "" : $$"""
                using (var second = {{(mode == "instance" ? "new Api()" : "Api.__DotCcCreateContext()")}})
                using ({{(mode == "instance" ? "second.__DotCcEnter()" : "second.Enter()")}}) {
                    if ({{(mode == "instance" ? "second" : "Api")}}.aggregate_first() != 0x91 ||
                        {{(mode == "instance" ? "second" : "Api")}}.aggregate_address() == address ||
                        {{(mode == "instance" ? "second" : "Api")}}.aggregate_tls_step() != 17 ||
                        {{(mode == "instance" ? "second" : "Api")}}.aggregate_aligned_global() != 1 ||
                        {{(mode == "instance" ? "second" : "Api")}}.aggregate_local() != 1 ||
                        {{(mode == "instance" ? "second" : "Api")}}.aggregate_aligned_local() != 1) return 4;
                }
                if ({{owner}}.aggregate_first() != 0x42 || {{owner}}.aggregate_tls_step() != 19) return 5;
                """;
            string host = $$"""
                namespace AggregateState;
                public static class Probe {
                    public static int Run() {
                        {{setup}}
                        if ({{owner}}.aggregate_values() != 1 || {{owner}}.aggregate_zero_gaps() != 1 ||
                            {{owner}}.aggregate_wide_store() != 1 || {{owner}}.aggregate_first() != 0x91 ||
                            {{owner}}.aggregate_aligned_global() != 1) return 1;
                        ulong address = {{owner}}.aggregate_address();
                        ulong threadAddress = {{owner}}.aggregate_tls_address();
                        if ({{owner}}.aggregate_mutate() != 1 || {{owner}}.aggregate_first() != 0x42) return 2;
                        for (int i = 0; i < 3; ++i)
                            if ({{owner}}.aggregate_local() != 1 || {{owner}}.aggregate_aligned_local() != 1) return 3;
                        if ({{owner}}.aggregate_tls_step() != 17 || {{owner}}.aggregate_tls_step() != 18) return 6;
                        {{second}}
                        var addresses = new ulong[2];
                        var errors = new int[2];
                        var workers = new System.Threading.Thread[2];
                        using var barrier = new System.Threading.Barrier(3);
                        for (int i = 0; i < 2; ++i) {
                            int slot = i;
                            workers[i] = new System.Threading.Thread(() => {
                                try {
                                    {{workerBinding}}
                                    addresses[slot] = {{owner}}.aggregate_tls_address();
                                    if (!barrier.SignalAndWait(10000)) { errors[slot] = 2; return; }
                                    if ({{owner}}.aggregate_tls_step() != 17 || {{owner}}.aggregate_tls_step() != 18 ||
                                        {{owner}}.aggregate_local() != 1 || {{owner}}.aggregate_aligned_local() != 1)
                                        errors[slot] = 1;
                                } catch { errors[slot] = 3; }
                            });
                            workers[i].IsBackground = true;
                            workers[i].Start();
                        }
                        if (!barrier.SignalAndWait(10000)) return 7;
                        foreach (var worker in workers) if (!worker.Join(10000)) return 8;
                        if (errors[0] != 0 || errors[1] != 0 || addresses[0] == addresses[1] ||
                            addresses[0] == threadAddress || addresses[1] == threadAddress) return 9;
                        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced, true, true);
                        if ({{owner}}.aggregate_address() != address || {{owner}}.aggregate_first() != 0x42 ||
                            {{owner}}.aggregate_tls_address() != threadAddress ||
                            {{owner}}.aggregate_tls_step() != {{(mode == "static" ? 19 : 20)}}) return 10;
                        return 0;
                    }
                }
                """;
            var trees = files.Select(file => Parse(file.Value, file.Key)).Append(Parse(host, "Probe.cs"));
            byte[] image = Compile(trees);
            var context = new AssemblyLoadContext("aggregate-state-" + Guid.NewGuid(), isCollectible: false);
            var assembly = context.LoadFromStream(new MemoryStream(image));
            assembly.GetType("AggregateState.Probe")!.GetMethod("Run")!.Invoke(null, null).ShouldBe(0);
        });
    }

    [Theory]
    [InlineData(false, 128)]
    [InlineData(true, 128)]
    [InlineData(false, 513)]
    [InlineData(true, 513)]
    public void Shared_immutable_RVA_payload_keeps_distinct_writable_destinations_and_allocation_free_reads(bool objects, int count)
    {
        WithDirectory(directory =>
        {
            string path = Path.Combine(directory, "payload.c");
            File.WriteAllText(path, $$"""
                struct Row { int signed_value; unsigned bits; };
                #define ROWS { {-17,0x80000000u},[{{count - 1}}]={23,0xffffffffu} }
                static struct Row first[{{count}}] = ROWS;
                static struct Row second[{{count}}] = ROWS;
                int distinct(void) {
                    if (&first[0] == &second[0] || first[0].signed_value != -17 ||
                        first[{{count - 1}}].bits != 0xffffffffu || second[1].bits != 0) return 0;
                    first[0].signed_value = 99;
                    return second[0].signed_value == -17 && first[0].signed_value == 99;
                }
                """);
            string[] paths = [path];
            if (objects) paths = EmitObjects(paths, directory, aggregate: true, instance: false);
            var options = new CSharpOutputOptions(NestTypes: true, Runtime: RuntimeProfile.C, AggregateData: !objects);
            var files = objects
                ? Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "AggregatePayload",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 2048)
                : Compiler.EmitCSharpFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "AggregatePayload",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 2048);
            string.Join("\n", files.Values).ShouldContain($"// aggregate-data: {count * 8} bytes");
            byte[] image = Compile(files.Select(file => Parse(file.Value, file.Key)));
            var context = new AssemblyLoadContext("aggregate-rva-" + Guid.NewGuid(), isCollectible: true);
            try
            {
                var assembly = context.LoadFromStream(new MemoryStream(image));
                assembly.GetType("AggregatePayload.Api")!.GetMethod("distinct")!.Invoke(null, null).ShouldBe(1);
                byte[] expected = new byte[count * 8];
                BinaryPrimitives.WriteInt32LittleEndian(expected, -17);
                BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(4), 0x80000000u);
                BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan((count - 1) * 8), 23);
                BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan((count - 1) * 8 + 4), 0xffffffffu);
                var payloads = assembly.GetTypes().SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    .Where(method => method.Name.StartsWith("get_B", StringComparison.Ordinal) && method.ReturnType == typeof(ReadOnlySpan<byte>))
                    .Select(method => (Method: method, Getter: method.CreateDelegate<PayloadGetter>())).ToArray();
                using var pe = new PEReader(new MemoryStream(image));
                var metadata = pe.GetMetadataReader();
                foreach (var chunk in expected.Chunk(4096))
                {
                    var matches = payloads.Where(payload => payload.Getter().SequenceEqual(chunk)).ToArray();
                    matches.Length.ShouldBe(1); // Shared assembly backing, separate observable writable C objects.
                    var selected = matches.Single();
                    var instructions = Instructions(selected.Method.GetMethodBody()!.GetILAsByteArray()!).ToArray();
                    instructions.ShouldNotContain(instruction => instruction.Code == OpCodes.Newarr);
                    var fields = instructions.Where(instruction => instruction.Code == OpCodes.Ldtoken || instruction.Code == OpCodes.Ldsflda)
                        .Where(instruction => (instruction.Token & unchecked((int)0xff000000)) == 0x04000000)
                        .Select(instruction => metadata.GetFieldDefinition(MetadataTokens.FieldDefinitionHandle(instruction.Token & 0x00ffffff))).ToArray();
                    fields.ShouldContain(field => field.GetRelativeVirtualAddress() != 0 && pe.GetSectionData(field.GetRelativeVirtualAddress())
                        .GetContent(0, chunk.Length).SequenceEqual(chunk));
                    byte[] destination = new byte[chunk.Length];
                    for (int i=0; i<200; ++i) selected.Getter().CopyTo(destination);
                    long before = GC.GetAllocatedBytesForCurrentThread();
                    for (int i=0; i<10000; ++i) selected.Getter().CopyTo(destination);
                    (GC.GetAllocatedBytesForCurrentThread() - before).ShouldBe(0);
                    destination.ShouldBe(chunk);
                }
                // Executed JIT checks plus an actual PE RVA establish the AOT
                // intent without mistaking Roslyn execution for NativeAOT proof.
            }
            finally { context.Unload(); }
        });
    }

    private static string FixtureDirectory(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    private static void AssertFixtureTablesUsePayloads(string source)
    {
        (string Name, int Bytes)[] tables = [("packets", 1536), ("twin", 1536), ("wide", 1280),
            ("mixed", 1024), ("scalars", 1024), ("nested", 1280), ("overlays", 1024),
            ("floating", 1280), ("aligned_global", 1024)];
        foreach (var table in tables)
        {
            // Object linking may append the translation-unit suffix to static
            // symbols. Associate each marker with its own destination alias so
            // an unrelated eligible table cannot conceal a fallback.
            var declaration = Regex.Match(source, @"\b(?<alias>" + table.Name
                + @"(?:__[A-Za-z0-9_]+)?_destination)\s*=\s*[^\n]+\n\s*// aggregate-data: "
                + table.Bytes + @" bytes\b");
            declaration.Success.ShouldBeTrue("Expected byte copies for " + table.Name);
            source.ShouldContain("new global::System.Span<byte>((byte*)(" + declaration.Groups["alias"].Value + ")");
        }
    }

    private static string[] EmitObjects(string[] paths, string directory, bool aggregate, bool instance) => paths.Select((path, index) =>
    {
        string obj = Path.Combine(directory, "unit" + index + ".o");
        File.WriteAllText(obj, Compiler.EmitObject(path, includeDirs: [Path.GetDirectoryName(path)!],
            outputOptions: new(AggregateData: aggregate, InstanceMethods: instance)));
        return obj;
    }).Reverse().ToArray(); // Link order differs from direct multi-unit emission.

    private static SyntaxTree Parse(string source, string path) =>
        CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path: path);

    private static byte[] Compile(IEnumerable<SyntaxTree> trees)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("AggregateData_" + Guid.NewGuid().ToString("N"), trees, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));
        using var image = new MemoryStream();
        var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
        result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));
        return image.ToArray();
    }

    private static IEnumerable<(OpCode Code, int Token)> Instructions(byte[] il)
    {
        var codes = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(field => field.FieldType == typeof(OpCode)).Select(field => (OpCode)field.GetValue(null)!)
            .ToDictionary(code => unchecked((ushort)code.Value));
        for (int position = 0; position < il.Length;)
        {
            ushort value = il[position++];
            if (value == 0xfe) value = (ushort)(0xfe00 | il[position++]);
            var code = codes[value];
            int size = code.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, position),
                _ => 4
            };
            yield return (code, size == 4 ? BitConverter.ToInt32(il, position) : 0);
            position += size;
        }
    }

    private static void WithDirectory(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-aggregate-execution-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { action(directory); }
        finally { Directory.Delete(directory, true); }
    }
}

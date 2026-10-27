using System.Buffers.Binary;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AggregateDataStorageTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Standalone_records_and_loop_arrays_match_native_storage(bool objects, bool aggregate)
    {
        WithDirectory(directory =>
        {
            string[] paths = Directory.GetFiles(FixtureDirectory, "*.c").Order(StringComparer.Ordinal).ToArray();
            if (objects) paths = EmitObjects(paths, directory, aggregate, instance: false);
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C);
            if (aggregate && !objects) options = options with { AggregateData = true };
            string source = objects
                ? Compiler.LinkObjects(paths, emit: EmitMode.Csproj, outputOptions: options)
                : Compiler.EmitCSharp(paths, includeDirs: [FixtureDirectory], emit: EmitMode.Csproj, outputOptions: options);
            AssertPayloads([Parse(source, "Program.cs")], aggregate);
            var result = FixtureRunner.CompileAndRunCapturingExit(source, []);
            result.exit.ShouldBe(0);
            result.stdout.ReplaceLineEndings("\n").ShouldBe(
                File.ReadAllText(Path.Combine(FixtureDirectory, "expected-stdout.txt")).ReplaceLineEndings("\n"));
        });
    }

    [Theory]
    [InlineData(false, "static")]
    [InlineData(true, "static")]
    [InlineData(false, "context")]
    [InlineData(true, "context")]
    [InlineData(false, "instance")]
    [InlineData(true, "instance")]
    public void Blob_records_and_reentered_arrays_preserve_owner_and_worker_storage(bool objects, string mode)
    {
        WithDirectory(directory =>
        {
            string[] paths = [Path.Combine(FixtureDirectory, "data.c")];
            if (objects) paths = EmitObjects(paths, directory, aggregate: true, instance: mode == "instance");
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, NestTypes: true,
                StateContext: mode == "context", InstanceMethods: mode == "instance", AggregateData: !objects);
            var files = objects
                ? Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "AggregateStorage",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 2048)
                : Compiler.EmitCSharpFiles(paths, includeDirs: [FixtureDirectory], emit: EmitMode.ManagedLib,
                    className: "Api", namespaceName: "AggregateStorage", outputOptions: options,
                    split: SourceSplit.Size, splitSize: 2048);
            var trees = files.Select(file => Parse(file.Value, file.Key)).ToArray();
            AssertPayloads(trees, aggregate: true);

            string owner = mode == "instance" ? "first" : "Api";
            string setup = mode == "instance" ? "using var first = new Api(); using var binding = first.__DotCcEnter();"
                : mode == "context" ? "using var first = Api.__DotCcCreateContext(); using var binding = first.Enter();" : "";
            string workerBinding = mode == "instance" ? "using var workerBinding = first.__DotCcEnter();"
                : mode == "context" ? "using var workerBinding = first.Enter();" : "";
            string secondOwner = mode == "instance" ? "second" : "Api";
            string second = mode == "static" ? "" : $$"""
                using (var second = {{(mode == "instance" ? "new Api()" : "Api.__DotCcCreateContext()")}})
                using ({{(mode == "instance" ? "second.__DotCcEnter()" : "second.Enter()")}}) {
                    if ({{secondOwner}}.storage_static_check(-17) != 1 || {{secondOwner}}.storage_tls_check(73) != 1 ||
                        {{secondOwner}}.storage_static_alignment() != 1 || {{secondOwner}}.storage_tls_alignment() != 1 ||
                        {{secondOwner}}.storage_static_address() == address || {{secondOwner}}.storage_tls_address() == tlsAddress)
                        return "second owner initialization/identity";
                    {{secondOwner}}.storage_static_set(202);
                    if ({{secondOwner}}.storage_tls_step() != 73 || {{secondOwner}}.storage_tls_step() != 74 ||
                        {{secondOwner}}.storage_tls_check(75) != 1 || {{secondOwner}}.storage_static_check(202) != 1 ||
                        {{secondOwner}}.storage_loop(5, 2) != 1) return "second owner mutation/reentry";
                }
                if ({{owner}}.storage_static_check(101) != 1 || {{owner}}.storage_tls_check(74) != 1 ||
                    {{owner}}.storage_static_address() != address || {{owner}}.storage_tls_address() != tlsAddress)
                    return "first owner restoration";
                """;
            string host = $$"""
                namespace AggregateStorage;
                public static class Probe {
                    public static string Run() {
                        {{setup}}
                        if ({{owner}}.storage_static_check(-17) != 1 || {{owner}}.storage_tls_check(73) != 1 ||
                            {{owner}}.storage_static_alignment() != 1 || {{owner}}.storage_tls_alignment() != 1)
                            return "initial values/zero fill/alignment";
                        ulong address = {{owner}}.storage_static_address(), tlsAddress = {{owner}}.storage_tls_address();
                        {{owner}}.storage_static_set(101);
                        if ({{owner}}.storage_static_check(101) != 1 || {{owner}}.storage_tls_step() != 73 ||
                            {{owner}}.storage_tls_check(74) != 1) return "first owner mutation";
                        for (int pass = 0; pass < 3; ++pass)
                            if ({{owner}}.storage_loop(5, 2) != 1) return "loop/recursive reentry";
                        {{second}}
                        var addresses = new ulong[2];
                        var errors = new int[2];
                        var workers = new System.Threading.Thread[2];
                        using var ready = new System.Threading.CountdownEvent(2);
                        using var release = new System.Threading.ManualResetEventSlim();
                        for (int i = 0; i < workers.Length; ++i) {
                            int slot = i;
                            workers[i] = new System.Threading.Thread(() => {
                                bool signaled = false;
                                try {
                                    {{workerBinding}}
                                    addresses[slot] = {{owner}}.storage_tls_address();
                                    if ({{owner}}.storage_tls_check(73) != 1 || {{owner}}.storage_tls_alignment() != 1 ||
                                        {{owner}}.storage_tls_step() != 73 || {{owner}}.storage_tls_step() != 74 ||
                                        {{owner}}.storage_tls_check(75) != 1 || {{owner}}.storage_static_check(101) != 1 ||
                                        {{owner}}.storage_static_address() != address || {{owner}}.storage_loop(5, 2) != 1)
                                        errors[slot] = 1;
                                    ready.Signal(); signaled = true;
                                    if (!release.Wait(10000)) { errors[slot] = 2; return; }
                                    if ({{owner}}.storage_tls_address() != addresses[slot] || {{owner}}.storage_tls_check(75) != 1 ||
                                        {{owner}}.storage_static_address() != address || {{owner}}.storage_static_check(101) != 1 ||
                                        {{owner}}.storage_loop(4, 1) != 1) errors[slot] = 3;
                                } catch { errors[slot] = 4; }
                                finally { if (!signaled) ready.Signal(); }
                            });
                            workers[i].IsBackground = true;
                            workers[i].Start();
                        }
                        bool initialized = ready.Wait(10000);
                        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced, true, true);
                        bool retained = {{owner}}.storage_static_address() == address && {{owner}}.storage_static_check(101) == 1 &&
                            {{owner}}.storage_tls_address() == tlsAddress && {{owner}}.storage_tls_check(74) == 1;
                        release.Set();
                        foreach (var worker in workers) if (!worker.Join(10000)) return "worker join timeout";
                        if (!initialized) return "worker initialization timeout";
                        if (errors[0] != 0 || errors[1] != 0) return "worker storage: " + errors[0] + "," + errors[1];
                        if (addresses[0] == 0 || addresses[1] == 0 || addresses[0] == addresses[1] ||
                            addresses[0] == tlsAddress || addresses[1] == tlsAddress) return "worker TLS identity";
                        if (!retained || {{owner}}.storage_tls_step() != 74 || {{owner}}.storage_tls_check(75) != 1)
                            return "retained owner/TLS storage";
                        return "ok";
                    }
                }
                """;
            byte[] image = Compile(trees.Append(Parse(host, "Probe.cs")));
            var context = new AssemblyLoadContext("aggregate-storage-" + Guid.NewGuid(), isCollectible: true);
            try
            {
                var assembly = context.LoadFromStream(new MemoryStream(image));
                assembly.GetType("AggregateStorage.Probe")!.GetMethod("Run")!.Invoke(null, null).ShouldBe("ok");
            }
            finally { context.Unload(); }
        });
    }

    private static void AssertPayloads(SyntaxTree[] trees, bool aggregate)
    {
        var roots = trees.Select(tree => tree.GetRoot()).ToArray();
        string[] markers = roots.SelectMany(root => root.DescendantTrivia())
            .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            .Select(trivia => trivia.ToString()).Where(text => text.StartsWith("// aggregate-data:", StringComparison.Ordinal)).ToArray();
        markers.Length.ShouldBe(aggregate ? 4 : 0);
        if (!aggregate) return;

        var calls = roots.SelectMany(root => root.DescendantNodes().OfType<InvocationExpressionSyntax>()).ToArray();
        var properties = roots.SelectMany(root => root.DescendantNodes().OfType<PropertyDeclarationSyntax>()).ToArray();
        foreach (var (destination, bytes) in ExpectedPayloads())
        {
            markers.Count(marker => marker == $"// aggregate-data: {bytes.Length} bytes").ShouldBe(1, destination);
            string payloadName = "B" + Convert.ToHexString(SHA256.HashData(bytes));
            var copies = calls.Where(call => call.Expression is MemberAccessExpressionSyntax access &&
                access.Name.Identifier.ValueText == "CopyTo" && access.Expression is MemberAccessExpressionSyntax payload &&
                payload.Name.Identifier.ValueText == payloadName).ToArray();
            copies.Length.ShouldBe(1, destination + " must copy its own expected aggregate payload");
            string destinationPattern = destination.EndsWith("_destination", StringComparison.Ordinal)
                ? "^" + Regex.Escape(destination[..^"_destination".Length]) + @"(?:__[A-Za-z0-9_]+)?_destination$"
                : "^" + Regex.Escape(destination) + "$";
            copies[0].ArgumentList.DescendantNodes().OfType<IdentifierNameSyntax>()
                .ShouldContain(name => Regex.IsMatch(name.Identifier.ValueText, destinationPattern),
                    "The expected payload must initialize " + destination);
            var payloads = properties.Where(property => property.Identifier.ValueText == payloadName).ToArray();
            payloads.Length.ShouldBe(1, destination);
            // Check the actual emitted bytes as well as their hashed identity.
            var collection = payloads[0].ExpressionBody!.Expression.ShouldBeOfType<CollectionExpressionSyntax>();
            byte[] actual = collection.Elements.Select(element =>
                Convert.ToByte(((LiteralExpressionSyntax)((ExpressionElementSyntax)element).Expression).Token.Value)).ToArray();
            actual.ShouldBe(bytes, destination);
        }
    }

    private static IEnumerable<(string Destination, byte[] Bytes)> ExpectedPayloads()
    {
        var record = new byte[1036];
        Cell(record, 0, -17, 0x80000000);
        Cell(record, 64, int.MinValue, 0xa5a55a5a);
        Cell(record, 128, 23, 0xffffffff);
        BinaryPrimitives.WriteUInt32LittleEndian(record.AsSpan(1032), 0x12345678);
        yield return ("storage_record_destination", record);

        var thread = new byte[1028];
        BinaryPrimitives.WriteUInt32LittleEndian(thread, 73);
        BinaryPrimitives.WriteUInt32LittleEndian(thread.AsSpan(127 * 4), 0x80000001);
        BinaryPrimitives.WriteUInt32LittleEndian(thread.AsSpan(255 * 4), 0xfedcba98);
        BinaryPrimitives.WriteUInt32LittleEndian(thread.AsSpan(1024), 5u | (0x12fu << 3) | (111u << 12) | (0x1234u << 19));
        yield return ("storage_thread_record_destination", thread);

        var rows = new byte[1024];
        Cell(rows, 0, 31, 0x01020304);
        Cell(rows, 64, -41, 0x80000002);
        Cell(rows, 127, 47, 0xaabbccdd);
        yield return ("loop_rows", rows);

        var aligned = new byte[1032];
        Cell(aligned, 0, -53, 0x76543210);
        Cell(aligned, 65, 59, 0xfffffffe);
        Cell(aligned, 128, 61, 0x10203040);
        yield return ("loop_aligned", aligned);
    }

    private static void Cell(byte[] bytes, int index, int value, uint bits)
    {
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(index * 8), value);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(index * 8 + 4), bits);
    }

    private static string FixtureDirectory => Path.Combine(AppContext.BaseDirectory, "Fixtures", "aggregate-byte-data-storage");

    private static string[] EmitObjects(string[] paths, string directory, bool aggregate, bool instance) => paths.Select((path, index) =>
    {
        var options = new CSharpOutputOptions(InstanceMethods: instance);
        if (aggregate) options = options with { AggregateData = true };
        string obj = Path.Combine(directory, "unit" + index + ".o");
        File.WriteAllText(obj, Compiler.EmitObject(path, includeDirs: [FixtureDirectory], outputOptions: options));
        return obj;
    }).Reverse().ToArray();

    private static SyntaxTree Parse(string source, string path) =>
        CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path: path);

    private static byte[] Compile(IEnumerable<SyntaxTree> trees)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("AggregateStorage_" + Guid.NewGuid().ToString("N"), trees, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));
        using var image = new MemoryStream();
        var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
        result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));
        return image.ToArray();
    }

    private static void WithDirectory(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-aggregate-storage-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { action(directory); }
        finally { Directory.Delete(directory, true); }
    }
}

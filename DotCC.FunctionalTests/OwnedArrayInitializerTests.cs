using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class OwnedArrayInitializerTests
{
    [Theory]
    [InlineData(0, "static")]
    [InlineData(1, "static")]
    [InlineData(2, "static")]
    [InlineData(0, "context")]
    [InlineData(1, "context")]
    [InlineData(2, "context")]
    [InlineData(0, "instance")]
    [InlineData(1, "instance")]
    [InlineData(2, "instance")]
    public void Owned_payload_helpers_preserve_storage_evaluation_and_lifetimes(int route, string mode)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-owned-payload-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "owned-array-initializers");
            string[] paths = [Path.Combine(fixture, "data.c"), Path.Combine(fixture, "main.c")];
            if (route != 0)
            {
                paths = paths.Select((path, index) =>
                {
                    string fragment = Path.Combine(directory, "unit" + index + ".o");
                    File.WriteAllText(fragment, Compiler.EmitObject(path, outputOptions: new(InstanceMethods: mode == "instance")));
                    return fragment;
                }).ToArray();
                if (route == 2) Array.Reverse(paths);
            }
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, LiteralPool: false, NestTypes: true,
                InstanceMethods: mode == "instance", StateContext: mode == "context");
            var files = route == 0
                ? Compiler.EmitCSharpFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Owned",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 4096)
                : Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Owned",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 4096);
            string source = string.Join("\n", files.Values);
            source.ShouldNotContain("GlobalArrayFrom<Token>");
            source.ShouldNotContain("new Token[]");
            source.ShouldNotContain("PinFnPtrArray(new ");
            source.ShouldNotContain("/*__dotcc_owner_member__*/");
            var trees = files.Select(file => CSharpSyntaxTree.ParseText(file.Value,
                new CSharpParseOptions(LanguageVersion.Preview), path: file.Key)).ToList();
            var fill = trees.SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                .First(method => method.Identifier.ValueText.StartsWith("__dotcc_owned_array_", StringComparison.Ordinal)
                    && method.Identifier.ValueText.Contains("_Fill_", StringComparison.Ordinal)
                    && method.ParameterList.Parameters.Single().Type!.ToString() == "Token*"
                    && method.Body!.ToString().Contains("[2]", StringComparison.Ordinal));
            string owner = mode == "instance" ? "first" : "Api";
            string setup = mode == "instance" ? "using var first = new Api(); using var binding = first.__DotCcEnter();"
                : mode == "context" ? "using var first = Api.__DotCcCreateContext(); using var binding = first.Enter();" : "";
            string bindWorker = mode == "instance" ? "using var workerBinding = first.__DotCcEnter();"
                : mode == "context" ? "using var workerBinding = first.Enter();" : "";
            string second = mode == "static" ? "" : $$"""
                using (var second = {{(mode == "instance" ? "new Api()" : "Api.__DotCcCreateContext()")}})
                using ({{(mode == "instance" ? "second.__DotCcEnter()" : "second.Enter()")}}) {
                    if ({{(mode == "instance" ? "second" : "Api")}}.owned_first() != -17 ||
                        {{(mode == "instance" ? "second" : "Api")}}.owned_static_step() != 44 ||
                        {{(mode == "instance" ? "second" : "Api")}}.owned_address() == address) return 5;
                }
                if ({{owner}}.owned_first() != 99) return 6;
                """;
            trees.Add(CSharpSyntaxTree.ParseText($$"""
                using Token = global::Owned.Api.Token;
                namespace Owned;
                public static unsafe class Probe {
                    public static int Run() {
                        {{setup}}
                        if ({{owner}}.owned_check() != 1 || {{owner}}.owned_first() != -17 ||
                            {{owner}}.owned_mutate() != 1 || {{owner}}.owned_check() != 1) return 1;
                        ulong address = {{owner}}.owned_address();
                        if ({{owner}}.owned_static_step() != 44 || {{owner}}.owned_static_step() != 45) return 2;
                        if ({{owner}}.owned_expressions(0) != 1 || {{owner}}.owned_expressions(1) != 1) return 3;
                        {{second}}
                        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced, true, true);
                        if ({{owner}}.owned_check() != 1 || {{owner}}.owned_address() != address) return 7;
                        if ({{owner}}.owned_thread_step() != 41 || {{owner}}.owned_thread_step() != 42) return 8;
                        var addresses = new ulong[2];
                        var errors = new int[2];
                        var workers = new System.Threading.Thread[2];
                        for (int i = 0; i < 2; ++i) {
                            int slot = i;
                            workers[i] = new System.Threading.Thread(() => {
                                try {
                                    {{bindWorker}}
                                    addresses[slot] = {{owner}}.owned_thread_address();
                                    if ({{owner}}.owned_thread_step() != 41 || {{owner}}.owned_thread_step() != 42 ||
                                        {{owner}}.owned_check() != 1) errors[slot] = 1;
                                } catch { errors[slot] = 2; }
                            });
                            workers[i].Start();
                        }
                        foreach (var worker in workers) if (!worker.Join(10000)) return 9;
                        if (errors[0] != 0 || errors[1] != 0 || addresses[0] == addresses[1] ||
                            addresses[0] == {{owner}}.owned_thread_address() || addresses[1] == {{owner}}.owned_thread_address()) return 10;
                        if ({{owner}}.owned_thread_step() != 43) return 11;
                        // Destination storage is allocated once, outside the measured
                        // window. Exercise the actual emitted row-copy caller directly.
                        void* destination = System.Runtime.InteropServices.NativeMemory.AllocZeroed((nuint)(3 * sizeof(Token)));
                        try {
                            for (int i = 0; i < 200; ++i) {{owner}}.{{fill.Identifier.ValueText}}((Token*)destination);
                            long before = System.GC.GetAllocatedBytesForCurrentThread();
                            for (int i = 0; i < 10000; ++i) {{owner}}.{{fill.Identifier.ValueText}}((Token*)destination);
                            if (System.GC.GetAllocatedBytesForCurrentThread() != before) return 12;
                            Token* rows = (Token*)destination;
                            if (rows[0].value != -17 || rows[1].code != 32768 || rows[2].owner != null ||
                                *rows[0].owner != 7 || rows[0].apply({{(mode == "instance" ? "first, " : "")}}5) != 8) return 13;
                        } finally { System.Runtime.InteropServices.NativeMemory.Free(destination); }
                        return 0;
                    }
                }
                """, new CSharpParseOptions(LanguageVersion.Preview), path: "Probe.cs"));
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path));
            var compilation = CSharpCompilation.Create("Owned" + Guid.NewGuid().ToString("N"), trees, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));
            using var stream = new MemoryStream();
            var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            var context = new AssemblyLoadContext("owned-" + Guid.NewGuid(), isCollectible: false);
            var assembly = context.LoadFromStream(new MemoryStream(stream.ToArray()));
            assembly.GetType("Owned.Probe")!.GetMethod("Run")!.Invoke(null, null).ShouldBe(0);
            InspectHelperClosure(assembly);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static void InspectHelperClosure(Assembly assembly)
    {
        var pending = new Queue<MethodBase>(assembly.GetTypes().SelectMany(type =>
                type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(method => method.Name.StartsWith("__dotcc_owned_array_", StringComparison.Ordinal)));
        pending.Count.ShouldBeGreaterThan(0);
        var seen = new HashSet<MethodBase>();
        var codes = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(field => field.FieldType == typeof(OpCode)).Select(field => (OpCode)field.GetValue(null)!)
            .ToDictionary(code => unchecked((ushort)code.Value));
        while (pending.TryDequeue(out var method))
        {
            if (!seen.Add(method) || method.GetMethodBody() is not { } body) continue;
            byte[] il = body.GetILAsByteArray()!;
            for (int position = 0; position < il.Length;)
            {
                ushort value = il[position++];
                if (value == 0xfe) value = (ushort)(0xfe00 | il[position++]);
                var code = codes[value];
                code.ShouldNotBe(OpCodes.Newarr, method.Name);
                int size = code.OperandType switch
                {
                    OperandType.InlineNone => 0,
                    OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                    OperandType.InlineVar => 2,
                    OperandType.InlineI8 or OperandType.InlineR => 8,
                    OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, position),
                    _ => 4
                };
                if (code == OpCodes.Call || code == OpCodes.Callvirt || code == OpCodes.Newobj)
                {
                    var target = method.Module.ResolveMethod(BitConverter.ToInt32(il, position))!;
                    if (code == OpCodes.Newobj)
                        (target.DeclaringType!.IsArray || typeof(Delegate).IsAssignableFrom(target.DeclaringType))
                            .ShouldBeFalse(method.Name + " -> " + target);
                    // These calls own required writable C storage. Their payload
                    // callers and every other same-assembly callee stay in scope.
                    bool destination = target.DeclaringType?.Name == "Libc"
                        && target.Name is "GlobalArrayZeroed" or "GlobalAlignedZeroed" or "GlobalArrayFrom" or "GlobalAlignedFrom";
                    if (target.Module.Assembly == assembly && !destination) pending.Enqueue(target);
                }
                position += size;
            }
        }
    }
}

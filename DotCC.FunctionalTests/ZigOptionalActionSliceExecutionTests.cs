using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigOptionalActionSliceExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Actual_parser_action_optional_slices_preserve_storage_without_allocations(bool objects)
    {
        var graphPath = Environment.GetEnvironmentVariable("DOTCC_LIBGHOSTTY_GRAPH");
        if (string.IsNullOrEmpty(graphPath)) Assert.Skip("Set DOTCC_LIBGHOSTTY_GRAPH to the pinned direct engine module graph.");
        var graph = ZigCompilationOptions.Load(graphPath);
        var repository = Repository();
        var source = Path.Combine(repository, "DotCC.FunctionalTests", "Fixtures", "ZigOptionalActionSlices", "actions.zig");
        var root = graph.Modules.Single(module => module.Name == graph.RootModule);
        var modules = graph.Modules.Where(module => module.Name != root.Name).ToList();
        modules.Add(new("vt", root.Root, root.Dependencies));
        modules.Add(new(root.Name, source, [new("vt", "vt")]));
        var options = new ZigCompilationOptions(root.Name, modules, zigLibDir: graph.ZigLibDir,
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed,
            managedRoots: ["answer"]);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-optional-action-slices-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
            emitted.ShouldContain("OptionalSlice<");
            emitted += """

                public static class OptionalActionSliceHost {
                    public static int Main() {
                        for (int i = 0; i < 16; i++) if (DotCcLib.answer() != 42) return 1;
                        long before = global::System.GC.GetAllocatedBytesForCurrentThread();
                        for (int i = 0; i < 1000; i++) if (DotCcLib.answer() != 42) return 2;
                        return global::System.GC.GetAllocatedBytesForCurrentThread() == before ? 42 : 3;
                    }
                }
                """;
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Repository([CallerFilePath] string file = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, ".."));
}

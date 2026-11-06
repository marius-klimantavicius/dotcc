using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigWidePackedExecutionTests
{
    [Theory]
    [InlineData(false, "main.zig")]
    [InlineData(true, "main.zig")]
    [InlineData(false, "flags.zig")]
    [InlineData(true, "flags.zig")]
    public void Wide_packed_defaults_crossing_lanes_nested_places_and_storage_execute(bool objects, string fixture)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-wide-packed-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(Path.GetDirectoryName(Fixture())!, fixture);
            var options = new ZigCompilationOptions("root", [new("root", path, [])], functionPointerAbi: ZigFunctionPointerAbi.Managed);
            var output = new CSharpOutputOptions(AggregateData: true);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options, outputOptions: output));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options, outputOptions: output);
            // Measure inside the generated assembly, outside reflection/runner allocations.
            var host = """
                public static class WidePackedHost {
                    public static int Main() {
                        for (int i = 0; i < 16; i++) if (DotCcLib.main() != 42) return 90;
                        long before = global::System.GC.GetAllocatedBytesForCurrentThread();
                        for (int i = 0; i < 100; i++) if (DotCcLib.main() != 42) return 91;
                        return global::System.GC.GetAllocatedBytesForCurrentThread() == before ? 42 : 92;
                    }
                }
                """;
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted + "\n" + host, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixture([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigWidePacked", "main.zig");
}

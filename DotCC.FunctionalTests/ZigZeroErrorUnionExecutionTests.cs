using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigZeroErrorUnionExecutionTests
{
    [Theory]
    [InlineData(false, "effects.zig")]
    [InlineData(true, "effects.zig")]
    [InlineData(false, "future.zig")]
    [InlineData(true, "future.zig")]
    public void Zero_payload_errors_and_future_presence_survive_source_and_objects(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(FixtureDirectory(), fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])],
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zero-error-union-" + Guid.NewGuid().ToString("N"));
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
            emitted.ShouldContain("ZigErrorUnionZero<Unit>");
            emitted += "\npublic static class ZeroErrorUnionHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigZeroErrorUnion");
}

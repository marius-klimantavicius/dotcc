using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigZeroWidthIntegerExecutionTests
{
    [Theory]
    [InlineData(false, "metadata.zig")]
    [InlineData(true, "metadata.zig")]
    [InlineData(false, "effects.zig")]
    [InlineData(true, "effects.zig")]
    [InlineData(false, "bitset.zig")]
    [InlineData(true, "bitset.zig")]
    public void Zero_width_source_identity_and_scalar_values_survive_source_and_objects(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(FixtureDirectory(), fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])],
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: fixture == "effects.zig" ? ["answer", "rejects"] : ["answer"]);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zero-width-" + Guid.NewGuid().ToString("N"));
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
            emitted += fixture == "effects.zig"
                ? "\npublic static class ZeroWidthHost { public static int Main() { if (DotCcLib.answer() != 42) return 1; try { DotCcLib.rejects(1); return 2; } catch (System.OverflowException) { return 42; } } }"
                : "\npublic static class ZeroWidthHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false, "nonzero-rejected.zig")]
    [InlineData(true, "nonzero-rejected.zig")]
    [InlineData(false, "array-cut.zig")]
    [InlineData(true, "array-cut.zig")]
    public void Nonzero_values_and_unproved_zero_stride_storage_are_rejected(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(FixtureDirectory(), fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])],
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        Should.Throw<CompileException>(() =>
        {
            if (objects) Compiler.EmitObject(source, zigOptions: options);
            else Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
        }).Message.ShouldContain("u0");
    }

    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigZeroWidthInteger");
}

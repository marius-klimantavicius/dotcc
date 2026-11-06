using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigManagedArrayPointerExecutionTests
{
    [Theory]
    [InlineData(false, "member-address.zig")]
    [InlineData(true, "member-address.zig")]
    [InlineData(false, "nested-return.zig")]
    [InlineData(true, "nested-return.zig")]
    [InlineData(false, "pointer-depth.zig")]
    [InlineData(true, "pointer-depth.zig")]
    [InlineData(false, "sentinel-argument.zig")]
    [InlineData(true, "sentinel-argument.zig")]
    public void Array_storage_and_pointer_depth_survive_managed_lowering(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(FixtureDirectory(), fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])],
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-managed-array-pointers-" + Guid.NewGuid().ToString("N"));
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
            emitted += "\npublic static class ArrayPointerHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigManagedArrayPointers");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Sentinel_decay_keeps_const_elements_readonly(bool objects)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(FixtureDirectory(), "const-sentinel-rejected.zig");
        var options = new ZigCompilationOptions("root", [new("root", source, [])],
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        Should.Throw<CompileException>(() =>
        {
            if (objects) Compiler.EmitObject(source, zigOptions: options);
            else Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
        }).Message.ShouldContain("const sentinel slice");
    }
}

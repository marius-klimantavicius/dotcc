using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigTestingEqualityExecutionTests
{
    [Theory]
    [InlineData(false, "scalars.zig")]
    [InlineData(true, "scalars.zig")]
    [InlineData(false, "source-struct.zig")]
    [InlineData(true, "source-struct.zig")]
    [InlineData(false, "type-equality.zig")]
    [InlineData(true, "type-equality.zig")]
    [InlineData(false, "scalar-peers.zig")]
    [InlineData(true, "scalar-peers.zig")]
    [InlineData(false, "source-pointers.zig")]
    [InlineData(true, "source-pointers.zig")]
    [InlineData(false, "source-bool-slices.zig")]
    [InlineData(true, "source-bool-slices.zig")]
    [InlineData(false, "source-error-void.zig")]
    [InlineData(true, "source-error-void.zig")]
    [InlineData(false, "typed-constant-peers.zig")]
    [InlineData(true, "typed-constant-peers.zig")]
    public void Assertions_keep_enum_failures_ieee_values_and_real_source_struct_comparison(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(FixtureDirectory(), fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])],
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-testing-equality-" + Guid.NewGuid().ToString("N"));
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
            if (fixture == "source-struct.zig") emitted.ShouldContain("testing__expectEqual");
            emitted += "\npublic static class TestingEqualityHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false, "runtime-signed-peer-invalid.zig")]
    [InlineData(true, "runtime-signed-peer-invalid.zig")]
    [InlineData(false, "negative-constant-peer-invalid.zig")]
    [InlineData(true, "negative-constant-peer-invalid.zig")]
    public void Runtime_signed_domains_and_unrepresentable_constants_stay_rejected(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(FixtureDirectory(), fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])],
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        var exception = Should.Throw<CompileException>(() =>
        {
            if (objects) Compiler.EmitObject(source, zigOptions: options);
            else Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
        });
        exception.Message.ShouldContain("expected type 'u64', found 'i32'");
    }
    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigTestingEquality");
}

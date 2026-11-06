using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigLazyOptionalFallbackExecutionTests
{
    [Theory]
    [InlineData(false, "lazy.zig")]
    [InlineData(true, "lazy.zig")]
    [InlineData(false, "void-return.zig")]
    [InlineData(true, "void-return.zig")]
    [InlineData(false, "error-switch-return.zig")]
    [InlineData(true, "error-switch-return.zig")]
    [InlineData(false, "void-arguments.zig")]
    [InlineData(true, "void-arguments.zig")]
    [InlineData(false, "reflected-defaults.zig")]
    [InlineData(true, "reflected-defaults.zig")]
    public void Exit_arms_remain_conditional_and_preserve_their_enclosing_return(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Fixture(fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])], outputMode: ZigSourceOutputMode.Lib,
            optimizeMode: ZigSourceOptimizeMode.Debug, zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed,
            managedRoots: fixture == "lazy.zig" ? ["answer", "probe", "seen"] : ["answer"]);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-lazy-optional-" + Guid.NewGuid().ToString("N"));
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
            emitted += fixture == "lazy.zig"
                ? "\npublic static class LazyOptionalHost { public static int Main() { if (DotCcLib.answer() != 42) return 1; try { DotCcLib.probe(false); return 2; } catch (System.Diagnostics.UnreachableException) { return DotCcLib.seen() == 2 ? 42 : 3; } } }"
                : "\npublic static class LazyOptionalHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42); result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixture(string fixture, [CallerFilePath] string caller = "")
        => Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "ZigLazyOptionalFallback", fixture);
}

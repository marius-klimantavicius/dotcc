using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigComptimeOnlyErrorExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Runtime_diagnostics_do_not_format_the_comptime_only_error(bool objects)
    {
        var result = Emit("positive.zig", objects);
        var execution = FixtureRunner.CompileAndRunCapturingStreams(result
            + "\npublic static class ComptimeOnlyErrorHost { public static int Main() => (int)DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
        execution.exit.ShouldBe(42); execution.stdout.ShouldBeEmpty(); execution.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false, "selected.zig")]
    [InlineData(true, "selected.zig")]
    [InlineData(false, "unconditional.zig")]
    [InlineData(true, "unconditional.zig")]
    public void Reached_compile_errors_still_reject(bool objects, string fixture)
        => Should.Throw<CompileException>(() => Emit(fixture, objects)).Message
            .ShouldContain(fixture == "selected.zig" ? "comptime" : "unconditional failure");

    private static string Emit(string fixture, bool objects, [CallerFilePath] string caller = "")
    {
        var source = Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "ZigComptimeOnlyErrors", fixture);
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var options = new ZigCompilationOptions("root", [new("root", source, [])], outputMode: ZigSourceOutputMode.Lib,
            optimizeMode: ZigSourceOptimizeMode.Debug, zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        if (!objects) return Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-comptime-only-errors-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var fragment = Path.Combine(directory, "main.csobj");
            File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
            return Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
        }
        finally { Directory.Delete(directory, true); }
    }
}

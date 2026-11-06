using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigComptimeStringCallExecutionTests
{
    [Theory]
    [InlineData(false, "positive.zig")]
    [InlineData(true, "positive.zig")]
    [InlineData(false, "descriptor.zig")]
    [InlineData(true, "descriptor.zig")]
    [InlineData(false, "named-integer-tuple.zig")]
    [InlineData(true, "named-integer-tuple.zig")]
    public void Source_calls_supply_exact_comptime_string_bytes(bool objects, string fixture)
    {
        var emitted = Emit(fixture, objects);
        var result = FixtureRunner.CompileAndRunCapturingStreams(emitted
            + "\npublic static class StringCallHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
        result.exit.ShouldBe(42); result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Runtime_arguments_cannot_supply_a_comptime_string(bool objects)
        => Should.Throw<CompileException>(() => Emit("runtime.zig", objects)).Message.ShouldContain("compile-time-known string");

    private static string Emit(string fixture, bool objects, [CallerFilePath] string caller = "")
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "ZigComptimeStringCall", fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])], zigLibDir: library,
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Debug,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        if (!objects) return Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-string-call-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try {
            var fragment = Path.Combine(directory, "main.csobj");
            File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
            return Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
        } finally { Directory.Delete(directory, true); }
    }
}

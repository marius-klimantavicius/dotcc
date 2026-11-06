using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigComptimeRecordLocalExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Source_comptime_records_keep_real_defaults_and_distinct_constructor_values(bool objects)
    {
        var emitted = Emit("positive.zig", objects);
        var result = FixtureRunner.CompileAndRunCapturingStreams(emitted
            + "\npublic static class ComptimeRecordLocalHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
        result.exit.ShouldBe(42); result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false, "runtime.zig")]
    [InlineData(true, "runtime.zig")]
    public void Runtime_state_cannot_initialize_a_comptime_only_record(bool objects, string fixture)
        => Should.Throw<CompileException>(() => Emit(fixture, objects)).Message.ShouldContain("comptime-only local");

    private static string Emit(string fixture, bool objects, [CallerFilePath] string caller = "")
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "ZigComptimeRecordLocal", fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])], zigLibDir: library,
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        if (!objects) return Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-comptime-record-local-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try {
            var fragment = Path.Combine(directory, "main.csobj");
            File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
            return Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
        } finally { Directory.Delete(directory, true); }
    }
}

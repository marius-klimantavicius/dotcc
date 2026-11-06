using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigConditionalTagTypeExecutionTests
{
    [Theory]
    [InlineData(false, "positive.zig")]
    [InlineData(true, "positive.zig")]
    [InlineData(false, "debug-payload.zig")]
    [InlineData(true, "debug-payload.zig")]
    public void Constant_conditional_tags_choose_only_the_selected_source_type(bool objects, string fixture)
    {
        var emitted = Emit(fixture, objects);
        var result = FixtureRunner.CompileAndRunCapturingStreams(emitted
            + "\npublic static class ConditionalTagTypeHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
        result.exit.ShouldBe(42); result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false, "runtime.zig")]
    [InlineData(true, "runtime.zig")]
    public void Runtime_conditions_cannot_choose_a_source_type(bool objects, string fixture)
        => Should.Throw<CompileException>(() => Emit(fixture, objects)).Message.ShouldContain("type spelled as a `switch`");

    private static string Emit(string fixture, bool objects, [CallerFilePath] string caller = "")
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "ZigConditionalTagType", fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])], zigLibDir: library,
            outputMode: ZigSourceOutputMode.Lib,
            optimizeMode: fixture == "debug-payload.zig" ? ZigSourceOptimizeMode.Debug : ZigSourceOptimizeMode.Fast,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        if (!objects) return Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-conditional-tag-type-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try {
            var fragment = Path.Combine(directory, "main.csobj");
            File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
            return Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
        } finally { Directory.Delete(directory, true); }
    }
}

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigComptimeLabeledExtentExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Source_order_mutation_supplies_later_array_extents_and_cleanup_preserves_aliases(bool objects)
    {
        var emitted = Emit("positive.zig", objects);
        var result = FixtureRunner.CompileAndRunCapturingStreams(emitted
            + "\npublic static class ComptimeLabeledExtentHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
        result.exit.ShouldBe(42); result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false, "runtime-bound.zig", "constant integer expression")]
    [InlineData(true, "runtime-bound.zig", "constant integer expression")]
    [InlineData(false, "reached-validation.zig", "reached counter validation")]
    [InlineData(true, "reached-validation.zig", "reached counter validation")]
    public void Unknown_extents_and_reached_validations_fail(bool objects, string fixture, string diagnostic)
        => Should.Throw<CompileException>(() => Emit(fixture, objects)).Message.ShouldContain(diagnostic);

    private static string Emit(string fixture, bool objects, [CallerFilePath] string caller = "")
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "ZigComptimeLabeledExtent", fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])], zigLibDir: library,
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        if (!objects) return Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-comptime-labeled-extent-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try {
            var fragment = Path.Combine(directory, "main.csobj");
            File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
            return Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
        } finally { Directory.Delete(directory, true); }
    }
}

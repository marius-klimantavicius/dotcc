using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigConstByteArrayConcatExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Immutable_source_byte_arrays_supply_exact_concat_bytes_without_replacing_storage(bool objects)
    {
        var emitted = Emit("positive.zig", objects);
        var result = FixtureRunner.CompileAndRunCapturingStreams(emitted
            + "\npublic static class ConstByteArrayConcatHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
        result.exit.ShouldBe(42); result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false, "mutable.zig")]
    [InlineData(true, "mutable.zig")]
    [InlineData(false, "runtime.zig")]
    [InlineData(true, "runtime.zig")]
    public void Mutable_or_runtime_array_storage_cannot_supply_comptime_concat_bytes(bool objects, string fixture)
        => Should.Throw<CompileException>(() => Emit(fixture, objects)).Message.ShouldContain("a ++ b");

    private static string Emit(string fixture, bool objects, [CallerFilePath] string caller = "")
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "ZigConstByteArrayConcat", fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])], zigLibDir: library,
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        if (!objects) return Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-const-byte-array-concat-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try {
            var fragment = Path.Combine(directory, "main.csobj");
            File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
            return Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
        } finally { Directory.Delete(directory, true); }
    }
}

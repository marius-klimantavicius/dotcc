using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigEnumTypeReceiverExecutionTests
{
    [Theory]
    [InlineData(false, "positive.zig")]
    [InlineData(true, "positive.zig")]
    [InlineData(false, "nonexhaustive.zig")]
    [InlineData(true, "nonexhaustive.zig")]
    [InlineData(false, "comptime-enum-copy.zig")]
    [InlineData(true, "comptime-enum-copy.zig")]
    [InlineData(false, "dependent-field-name.zig")]
    [InlineData(true, "dependent-field-name.zig")]
    [InlineData(false, "comptime-type-comparison.zig")]
    [InlineData(true, "comptime-type-comparison.zig")]
    [InlineData(false, "storage-reduce-or.zig")]
    [InlineData(true, "storage-reduce-or.zig")]
    [InlineData(false, "wide-constant-peer.zig")]
    [InlineData(true, "wide-constant-peer.zig")]
    [InlineData(false, "comptime-module-arrays.zig")]
    [InlineData(true, "comptime-module-arrays.zig")]
    [InlineData(false, "comptime-mode-operand.zig")]
    [InlineData(true, "comptime-mode-operand.zig")]
    [InlineData(false, "empty-packed-pointer.zig")]
    [InlineData(true, "empty-packed-pointer.zig")]
    public void Named_enum_value_type_methods_keep_declaring_scope_and_logical_widths(bool objects, string fixture)
    {
        var emitted = Emit(fixture, objects);
        var result = FixtureRunner.CompileAndRunCapturingStreams(emitted
            + "\npublic static class EnumTypeReceiverHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
        result.exit.ShouldBe(42); result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false, "missing.zig")]
    [InlineData(true, "missing.zig")]
    [InlineData(false, "missing-field-name.zig")]
    [InlineData(true, "missing-field-name.zig")]
    public void Missing_enum_values_or_fields_do_not_get_a_type(bool objects, string fixture)
        => Should.Throw<CompileException>(() => Emit(fixture, objects));

    private static string Emit(string fixture, bool objects, [CallerFilePath] string caller = "")
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "ZigEnumTypeReceivers", fixture);
        var options = new ZigCompilationOptions("root", [new("root", source, [])], zigLibDir: library,
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        if (!objects) return Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-enum-type-receiver-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try {
            var fragment = Path.Combine(directory, "main.csobj");
            File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
            return Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
        } finally { Directory.Delete(directory, true); }
    }
}

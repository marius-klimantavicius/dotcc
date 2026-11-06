using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigArrayDataPointerExecutionTests
{
    [Theory]
    [InlineData(false, "positive.zig")]
    [InlineData(true, "positive.zig")]
    [InlineData(false, "string-array-copy.zig")]
    [InlineData(true, "string-array-copy.zig")]
    [InlineData(false, "string-concat.zig")]
    [InlineData(true, "string-concat.zig")]
    [InlineData(false, "optional-callback.zig")]
    [InlineData(true, "optional-callback.zig")]
    [InlineData(false, "optional-container-slice.zig")]
    [InlineData(true, "optional-container-slice.zig")]
    [InlineData(false, "large-splat.zig")]
    [InlineData(true, "large-splat.zig")]
    [InlineData(false, "array-row-copy.zig")]
    [InlineData(true, "array-row-copy.zig")]
    public void Source_array_pointer_data_preserves_sentinel_aliasing_and_one_evaluation(bool objects, string fixture)
    {
        var result = Emit(Fixture(fixture), objects);
        var execution = FixtureRunner.CompileAndRunCapturingStreams(result
            + "\npublic static class ArrayDataPointerHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
        execution.exit.ShouldBe(42); execution.stdout.ShouldBeEmpty(); execution.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false, "plain-array.zig", "array has no field 'ptr'")]
    [InlineData(true, "plain-array.zig", "array has no field 'ptr'")]
    [InlineData(false, "write-const.zig", "const")]
    [InlineData(true, "write-const.zig", "const")]
    [InlineData(false, "string-array-length.zig", "declared length/sentinel")]
    [InlineData(true, "string-array-length.zig", "declared length/sentinel")]
    public void A_plain_array_and_const_data_stores_remain_invalid(bool objects, string fixture, string diagnostic)
    {
        var source = Fixture(fixture);
        var options = Options(source);
        Should.Throw<CompileException>(() => Emit(source, objects, options)).Message.ShouldContain(diagnostic);
    }

    private static string Emit(string source, bool objects, ZigCompilationOptions? options = null)
    {
        options ??= Options(source);
        if (!objects) return Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-array-data-pointer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var fragment = Path.Combine(directory, "main.csobj");
            File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
            return Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static ZigCompilationOptions Options(string source)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        return new("root", [new("root", source, [])], outputMode: ZigSourceOutputMode.Lib,
            optimizeMode: ZigSourceOptimizeMode.Fast, zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
    }

    private static string Fixture(string file, [CallerFilePath] string caller = "")
        => Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "ZigArrayDataPointer", file);
}

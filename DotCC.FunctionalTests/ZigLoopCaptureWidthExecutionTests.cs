using System;
using System.IO;
using System.Runtime.CompilerServices;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigLoopCaptureWidthExecutionTests
{
    [Theory]
    [InlineData("field.zig", false)]
    [InlineData("field.zig", true)]
    [InlineData("call.zig", false)]
    [InlineData("call.zig", true)]
    [InlineData("nested.zig", false)]
    [InlineData("nested.zig", true)]
    [InlineData("column.zig", false)]
    [InlineData("column.zig", true)]
    public void Slice_capture_keeps_its_proven_source_element_width(string fixture, bool objects)
    {
        var path = Path.Combine(FixtureDirectory(), fixture);
        var options = Options(path);
        var directory = Path.Combine(Path.GetTempPath(), "loop-capture-width-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted
                + "\npublic static class LoopCaptureWidthHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Genuinely_wide_slice_capture_cannot_narrow_without_a_cast(bool objects)
    {
        var path = Path.Combine(FixtureDirectory(), "wide-rejected.zig");
        var options = Options(path);
        var error = Should.Throw<CompileException>(() =>
        {
            if (objects) Compiler.EmitObject(path, zigOptions: options);
            else Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
        });
        error.Message.ShouldContain("expected type 'u21', found 'u32'");
    }

    private static ZigCompilationOptions Options(string path)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        return new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: library,
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Debug,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed,
            managedRoots: ["answer"], isTest: true);
    }
    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigLoopCaptureWidth");
}

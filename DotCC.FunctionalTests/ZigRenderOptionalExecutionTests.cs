using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigRenderOptionalExecutionTests
{
    [Theory]
    [InlineData("array-payload.zig", false)]
    [InlineData("array-payload.zig", true)]
    [InlineData("equality.zig", false)]
    [InlineData("equality.zig", true)]
    [InlineData("forward-null.zig", false)]
    [InlineData("forward-null.zig", true)]
    public void Render_optional_values_keep_source_storage_equality_and_null_sinks(string fixture, bool objects)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var path = Path.Combine(FixtureDirectory(), fixture);
        var options = new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: library,
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Debug,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed,
            managedRoots: ["answer"], isTest: true);
        var directory = Path.Combine(Path.GetTempPath(), "render-optional-" + Guid.NewGuid().ToString("N"));
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
                + "\npublic static class RenderOptionalHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigRenderOptional");
}

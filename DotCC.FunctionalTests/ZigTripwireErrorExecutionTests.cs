using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigTripwireErrorExecutionTests
{
    private static string Fixture(string name, [CallerFilePath] string caller = "")
        => Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "zig-tripwire-error", name);

    [Theory]
    [InlineData(false, "tripwire.zig")]
    [InlineData(true, "tripwire.zig")]
    [InlineData(false, "void-catch.zig")]
    [InlineData(true, "void-catch.zig")]
    [InlineData(false, "void-capture.zig")]
    [InlineData(true, "void-capture.zig")]
    [InlineData(false, "concat.zig")]
    [InlineData(true, "concat.zig")]
    public void Reflected_error_aliases_and_void_catch_preserve_identity_and_effects(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var path = Fixture(fixture);
        var options = new ZigCompilationOptions("root", [new("root", path, [])], outputMode: ZigSourceOutputMode.Lib,
            optimizeMode: ZigSourceOptimizeMode.Fast, zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-tripwire-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "tripwire.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted
                + "\npublic static class TripwireHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
}

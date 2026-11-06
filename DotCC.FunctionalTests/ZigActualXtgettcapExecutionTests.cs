using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigActualXtgettcapExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Actual_terminfo_builder_preserves_response_bytes_sentinels_and_runtime_lookup(bool objects)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        var source = Environment.GetEnvironmentVariable("DOTCC_GHOSTTY_SOURCE");
        if (string.IsNullOrEmpty(library) || string.IsNullOrEmpty(source))
            Assert.Skip("Set DOTCC_TARGET_ZIG_LIB and DOTCC_GHOSTTY_SOURCE to the pinned Zig library and terminfo/Source.zig.");
        var path = Fixture();
        var options = new ZigCompilationOptions("root", [new("root", path, [new("source", "source")]), new("source", source, [])],
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed,
            managedRoots: ["answer"]);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-xtgettcap-" + Guid.NewGuid().ToString("N"));
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
            emitted += "\npublic static class XtgettcapHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixture([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigSourceFileType", "actual-xtgettcap.zig");
}

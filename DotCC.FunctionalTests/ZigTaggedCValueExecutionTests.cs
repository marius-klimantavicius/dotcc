using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigTaggedCValueExecutionTests
{
    [Theory]
    [InlineData(false, "custom-conversion.zig")]
    [InlineData(true, "custom-conversion.zig")]
    [InlineData(false, "lazy-membership.zig")]
    [InlineData(true, "lazy-membership.zig")]
    public void Imported_custom_payload_conversion_observes_source_method_declarations(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var repository = Repository();
        var source = Path.Combine(repository, "DotCC.FunctionalTests", "Fixtures", "ZigTaggedCValue", fixture);
        var upstream = Path.Combine(repository, "libghostty", "ref", "ghostty-93967b7f9478e94dcbb42125c8a90f34feb40d75", "src", "lib", "union.zig");
        var options = new ZigCompilationOptions("root",
            [new("root", source, [new("unions", "unions")]), new("unions", upstream, [])],
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zig-tagged-cvalue-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
            emitted += "\npublic static class TaggedCValueHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Repository([CallerFilePath] string file = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, ".."));
}

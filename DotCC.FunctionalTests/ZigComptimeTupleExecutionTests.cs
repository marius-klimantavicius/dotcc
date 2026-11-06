using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigComptimeTupleExecutionTests
{
    [Theory]
    [InlineData(false, "comptime-tuple-format.zig")]
    [InlineData(true, "comptime-tuple-format.zig")]
    [InlineData(false, "reflection-array-child.zig")]
    [InlineData(true, "reflection-array-child.zig")]
    [InlineData(false, "comptime-mode-validation.zig")]
    [InlineData(true, "comptime-mode-validation.zig")]
    public void Mixed_tuples_preserve_constant_values_and_runtime_effects_through_real_writer(bool objects, string entry)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-comptime-tuple-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Fixture(entry);
            ZigModuleDefinition[] modules = entry == "comptime-mode-validation.zig"
                ? [new("root", path, [new("modes", "modes")]), new("modes", Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, "../../../libghostty/ref/ghostty-93967b7f9478e94dcbb42125c8a90f34feb40d75/src/terminal/modes.zig")), [])]
                : [new("root", path, [])];
            var options = new ZigCompilationOptions("root", modules,
                outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
                zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
                functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
            emitted += "\npublic static class CallbackHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixture(string entry, [CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigSourceFileType", entry);
}

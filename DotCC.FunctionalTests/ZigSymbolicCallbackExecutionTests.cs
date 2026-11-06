using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigSymbolicCallbackExecutionTests
{
    [Theory]
    [InlineData(false, "symbolic-callback.zig")]
    [InlineData(true, "symbolic-callback.zig")]
    [InlineData(false, "base64-codecs.zig")]
    [InlineData(true, "base64-codecs.zig")]
    [InlineData(false, "runtime-tag-name.zig")]
    [InlineData(true, "runtime-tag-name.zig")]
    [InlineData(false, "constant-record-condition.zig")]
    [InlineData(true, "constant-record-condition.zig")]
    public void Known_callbacks_keep_identity_and_execute_in_constant_and_runtime_values(bool objects, string entry)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-symbolic-callback-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Fixture(entry);
            var options = new ZigCompilationOptions("root", [new("root", path, [])],
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

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigReflectionReturnExecutionTests
{
    [Theory]
    [InlineData(false, "reflected.zig")]
    [InlineData(true, "reflected.zig")]
    [InlineData(false, "local-struct.zig")]
    [InlineData(true, "local-struct.zig")]
    [InlineData(false, "discarded.zig")]
    [InlineData(true, "discarded.zig")]
    public void Source_reflection_helpers_execute_validations_and_keep_defining_scope(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(FixtureDirectory(), fixture);
        var options = Options(source, library);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-reflection-return-" + Guid.NewGuid().ToString("N"));
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
            emitted += "\npublic static class ReflectionReturnHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false, "plain.zig", "metadata requires a packed struct")]
    [InlineData(true, "plain.zig", "metadata requires a packed struct")]
    [InlineData(false, "wrong-count.zig", "expects 1 arguments")]
    [InlineData(true, "wrong-count.zig", "expects 1 arguments")]
    [InlineData(false, "wrong-type.zig", "type")]
    [InlineData(true, "wrong-type.zig", "type")]
    [InlineData(false, "discarded-plain.zig", "metadata requires a packed struct")]
    [InlineData(true, "discarded-plain.zig", "metadata requires a packed struct")]
    public void Invalid_reflection_calls_and_source_validation_failures_are_rejected(bool objects, string fixture, string message)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(FixtureDirectory(), fixture);
        Should.Throw<CompileException>(() =>
        {
            if (objects) Compiler.EmitObject(source, zigOptions: Options(source, library));
            else Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: Options(source, library));
        }).Message.ShouldContain(message);
    }
    private static ZigCompilationOptions Options(string source, string library)
        => new("root", [new("root", source, [])], outputMode: ZigSourceOutputMode.Lib,
            optimizeMode: ZigSourceOptimizeMode.Fast, zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigReflectionReturn");
}

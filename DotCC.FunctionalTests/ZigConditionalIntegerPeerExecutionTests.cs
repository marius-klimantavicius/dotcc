using System;
using System.IO;
using System.Runtime.CompilerServices;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigConditionalIntegerPeerExecutionTests
{
    [Theory]
    [InlineData("runtime.zig", false)]
    [InlineData("runtime.zig", true)]
    [InlineData("logical-width.zig", false)]
    [InlineData("logical-width.zig", true)]
    [InlineData("hoisted.zig", false)]
    [InlineData("hoisted.zig", true)]
    public void Conditional_literal_adopts_proven_integer_peer_with_lazy_arms(string fixture, bool objects)
    {
        var path = Path.Combine(FixtureDirectory(), fixture);
        var options = Options(path);
        var directory = Path.Combine(Path.GetTempPath(), "conditional-integer-peer-" + Guid.NewGuid().ToString("N"));
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
                + "\npublic static class ConditionalIntegerPeerHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Conditional_constant_must_fit_its_declared_integer_peer(bool objects)
    {
        var path = Path.Combine(FixtureDirectory(), "out-of-range.zig");
        var options = Options(path);
        var error = Should.Throw<CompileException>(() =>
        {
            if (objects) Compiler.EmitObject(path, zigOptions: options);
            else Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
        });
        error.Message.ShouldContain("type 'u8' cannot represent integer value '256'");
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
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigConditionalIntegerPeers");
}

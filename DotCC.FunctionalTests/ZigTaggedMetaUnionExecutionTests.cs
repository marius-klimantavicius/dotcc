using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigTaggedMetaUnionExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_alignment_one_on_type_payloads_preserves_source_metadata(bool objects)
    {
        var source = Path.Combine(FixtureDirectory(), "metadata.zig");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-tagged-meta-union-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: Options(source)));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: Options(source));
            emitted += "\npublic static class TaggedMetaUnionHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Type_payload_unions_cannot_escape_into_C_signatures(bool objects)
    {
        var source = Path.Combine(FixtureDirectory(), "runtime-escape.zig");
        Should.Throw<CompileException>(() =>
        {
            if (objects) Compiler.EmitObject(source, zigOptions: Options(source));
            else Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: Options(source));
        }).Message.ShouldContain("comptime-only");
    }

    private static ZigCompilationOptions Options(string source)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        return new("root", [new("root", source, [])], outputMode: ZigSourceOutputMode.Lib,
            optimizeMode: ZigSourceOptimizeMode.Fast, zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
    }
    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigTaggedMetaUnion");
}

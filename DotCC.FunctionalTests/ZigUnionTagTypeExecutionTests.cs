using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigUnionTagTypeExecutionTests
{
    [Theory]
    [InlineData(false, "tagged.zig")]
    [InlineData(true, "tagged.zig")]
    [InlineData(false, "untagged.zig")]
    [InlineData(true, "untagged.zig")]
    public void Optional_tag_types_preserve_identity_values_and_selected_arm_effects(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(FixtureDirectory(), fixture);
        var options = Options(source, library);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-union-tag-type-" + Guid.NewGuid().ToString("N"));
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
            emitted += "\npublic static class UnionTagTypeHost { public static int Main() => DotCcLib.answer(); }";
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
    public void Captured_types_do_not_escape_their_branch_scope(bool objects)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Path.Combine(FixtureDirectory(), "leaked-capture.zig");
        Should.Throw<CompileException>(() =>
        {
            if (objects) Compiler.EmitObject(source, zigOptions: Options(source, library));
            else Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: Options(source, library));
        }).Message.ShouldContain("Captured");
    }

    private static ZigCompilationOptions Options(string source, string library)
        => new("root", [new("root", source, [])], outputMode: ZigSourceOutputMode.Lib,
            optimizeMode: ZigSourceOptimizeMode.Fast, zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigUnionTagType");
}

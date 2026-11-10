using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigOptionalStorageVectorExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Wide_optional_vectors_keep_source_extent_alignment_and_independent_values(bool objects)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-optional-vector-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Fixture();
            var options = new ZigCompilationOptions("root", [new("root", path, [])],
                zigLibDir: library, outputMode: ZigSourceOutputMode.Lib, stdlibTypes: ZigStdlibTypeModel.Source,
                functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: ["answer"]);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted
                + "\npublic static class OptionalVectorHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixture([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigWideVectors", "optional-storage.zig");
}

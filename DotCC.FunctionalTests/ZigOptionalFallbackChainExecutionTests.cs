using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigOptionalFallbackChainExecutionTests
{
    [Theory]
    [InlineData(false, false, "optional-fallback-chain.zig")]
    [InlineData(true, false, "optional-fallback-chain.zig")]
    [InlineData(false, true, "optional-fallback-chain.zig")]
    [InlineData(true, true, "optional-fallback-chain.zig")]
    [InlineData(false, false, "bit-count-divisions.zig")]
    [InlineData(true, false, "bit-count-divisions.zig")]
    [InlineData(false, true, "bit-count-divisions.zig")]
    [InlineData(true, true, "bit-count-divisions.zig")]
    public void Lz4_optional_fallback_and_count_division_reductions_execute(bool objects, bool source, string fixture)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-optional-chain-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(Path.GetDirectoryName(Fixture())!, fixture);
            var library = Path.Combine(directory, "lib");
            Directory.CreateDirectory(Path.Combine(library, "std"));
            File.WriteAllText(Path.Combine(library, "std", "std.zig"), "");
            var options = source ? new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: library,
                stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed) : null;
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.Csproj, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixture([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigSourceFileType", "optional-fallback-chain.zig");
}

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigStorageVectorBitwiseExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Integer_storage_bitwise_preserves_all_lanes_and_ordered_value_operands(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-storage-bitwise-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var library = Path.Combine(directory, "lib");
            Directory.CreateDirectory(Path.Combine(library, "std"));
            File.WriteAllText(Path.Combine(library, "std", "std.zig"), "");
            var path = Fixture();
            var options = new ZigCompilationOptions("root", [new("root", path, [])],
                zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed);
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
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigWideVectors", "bitwise.zig");
}

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigWideVectorExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Storage_vectors_preserve_lanes_guards_value_copies_and_single_evaluation(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-wide-vector-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Fixture();
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixture([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigWideVectors", "storage.zig");
}

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigWrappingAssignmentExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Logical_width_wrapping_preserves_evaluation_and_storage(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-wrapping-assignments-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Fixture();
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Fixture([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigSourceFileType", "wrapping-assignments.zig");
}

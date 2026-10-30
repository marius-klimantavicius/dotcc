using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigBitcastWidthExecutionTests
{
    [Theory]
    [InlineData("scalar-widths.zig", false)]
    [InlineData("scalar-widths.zig", true)]
    [InlineData("result-locations.zig", false)]
    [InlineData("result-locations.zig", true)]
    [InlineData("array-storage.zig", false)]
    [InlineData("array-storage.zig", true)]
    [InlineData("once-lazy.zig", false)]
    [InlineData("once-lazy.zig", true)]
    [InlineData("promotion-and-float.zig", false)]
    [InlineData("promotion-and-float.zig", true)]
    [InlineData("../ZigBitcastWidthR38/full-width.zig", false)]
    [InlineData("../ZigBitcastWidthR38/full-width.zig", true)]
    public void Logical_widths_and_transport_match_pinned_native_values(string fixture, bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-bitcast-width-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.Copy(Path.Combine(FixtureDirectory(), fixture), path);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigBitcastWidthR37");
}

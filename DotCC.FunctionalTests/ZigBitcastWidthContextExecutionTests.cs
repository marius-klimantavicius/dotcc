using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;
namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigBitcastWidthContextExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Declaration_scopes_and_generic_instances_keep_logical_shapes(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-width-context-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            foreach (var file in Directory.GetFiles(FixtureDirectory(), "*.zig"))
                File.Copy(file, Path.Combine(directory, Path.GetFileName(file)));
            var path = Path.Combine(directory, "contexts.zig");
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
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigBitcastWidthR38");
}

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigTaggedUnionReifiedExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Imported_and_union_scoped_enum_aliases_preserve_layout_holes_and_payload_execution(bool objects)
    {
        var fixture = FixtureDirectory();
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-union-tag-reified-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            foreach (var source in Directory.GetFiles(fixture, "*.zig", SearchOption.AllDirectories))
            {
                var destination = Path.Combine(directory, Path.GetRelativePath(fixture, source));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(source, destination);
            }
            var path = Path.Combine(directory, "main.zig");
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.object.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { fragment }, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigTaggedUnionReified");
}

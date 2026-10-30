using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigDeferredTypeAliasExecutionTests
{
    [Theory]
    [InlineData(false, "main.zig")]
    [InlineData(true, "main.zig")]
    [InlineData(false, "runtime.zig")]
    [InlineData(true, "runtime.zig")]
    public void Forward_aliases_and_anonymous_contexts_preserve_captures_members_and_identity(bool objects, string entry)
    {
        var fixture = FixtureDirectory();
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-deferred-type-alias-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            foreach (var source in Directory.GetFiles(fixture, "*.zig", SearchOption.AllDirectories))
            {
                var destination = Path.Combine(directory, Path.GetRelativePath(fixture, source));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(source, destination);
            }
            var path = Path.Combine(directory, entry);
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
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigDeferredTypeAlias");
}

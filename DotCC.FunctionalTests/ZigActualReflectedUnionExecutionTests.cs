using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigActualReflectedUnionExecutionTests
{
    [Theory]
    [InlineData(false, "main.zig")]
    [InlineData(true, "main.zig")]
    [InlineData(false, "inline.zig")]
    [InlineData(true, "inline.zig")]
    public void Original_tagged_union_reflection_preserves_layout_payloads_and_custom_C_conversion(bool objects, string entry)
    {
        var root = Repository();
        var path = Path.Combine(root, "DotCC.FunctionalTests", "Fixtures", "ZigActualReflectedUnion", entry);
        var upstream = Path.Combine(root, "libghostty", "ref", "ghostty-93967b7f9478e94dcbb42125c8a90f34feb40d75", "src", "lib", "union.zig");
        var options = new ZigCompilationOptions("root", [new("root", path, [new("unions", "unions")]), new("unions", upstream, [])]);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-actual-reflected-union-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.Csproj, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>(), TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
    private static string Repository([CallerFilePath] string file = "") => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, ".."));
}

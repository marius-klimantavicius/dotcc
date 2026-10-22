using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class CompilerBoundaryExecutionTests
{
    [Theory]
    [InlineData("block-extern-objects", false, false)]
    [InlineData("block-extern-objects", false, true)]
    [InlineData("block-extern-objects", true, false)]
    [InlineData("block-extern-objects", true, true)]
    [InlineData("pointer-to-array-casts", false, false)]
    [InlineData("pointer-to-array-casts", true, false)]
    public void Extern_objects_and_row_pointer_casts_preserve_storage_and_stride(string name, bool objects, bool reverse)
    {
        var fixture = FixtureRunner.Discover().Single(f => f.name == name);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-boundary-objects-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var sources = fixture.sources.ToArray();
            if (reverse) Array.Reverse(sources);
            string generated;
            if (objects)
            {
                var paths = sources.Select((source, i) =>
                {
                    var output = Path.Combine(directory, i + ".o");
                    File.WriteAllText(output, Compiler.EmitObject(source));
                    return output;
                }).ToArray();
                generated = Compiler.LinkObjects(paths);
            }
            else generated = Compiler.EmitCSharp(sources);
            var observed = FixtureRunner.CompileAndRunCapturingExit(generated, Array.Empty<string>());
            observed.exit.ShouldBe(0);
            observed.stdout.Trim().ShouldBe(fixture.expectedStdout.Trim());
        }
        finally { Directory.Delete(directory, true); }
    }
}
